using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NLog;
using Newtonsoft.Json;

namespace VMF.Core
{

    public class I18N1
    {
        private string[] _languages { get; set; }
        private Dictionary<string, string[]> _cache = null;
        private Dictionary<string, string[]> _overrides = null;
        private string _inputFile;
        private string _overrideFile = null;
        private DateTime _loadDate = DateTime.MinValue;

        private static Logger log = LogManager.GetCurrentClassLogger();
        private static I18N1 _instance = null;
        private HashSet<string> _notFound = new HashSet<string>();
        public I18N1(string inputFile, string overrideFile = null)
        {
            _inputFile = inputFile;
            _overrideFile = overrideFile;
        }

        public static I18N1 GetInstance()
        {
            var i = _instance;
            if (i == null)
            {
                
                
                var f = AppDomain.CurrentDomain.BaseDirectory;
                var overrideFile = Path.Combine(f, "App_Data\\lang_overrides.json");
                var ifn = Path.Combine(f, "lang.json");
                
                i = new I18N1(ifn, overrideFile);
                lock (typeof(I18N1))
                {
                    if (_instance == null)
                    {
                        _instance = i;
                    }
                }
            }
            return i;
        }

        public static void SetInstance(I18N1 inst)
        {
            _instance = inst;
        }


        public static string GetString(IEnumerable<string> symbols, string lang)
        {
            var i = GetInstance();
            return i.Get(symbols, lang);
        }

        public static string GetString(IEnumerable<string> symbols)
        {
            var i = GetInstance();
            return i.Get(symbols, null);
        }

        public static string TryGetString(IEnumerable<string> symbols, string defaultText, string lang)
        {
            var i = GetInstance();
            return i.TryGet(symbols, defaultText, lang);
        }

        public static string TryGetString(IEnumerable<string> symbols, string defaultText)
        {
            var i = GetInstance();
            return i.TryGet(symbols, defaultText, null);
        }

        public static string GetString(string id, string lang)
        {
            return GetString(new string[] { id }, lang);
        }

        public static string GetString(string id)
        {
            return GetString(new string[] { id }, null);
        }

        public static string TryGetString(string id, string defaultText, string lang)
        {
            return TryGetString(new string[] { id }, defaultText, lang);
        }

        public static string TryGetString(string id, string defaultText)
        {
            return TryGetString(new string[] { id }, defaultText, null);
        }

        public static string FormatDate(DateTime? dt)
        {
            if (!dt.HasValue) return "";
            var str = GetString("dateFormat");
            return dt.Value.ToString(str);
        }

        public static string FormatDateTime(DateTime? dt)
        {
            if (!dt.HasValue) return "";
            var str = GetString("dateTimeFormat");
            return dt.Value.ToString(str);
        }

        public static string FormatString(string id, params object[] args)
        {
            var s = GetString(id);
            return string.Format(s, args);
        }

        public static string DefaultLanguage
        {
            get
            {
                var defl = System.Configuration.ConfigurationManager.AppSettings["DefaultLanguage"];
                if (!string.IsNullOrEmpty(defl)) return defl;
                var i = GetInstance();
                i.LoadIfNecessary();
                return i._languages.FirstOrDefault() ?? "en";
            }
        }

        public static IEnumerable<string> AllLanguages
        {
            get
            {
                var i = GetInstance();
                i.LoadIfNecessary();
                return i._languages;
            }
        }

        public string Get(string id, string lang = null)
        {
            return Get(new string[] { id }, lang);
        }

        private static bool DebugTranslation => AppGlobal.Config.Get("UI.DebugTranslation", false);
        

