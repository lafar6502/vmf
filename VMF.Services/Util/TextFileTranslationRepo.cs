using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;

namespace VMF.Services.Util
{
    public class TextFileTranslationRepo : ITextTranslation
    {
        public string FileName { get; set; }

        private DateTime _readDate;
        private Dictionary<string, string> _baseData;
        private Dictionary<string, string> _overrides; 
        public IEnumerable<string> Languages => throw new NotImplementedException();

        public event Action<ITextTranslation> TranslationsChanged;

        public string Get(string id, string language)
        {
            throw new NotImplementedException();
        }

        public string GetFirst(IEnumerable<string> ids, string language)
        {
            throw new NotImplementedException();
        }

        public void Set(string id, string text, string language)
        {
            throw new NotImplementedException();
        }
    }
}
