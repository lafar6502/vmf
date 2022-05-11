using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;

namespace VMF.Core.Util
{
    public class StringUtil
    {
        private static readonly Regex Re = new Regex(@"\$\{([\%\w._]+)\}");

        private static readonly Regex DapperRe = new Regex(@"@([\%\w._]+)");

        private static readonly Regex MustacheRe = new Regex(@"(?<!SOQL)\{\{([\%\w._]+)\}\}");

        /// <summary>
        /// subst values in ${ValueName} form
        /// </summary>
        /// <param name="input"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string SubstValues(string input, IDictionary<string, object> values)
        {
            return SubstValues(input, key => values.GetValueOrDefault(key, ""));
        }

        /// <summary>
        /// replace input parameters in ${parameterName} form with values provided by a function.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static string SubstValues(string input, Func<string, string> re)
        {
            return SubstValues(input, re, Re, true);
        }

        /// <summary>
        /// substitute values in double mustache {{Value}}
        /// </summary>
        /// <param name="input"></param>
        /// <param name="getVal"></param>
        /// <returns></returns>
        public static string SubstValuesMustache(string input, Func<string, string> getVal)
        {
            return SubstValues(input, getVal, MustacheRe, false);
        }

        private static string SubstValues(string input, Func<string, string> re, Regex rgEx, bool recursive)
        {
            string v1;
            string v2 = input;
            do
            {
                v1 = v2;
                v2 = rgEx.Replace(v1, new MatchEvaluator(m =>
                {
                    Capture cval = m.Groups[1].Captures[0];
                    string propName = cval.Value;
                    return re(propName);
                }));
            } while (v1 != v2 && recursive);
            return v2;
        }
        /// <summary>
        /// replace parameters in text with values provided by a function
        /// parameter format: @paramName
        /// </summary>
        /// <param name="input"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static string SubstDapperParams(string input, Func<string, string> handler)
        {
            return DapperRe.Replace(input, new MatchEvaluator(m =>
            {
                Capture cval = m.Groups[1].Captures[0];
                string propName = cval.Value;
                return handler(propName);
            }));
        }
    }
}
