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
        public abstract void Prepare(ConfigLogicProvider clp);

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
        }

        public ProductConfigInfo Evaluate(IDictionary<string, object> inputParams)
        {
            throw new NotImplementedException();
        }

        protected void input_param(string name, Type t, Action act)
        {

        }
    }
}
