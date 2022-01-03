using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static string Get(string id, string defaultText)
        {
            return id;
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

        }

        /// <summary>
        /// try get translation but only if it exists. returns defaultText if not found
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="defaultText"></param>
        /// <returns></returns>
        public static string TryGet(IEnumerable<string> ids, string defaultText = null)
        {
            return null;
        }

        public static string TryGet(string id, string defaultText = null)
        {
            return null;
        }

        public static string FormatString(string id, params object[] args)
        {
            var s = TryGet(id);
            if (s == null) return null;
            return string.Format(s, args);
        }
    }
}