        public string Get(IEnumerable<string> ids, string lang = null)
        {
            //we have inputs like Aaaaa.Bbbbbb.Ccccccc so
            //for the first one we cut off the prefix and replace it with common: common.Bbbbb.Ccccc and common.Cccccc
            var id0 = ids.FirstOrDefault();
            //we can try to skip thigs like Label, Name or List (configured)
            IEnumerable<string> skip = AppGlobal.Config != null ? AppGlobal.Config.Get<IEnumerable<string>>("UI.TranslateSkip") : null;
            if (skip != null)
            {
                foreach (var sk in skip)
                {
                    id0 = id0.EndsWith("." + sk) ? id0.Substring(0, id0.Length - (sk.Length + 1)) : id0;
                }
            }

            var ix0 = id0.IndexOf('.');
            var auto = AppGlobal.Config.Get("UI.AutoTranslate", true);
            var dt = !auto
                   ? $"[{id0}]" 
                   : (ix0 > 0 && ix0 < id0.Length - 1 ? id0.Substring(ix0 + 1) : id0);
            
            return TryGet(ids, dt, lang);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private string HandleResult(string id, string[] translations, int idx, string lang)
        {
            if (lang == "dbg" || lang == "inv" || idx < 0) return id;
            if (translations == null || translations.Length == 0) return null; 
            if (translations.Length == 1 && !DebugTranslation)
            {
                if (translations[0].StartsWith("$") && translations[0].EndsWith("$") && translations[0].Length > 2)
                {
                    var ky = translations[0].Substring(1, translations[0].Length - 2);
                    var tran = TryGet(new string[] { ky }, ky, lang);
                    if (tran != ky) return tran;
                }
            }
            if (idx >= translations.Length) return null;

            return DebugTranslation ? translations[idx] + "[" + id + "]" : translations[idx];
        }


        /// <summary>
        /// get first matching translation for a list of ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="defaultText">text to return if no translation is found</param>
        /// <param name="lang">language code. if you use 'dbg' then it will return the matching id untranslated</param>
        /// <returns></returns>
        public string TryGet(IEnumerable<string> ids, string defaultText, string lang = null)
        {
            var idz = new List<string>(ids);

            if (lang == null) lang = SessionContext.Current != null ? SessionContext.Current.Language : null;
            if (string.IsNullOrEmpty(lang)) lang = DefaultLanguage;
            LoadIfNecessary();
            var c = _cache;
            if (c.Count == 0) return ids.FirstOrDefault();
            string[] langs = c["_languages"];
            if (langs == null || langs.Length == 0) return ids.FirstOrDefault();
            int idx = Array.IndexOf(langs, lang.ToLower());
            //if (idx < 0) return ids.FirstOrDefault();
            if (lang == "inv" || lang == "dbg") return ids.FirstOrDefault();

            Func<string, string> transGet = sid =>
            {
                string[] ent;
                
                if (AppGlobal.Config != null && AppGlobal.Config.Get("UI.EnableI18NOverrideService", false))
                {
                    /*var ovr = AppGlobal.ResolveService<I18NOverride>();
                    ent = ovr.GetTranslations(sid);
                    if (ent != null && ent.Length > 0)
                    {
                        var res = HandleResult(sid, ent, idx, lang);
                        if (res != null) return res;
                    }*/
                }
                if (_overrides != null && _overrides.TryGetValue(sid, out ent))
                {
                    var res = HandleResult(sid, ent, idx, lang);
                    if (res != null) return res;
                }
                if (c.TryGetValue(sid, out ent))
                {
                    var res = HandleResult(sid, ent, idx, lang);
                    if (res != null) return res;
                }
                return null;
            };

            foreach (var id in ids)
            {
                if (string.IsNullOrEmpty(id)) continue;
                var res = transGet(id);
                if (res != null) return res;

                var ix = id.IndexOf('.');
                var ix2 = id.LastIndexOf('.', ix + 1);
                if (ix > 0)
                {
                    var id1 = "common." + id.Substring(ix + 1);
                    res = transGet(id1);
                    if (res != null) return res;
                    if (ix2 > 0)
                    {
                        var id2 = "common." + id.Substring(ix2 + 1);
                        res = transGet(id2);
                        if (res != null) return res;
                    }
                }
            }
            ids = ids.Where(x => !string.IsNullOrEmpty(x) && !_notFound.Contains(x));
            if (ids.Any())
            {
                lock(_notFound)
                {
                    foreach (var id in ids) _notFound.Add(id);
                }
            }
            
            if (lang == "inv" || lang == "dbg") return ids.FirstOrDefault();
            return DebugTranslation ? defaultText + "[!" + string.Join(",", ids) + "]" : defaultText;
        }



        private DateTime _lastCheck = DateTime.MinValue;

        private void LoadIfNecessary()
        {
            var c = _cache;
            var ovr = _overrides;
            var reload = c == null;
            var reloadOverride = !string.IsNullOrEmpty(_overrideFile) && ovr == null && File.Exists(_overrideFile);
            if (_lastCheck.AddSeconds(30) <= DateTime.Now)
            {
                if (!reload && _inputFile != null && File.Exists(_inputFile)) reload = File.GetLastWriteTime(_inputFile) >= _loadDate;
                if (!reloadOverride && _overrideFile != null && File.Exists(_overrideFile)) reloadOverride = File.GetLastWriteTime(_overrideFile) >= _loadDate;
                _lastCheck = DateTime.Now;
            }
        
            if (reload)
            {
                if (!File.Exists(_inputFile))
                {
                    c = new Dictionary<string, string[]>();
                    _languages = new string[0];
                }
                else
                {
                    c = LoadCfgFile(_inputFile);
                    _loadDate = DateTime.Now;
                    _languages = c["_languages"]; // c.GetValueOrDefault("_languages", new string[0]);
                }
                _cache = c;
            }

            if (reloadOverride)
            {
                _overrides = LoadCfgFile(_overrideFile);
                _loadDate = DateTime.Now;
            }

        }

        protected Dictionary<string, string[]> LoadCfgFile(string path, IDictionary<string, string> inputFiles = null)
        {
            var c = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(path, Encoding.UTF8));
            if (inputFiles != null)
            {
                foreach (var k in c.Keys) inputFiles[k] = path;
            }

            if (c.ContainsKey("@include"))
            {
                foreach (var ifn in c["@include"])
                {
                    var nfn = Path.Combine(Path.GetDirectoryName(_inputFile), ifn);
                    if (!File.Exists(nfn))
                    {
                        log.Warn("Include file not found: {0}", ifn);
                        continue;
                    }
                    var c2 = LoadCfgFile(nfn, inputFiles);
                    if (c2 != null)
                    {
                        foreach (var kv in c2)
                        {
                            c[kv.Key] = kv.Value;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(AppGlobal.AppProfile))
            {
                var fn = Path.GetFileNameWithoutExtension(path);
                var nfn = fn + "." + AppGlobal.AppProfile + ".json";
                nfn = Path.Combine(Path.GetDirectoryName(path), nfn);
                if (File.Exists(nfn))
                {
                    var c3 = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(nfn, Encoding.UTF8));
                    if (c3 != null)
                    {
                        foreach (var kv in c3)
                        {
                            c[kv.Key] = kv.Value;
                            if (inputFiles != null) inputFiles[kv.Key] = nfn;
                        }
                    }
                }
            }
            return c;
        }


        protected Dictionary<string, string[]> LoadOverride(string inputFile, string overrideName)
        {
            var fn = Path.GetFileNameWithoutExtension(inputFile);
            var nfn = fn + "." + overrideName + ".json";
            nfn = Path.Combine(Path.GetDirectoryName(inputFile), nfn);
            
            if (!File.Exists(nfn))
            {
                log.Debug("Overridden translation doesn't exist: {0}", nfn);
                return null;
            }
            log.Info("Loading translation override from {0}", nfn);
            return JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(nfn, Encoding.UTF8));
        }

        public static IEnumerable<string> NotFoundTranslations => GetInstance()._notFound;

        public static void SetTranslation(string id, string lang, string text)
        {
            GetInstance().SetTranslationOverride(id, lang, text);
        }

        public void SetTranslationOverride(string id, string lang, string text)
        {
            /*if (AppGlobal.Config != null && AppGlobal.Config.Get("UI.EnableI18NOverrideService", false))
            {
                var sv = AppGlobal.ResolveService<I18NOverride>();
                sv.Update(id, lang, text);
                return;
            }*/

            var ovr = _overrides;
            if (ovr == null) ovr = new Dictionary<string, string[]>();
            var idx = Array.IndexOf(_languages, lang.ToLowerInvariant());
            if (idx < 0) throw new Exception("lang invalid; " + lang);
            string[] s;
            if (!ovr.TryGetValue(id, out s))
            {
                s = new string[_languages.Length];
                ovr[id] = s; 
            }
            if (s.Length < _languages.Length)
            {
                var s2 = new string[_languages.Length];
                Array.Copy(s, 0, s2, 0, s.Length);
                s = s2;
                ovr[id] = s;
            }
            s[idx] = string.IsNullOrEmpty(text) ? null : text;
            lock(this)
            {
                if (ovr != _overrides) _overrides = ovr;
                File.WriteAllText(_overrideFile, JsonConvert.SerializeObject(_overrides), Encoding.UTF8);
                _loadDate = DateTime.Now;
            }
        }

        public static void MergeOverrides(string[] languagesToMerge)
        {
            GetInstance().MergeOverrides(null, languagesToMerge);
        }
        public void MergeOverrides(string overrideFile, string[] languagesToMerge)
        {
            if (overrideFile == null) overrideFile = _overrideFile;
            var ovrDic = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(overrideFile, Encoding.UTF8));
            if (ovrDic == null || !ovrDic.Any()) return;
            Dictionary<string, string> originz = new Dictionary<string, string>();
            var translations = LoadCfgFile(_inputFile, originz);
            string[] langs = translations["_languages"];
            if (langs == null) throw new Exception("No _languages");
            var idxes = languagesToMerge.Select(x => Array.IndexOf(langs, x)).ToArray();

            var updates = new Dictionary<string, string[]>();
            foreach(var kv in ovrDic)
            {
                string[] v = null;
                if (!translations.TryGetValue(kv.Key, out v))
                {
                    v = new string[langs.Length];
                }
                var updated = false;
                if (v.Length < langs.Length)
                {
                    var s2 = new string[langs.Length];
                    Array.Copy(v, 0, s2, 0, v.Length);
                    v = s2;
                    updated = true;
                }
                foreach(var ix in idxes)
                {
                    if (ix < 0) continue;
                    if (ix >= kv.Value.Length || kv.Value[ix] == null) continue;
                    if (string.Equals(kv.Value[ix], v[ix])) continue;
                    v[ix] = kv.Value[ix];
                    updated = true;
                }
                if (!updated) continue;
                updates[kv.Key] = v;
            }

            var grouped = updates.GroupBy(x => originz.ContainsKey(x.Key) ? originz[x.Key] : _inputFile);
            foreach(var g in grouped)
            {
                var dx = new Dictionary<string, string[]>();
                foreach (var kv in g) dx[kv.Key] = kv.Value;
                UpdateTranslationFile(g.Key, dx);
            }
        }

