using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core.Configurator;
using BL = Boo.Lang;
using NLog;
using VMF.Core;

namespace VMF.Configurator
{
    public abstract class ConfigDSLBase
    {
        public abstract void Prepare();

        public void Initialize(ConfigLogicProvider clp)
        {
            this.Prepare();
        }

        public class ParamDef
        {
            public string Name { get; set; }
            public Type ParamType { get; set; }

            public Action Calculate { get; set; }

            public string Label { get; set; }
            public List<string> DependsOn { get; set; }

            public Func<IEnumerable<object>> OptionsGenerator { get; set; }

            public IEnumerable<object> Options { get; set; }

            public FieldAccess Access { get; set; }

            public object DefaultValue { get; set; }

            public bool IsInput { get; set; }

        }

        public string ProductId => GetType().Name;

        public ConfigModelInfo Evaluate(IDictionary<string, object> inputParams)
        {
            var m = new ConfigModelInfo
            {
                ProductId = this.ProductId,
                Fields = new List<ParamInfo>()
            };
            foreach(var p in _paramDefs)
            {
                m.Fields.Add(new ParamInfo
                {
                    Name = p.Name,
                    ParamType = p.ParamType,
                    Access = p.Access,
                    Value = p.DefaultValue,
                    Label = p.Label,
                    IsInput = p.IsInput
                });
            }
            return m;
        }

        private List<ParamDef> _paramDefs  = new List<ParamDef>();
        private ParamDef _curParam = null;
        protected void input_param(string name, Type t, Action act)
        {
            if (_curParam != null) throw new Exception();
            _curParam = new ParamDef
            {
                Name = name,
                ParamType = t,
                Access = FieldAccess.ReadOnly,
                IsInput = true
            };
            act();
            _paramDefs.Add(_curParam);
            _curParam = null;
        }

        protected void label(string s)
        {
            if (_curParam != null)
            {
                _curParam.Label = s;
            }
            else throw new Exception();
        }

        protected void default_value(object v)
        {
            if (_curParam != null)
            {
                _curParam.DefaultValue = v;
            }
        }

        protected void options(IEnumerable<object> opts)
        {
            if (_curParam != null)
            {
                _curParam.Options = opts;
            }
            else throw new Exception();
        }

        protected void options(Func<IEnumerable<object>> opts)
        {
            if (_curParam != null)
            {
                _curParam.OptionsGenerator = opts;
            }
            else throw new Exception();
        }
    }
}
