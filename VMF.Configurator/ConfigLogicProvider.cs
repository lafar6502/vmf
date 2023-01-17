using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGinnBPM.DSLServices;
using NLog;
using VMF.Core.Configurator;

namespace VMF.Configurator
{
    public class ConfigLogicProvider : IProductConfigValidator
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public string ScriptFolder { get; set; }

        private SimpleBaseClassDslCompiler<ConfigDSLBase> _dsl;
        private ISimpleScriptStorage _scriptStorage;

        protected Type GetScriptBaseType()
        {
            return typeof(ConfigDSLBase);
        }
        protected SimpleBaseClassDslCompiler<ConfigDSLBase> GetDSL()
        {
            var ds = _dsl;
            if (ds != null) return ds;
            lock (typeof(ConfigLogicProvider))
            {
                if (_dsl != null) return _dsl;
                if (_scriptStorage == null)
                {
                    _scriptStorage = new ScriptStorage(ScriptFolder);
                }

                ds = new SimpleBaseClassDslCompiler<ConfigDSLBase>(_scriptStorage, GetScriptBaseType());
                ds.LogScripts = false;
                ds.CompileSeparately = false;
                ds.Namespaces.Add("System.Linq");
                ds.Namespaces.Add("VMF.Core");
                ds.Namespaces.Add("VMF.Configurator");

                ds.CompilationCallback((cc, urlz) =>
                {
                    if (urlz != null)
                    {
                    }
                });
                ds.CreateAll();
                _dsl = ds;
            }
            return _dsl;
        }

        protected ConfigDSLBase GetConfigLogic(string productId)
        {
            var dsl  = GetDSL();
            var x = dsl.Create(productId);
            x.Prepare(this);
            return x;
        }

        public ProductConfigInfo Validate(string productId, IDictionary<string, object> config, ValidationOptions options, object context)
        {
            throw new NotImplementedException();
        }
    }
}
