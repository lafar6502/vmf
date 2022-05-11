using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NLog;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Configuration;
using VMF.Core.Util;

namespace VMF.Core.Config
{
    /// <summary>
    /// json config class
    /// 
    /// config loading rules
    /// 1. folder: [web root]/config
    /// 2. look for files
    /// appname.machinename.json
    /// machinename.json
    /// config.json
    /// where appname can be set in the web config (appprofile)
    /// or if not specified in web.config then
    /// is a directory name above the webroot
    /// </summary>
    public class JsonConfig : IConfigProvider
    {
        private JObject _configContainer;
        private DateTime _configLoadDate = DateTime.MinValue;
        private DateTime _lastCheck = DateTime.MinValue;
        private List<string> _monitorFiles = null;
        
        private static Logger log = LogManager.GetCurrentClassLogger();


        public event Action<JsonConfig> ConfigurationChanged;

        public JsonConfig(string baseDirectory, string profileName, string machineName)
        {
            Load(baseDirectory, profileName, machineName);
        }

        public JsonConfig()
        {
            var bd = AppDomain.CurrentDomain.BaseDirectory;
            var configDir = Path.Combine(bd, "config");
            var profile = ConfigurationManager.AppSettings["ConfigProfile"];
            var fn = Path.Combine(configDir, "app.profile");
            if (File.Exists(fn))
            {
                profile = File.ReadAllText(fn);
            }
            if (string.IsNullOrEmpty(profile)) profile = Path.GetFileName(Path.GetDirectoryName(bd));
            Load(configDir, profile, Environment.MachineName);
        }

        public JsonConfig(string profileName) 
        {
            var bd = AppDomain.CurrentDomain.BaseDirectory;
            Load(Path.Combine(bd, "config"), profileName, Environment.MachineName);
        }

        private void Load(string baseDirectory, string profileName, string machineName)
        {
            log.Warn("CONFIG RELOAD {0} {1} {2}", baseDirectory, profileName, machineName);
            lock (this)
            {
                _configContainer = new JObject();
                _monitorFiles = new List<string>();
                _lastCheck = DateTime.Now;
                _configLoadDate = DateTime.Now;
                
                _configContainer["baseDir"] = AppDomain.CurrentDomain.BaseDirectory;
                _configContainer["machineName"] = machineName;
                _configContainer["appProfile"] = profileName;
                
                
                

                var files = new string[] {
                    "config.json",
                    string.Format("{0}.json", machineName),    
                    string.Format("{0}.{1}.json", profileName, machineName)                    
                };
                
                foreach (var f in files)
                {
                    var pth = Path.Combine(baseDirectory, f);
                    if (File.Exists(pth))
                    {
                        log.Info("Loading config file: {0}", pth);
                        LoadConfigFile(_configContainer, pth);
                    }
                    else
                    {
                        log.Info("Config file not provided: {0}", pth);
                    }
                }

                ExpandPropertyReferences(_configContainer);
                _lastCheck = DateTime.Now;
                _configContainer["baseDir"] = baseDirectory;
                _configContainer["machineName"] = machineName;
                _configContainer["appProfile"] = profileName;
                
            }
        }

        
       
        private void ExpandPropertyReferences(JToken t)
        {
            if (t is JArray)
            {
                var arr = t as JArray;
                foreach (var jt in arr)
                {
                    ExpandPropertyReferences(jt);
                }
            }
            else if (t is JObject)
            {
                var obj = t as JObject;
                foreach (var jt in obj)
                {
                    ExpandPropertyReferences(jt.Value);
                }
            }
            else if (t is JValue)
            {
                JValue jv = t as JValue;
                if (jv.Type == JTokenType.String)
                {
                    jv.Value = StringUtil.SubstValues(jv.Value<string>(), p => GetInternal(p, "$"+p));
                }
            }
            
        }

        


