using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boo.Lang;

namespace VMF.Configurator
{
    public class QuackDictWrapper : IQuackFu
    {
        private IDictionary<string, object> _d;
        private bool _mutable = false;
        public QuackDictWrapper(IDictionary<string, object> d)
        {
            _d = d;
        }

        public QuackDictWrapper(Dictionary<string, object> d) : this((IDictionary<string, object>) d)
        {

        }

        public QuackDictWrapper(System.Collections.IDictionary d)
        {
            _d = new Dictionary<string, object>();
            foreach(System.Collections.DictionaryEntry kv in d)
            {
                _d[kv.Key.ToString()] = kv.Value;
            }
        }
        public QuackDictWrapper(IDictionary<string, object> d, bool mutable)
        {
            _d = d;
            _mutable = mutable;
        }

        public IDictionary<string, object> Dictionary
        {
            get { return _d;  }
            set { _d = value; }
        }

        public object QuackGet(string name, object[] parameters)
        {
            object v;
            if (string.IsNullOrEmpty(name) && parameters != null && parameters.Length == 1)
            {
                name = parameters[0].ToString();
            }
            if (!_d.TryGetValue(name, out v))
            {
                if (name.ToLower() == "keys" || name.ToLower() == "_keys") return _d.Keys;
                if (name.ToLower() == "contents" || name.ToLower() == "_contents") return _d;
                return null;
            }
            if (v is IDictionary<string, object>) return new QuackDictWrapper((IDictionary<string, object>)v);
            return v;
        }

        public object QuackInvoke(string name, params object[] args)
        {
            throw new NotImplementedException();
        }

        public object QuackSet(string name, object[] parameters, object value)
        {
            if (!_mutable) throw new Exception("read only");
            if (string.IsNullOrEmpty(name) && parameters != null && parameters.Length == 1)
            {
                name = parameters[0].ToString();
            }
            _d[name] = value;
            return value;
        }
    }
}
