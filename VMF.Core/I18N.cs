using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VMF.Core
{
    /// <summary>
    /// 3 -level structure
    /// local translations (user provided)
    /// app - profile translations
    /// global 
    /// need text files for easy versioning
    /// </summary>
    public class I18N
    {
        private ITextTranslation[] _sources;

        public I18N()
        {
            var bd = AppDomain.CurrentDomain.BaseDirectory;
            var pth = Path.Combine(bd, "i18n.json");
            var d0 = new Util.JsonTranslationFile(pth);
            var pth2 = Path.Combine(bd, "i18n." + AppGlobal.AppProfile + ".json");
            var d1 = new Util.JsonTranslationFile(pth2);
            _sources = new ITextTranslation[] { d1, d0 };
        }
        public static string Get(string id, string defaultText)
        {
            return DefaultInstance.TryGet(id, defaultText);
        }


        public static string Get(string id, string defaultText, string lang)
        {
            return DefaultInstance.TryGet(new string[] { id }, defaultText, lang);
        }


        public static string Get(string id)
        {
            return Get(id, null);
        }

        /// <summary>
        /// return first matching translation for given list of ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="defaultText"></param>
        /// <returns></returns>
        public static string Get(IEnumerable<string> ids, string defaultText)
        {
            return Get(ids, defaultText, SessionContext.Current.Language);
        }

        public static string Get(IEnumerable<string> ids, string defaultText, string lang)
        {
            return DefaultInstance.TryGet(ids, defaultText, lang);
        }

        /// <summary>
        /// try get translation but only if it exists. returns defaultText if not found
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="defaultText"></param>
        /// <returns></returns>
        public string TryGet(IEnumerable<string> ids, string defaultText, string lang)
        {
            foreach(var id in ids)
            {
                foreach(var st in _sources)
                {
                    var s = st.Get(id, lang);
                    if (s != null) return s;
                }
            }
            return defaultText;
        }

        public string TryGet(IEnumerable<string> ids, string defaultText)
        {
            return TryGet(ids, defaultText, SessionContext.Current.Language);
        }

        public  string TryGet(string id, string defaultText = null)
        {
            return TryGet(new String[] { id }, defaultText);
        }

        public string FormatString(string id, params object[] args)
        {
            var s = TryGet(new String[] { id }, null);
            if (s == null) return null;
            return string.Format(s, args);
        }


        public static I18N DefaultInstance { get; set; }

        
    }
}