        public static void NormalizeTranslationFile(string fileName)
        {
            UpdateTranslationFile(fileName, new Dictionary<string, string[]>());
        }

        protected static void UpdateTranslationFile(string fileName, IDictionary<string, string[]> data)
        {
            log.Warn("UPdating translation file {0}, {1} entries", fileName, data.Count());
            var d1 = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(fileName, Encoding.UTF8));
            foreach(var kv in data)
            {
                d1[kv.Key] = kv.Value;
            }
            var content = d1.OrderBy(x => x.Key);
            SaveTranslationFile(content, fileName);
        }
        protected static void SaveTranslationFile(IEnumerable<KeyValuePair<string, string[]>> data, string fileName)
        {
            var ser = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.None
            });
            var tf = Path.GetTempFileName();
            using (var sw = new StreamWriter(tf, false, Encoding.UTF8))
            {
                var jw = new JsonTextWriter(sw);
                jw.Formatting = Formatting.Indented;
                jw.WriteStartObject();
                foreach(var kv in data)
                {
                    jw.WritePropertyName(kv.Key);
                    if (kv.Value != null)
                    {
                        ser.Serialize(jw, kv.Value);         
                    }
                    else jw.WriteNull();
                }
                jw.WriteEndObject();
                jw.Flush();
            }
            File.Delete(fileName);
            File.Move(tf, fileName);
        }

        public static void MergeLangFiles(string inputFile1, string inputFile2, string outputFile, bool ifConflictTakeSecond)
        {
            var origDic = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(inputFile1, Encoding.UTF8));
            var secondDic = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(inputFile2, Encoding.UTF8));
            
            string[] langs = origDic["_languages"];
            if (langs == null) throw new Exception("No _languages");
            Dictionary<string, int> idxes = new Dictionary<string, int>(langs.Length, StringComparer.InvariantCultureIgnoreCase);
            for (var i = 0; i < langs.Length; i++) idxes[langs[i]] = i;
            var langs2 = secondDic["_languages"];
            if (langs2 == null) langs2 = langs;
            Dictionary<string, int> idx2 = new Dictionary<string, int>(langs2.Length, StringComparer.InvariantCultureIgnoreCase);
            for (var i =0; i<langs2.Length; i++)
            {
                if (!idxes.ContainsKey(langs2[i])) idx2[langs2[i]] = langs.Length + i;
                idx2[langs2[i]] = i;
            }
            
            var rezultat = new Dictionary<string, string[]>();
            foreach(var kv in origDic)
            {
                string[] l2;
                if (!secondDic.TryGetValue(kv.Key, out l2)) l2 = null;
                var v = new string[idxes.Count];
                foreach(var lv in idxes)
                {
                    var t2 = l2 != null && idx2.ContainsKey(lv.Key) && idx2[lv.Key] < l2.Length ? l2[idx2[lv.Key]] : null;
                    var t1 = kv.Value != null && lv.Value <  kv.Value.Length && lv.Value < langs.Length ? kv.Value[lv.Value] : null;
                    v[lv.Value] = ifConflictTakeSecond ? (string.IsNullOrEmpty(t2) ? t1 : t2) : (string.IsNullOrEmpty(t1) ? t2 : t1);
                }
                rezultat[kv.Key] = v;
                secondDic.Remove(kv.Key);
            }
            foreach(var kv in secondDic)
            {
                var v = new string[idxes.Count];
                foreach(var lv in idx2)
                {
                    var x1 = idxes[lv.Key];
                    v[x1] = lv.Value < kv.Value.Length ? kv.Value[lv.Value] : null;
                }
                rezultat[kv.Key] = v;
            }
            SaveTranslationFile(rezultat.OrderBy(x => x.Key), outputFile);
        }



    }

    
    


}
