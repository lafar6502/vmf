using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using NLog;

namespace VMF.Core.Util
{
    public class JsonTranslationFile : ITextTranslation
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Dictionary<string, string[]> _translations;
        private DateTime _readDate;
        private DateTime _lastCheck = DateTime.Now;
        private string _fileName;
        private string[] _langs;
        public JsonTranslationFile(string path, string[] langs = null)
        {
            var d = LoadFile(path);
            if (langs == null)
            {
                if (!d.ContainsKey("_languages")) throw new Exception("_languages missing in " + path);
                _langs = d["_languages"];
            }
            else
            {
                _langs = langs;
            }
            _translations = d;
            _readDate = DateTime.Now;
        }

        public IEnumerable<string> Languages => throw new NotImplementedException();

        public event Action<ITextTranslation> TranslationsChanged;

        private void CheckReload()
        {
            if (DateTime.Now.AddMinutes(1) > _lastCheck) return;
            var d0 = File.GetLastWriteTime(_fileName);
            _lastCheck = DateTime.Now;
            if (d0 > _readDate)
            {
                var d = LoadFile(_fileName);
                _translations = d;
                _readDate = DateTime.Now;
                if (d.ContainsKey("_languages"))
                {
                    _langs = d["_languages"];
                }
                log.Info("Reloaded translation file {0}", _fileName);
                if (TranslationsChanged != null) TranslationsChanged(this);
            }

        }
        public string Get(string id, string language)
        {
            CheckReload();
            string[] v;
            var idx = Array.IndexOf(_langs, language);
            if (idx < 0) throw new Exception("Unknown language:" + language);
            if (_translations.TryGetValue(id, out v))
            {
                return v[idx];
            }
            return null;
        }

        public string GetFirst(IEnumerable<string> ids, string language)
        {
            CheckReload();
            var idx = Array.IndexOf(_langs, language);
            if (idx < 0) throw new Exception("Unknown language:" + language);
            Dictionary<string, string[]> d = _translations;
            string[] v;
            foreach (var id in ids)
            {
                if (d.TryGetValue(id, out v) && v[idx] != null)
                {
                    return v[idx];
                }
            }
            return null;
        }

        public void Set(string id, string text, string language)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, string[]> LoadFile(string fn)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(fn, Encoding.UTF8));
        }

        /// <summary>
        /// saves a normalized translation file
        /// </summary>
        /// <param name="path"></param>
        public void SaveFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}