        private void LoadConfigFile(JObject obj, string path)
        {
            var t =  JObject.Load(new JsonTextReader(new StringReader(File.ReadAllText(path, Encoding.UTF8))));
            _monitorFiles.Add(path);
            var inc = t.GetValue("@@include");
            IEnumerable<string> includes = null;
            if (inc != null)
            {
                if (inc is JArray)
                {
                    includes = ((JArray)inc).Values<string>();
                }
                else if (inc is JValue)
                {
                    includes = new string[] { inc.ToObject<string>() };
                }
                else throw new Exception("invalid type of @@include: " + inc.GetType().Name);
            }
            if (includes != null)
            {
                t.Remove("@@include");
                var fp = Path.GetDirectoryName(path);
                foreach(var s in includes)
                {
                    var fn = s;
                    if (!Path.IsPathRooted(fn))
                    {
                        fn = Path.Combine(fp, s);
                    }
                    if (File.Exists(fn))
                    {
                        log.Info("Loading include file: {0}", fn);
                        LoadConfigFile(t, fn);
                    }
                    else
                    {
                        log.Warn("Include file not found: {0}", fn);
                    }
                }
            }


            obj.Merge(t, new JsonMergeSettings
            {
                MergeArrayHandling = Newtonsoft.Json.Linq.MergeArrayHandling.Replace
            });
        }


        private static JToken GetJsonValue(JToken root, string path)
        {
            if (root.Type != JTokenType.Object && root.Type != JTokenType.Array)
            {
                return null; 
            }
            int idx = path.IndexOf('.');
            string segment = idx > 0 ? path.Substring(0, idx) : path;
            var t = root[segment];
            if (t == null) return null;
            

            if (idx > 0) 
            {
                return GetJsonValue(t, path.Substring(idx + 1));
            } 
            else 
            {
                return t;
            }
        }

        protected void ReloadIfNecessary()
        {
            var modified = false;
            lock(this)
            {
                if (_lastCheck.AddSeconds(30) >= DateTime.Now) return;
                modified = _monitorFiles.Any(x => File.GetLastWriteTime(x) >= _configLoadDate);
                _lastCheck = DateTime.Now;
                if (!modified) return;
                var bd = _configContainer.GetValue("baseDir").Value<string>();
                var mn = _configContainer.GetValue("machineName").Value<string>();
                var ap = _configContainer.GetValue("appProfile").Value<string>();
                Load(bd, ap, mn);
                _lastCheck = DateTime.Now;
            }
            if (modified && ConfigurationChanged != null)
            {
                ConfigurationChanged(this);
            }
        }
        protected T GetInternal<T>(string name, T defaultValue)
        {
            if (_lastCheck.AddSeconds(30) < DateTime.Now)
            {
                ReloadIfNecessary();
            }
            var v = _configContainer.GetValue(name);
            if (v == null)
            {
                v = GetJsonValue(_configContainer, name);
            }
            if (defaultValue != null && v != null)
            {
                return (T) v.ToObject(defaultValue.GetType());
            }
            return v == null ? defaultValue : v.ToObject<T>();

        }
        
        public T Get<T>(string name, T defaultValue)
        {
            return GetInternal(name, defaultValue);
        }

        public T Get<T>(string name)
        {
            return Get(name, default(T));
        }

        /// <summary>
        /// Initialize properties of passed object
        /// to values retrieved from json JObject
        /// configValue must point to an object
        /// </summary>
        /// <param name="configValue"></param>
        /// <param name="target"></param>
        public void SetProperties(string configValue, object target)
        {
            var oo = Get<JObject>(configValue);
            if (oo == null) return;
            if (AppGlobal.Container == null || AppGlobal.Config == null)
            {
                
                oo.AssignTo(target);
                return;
            }

            var json = oo.ToString();
            var ss = new JsonSerializerSettings();
            //var c0 = new Serialization.EntityRefConverter(AppGlobal.ResolveService<IEntityResolver>());
            //ss.Converters.Add(c0);
            ss.MissingMemberHandling = MissingMemberHandling.Ignore;
            var ser = JsonSerializer.Create(ss);
            oo.AssignTo(target, ser);
        }



    }
}
