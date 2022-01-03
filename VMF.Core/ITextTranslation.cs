using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public interface ITextTranslation
    {
        IEnumerable<string> Languages { get; }
        /// <summary>
        /// get translation or null if not available
        /// </summary>
        /// <param name="id"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        string Get(string id, string language);
        /// <summary>
        /// first available translation or null
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        string GetFirst(IEnumerable<string> ids, string language);

        /// <summary>
        /// 
        /// </summary>
        event Action<ITextTranslation> TranslationsChanged;

        /// <summary>
        /// update translation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <param name="language"></param>
        void Set(string id, string text, string language);
    }
}
