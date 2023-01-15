using Boo.Lang.Compiler;
using NGinnBPM.DSLServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Configurator
{
    internal class ConfigDSLCompiler : SimpleBaseClassDslCompiler<ConfigDSLBase>
    {
        public ConfigDSLCompiler(ISimpleScriptStorage storage) : base(storage, typeof(ConfigDSLBase))
        {
            CompileSeparately = false;
        }

        public ConfigDSLCompiler(ISimpleScriptStorage storage, Type baseType) : base(storage, baseType)
        {
            CompileSeparately = false;
        }

        public List<Assembly> AsmReferences { get; set; }

        protected override void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, string[] urls)
        {
            if (AsmReferences != null)
            {
                foreach (Assembly asm in AsmReferences)
                {
                    if (!compiler.Parameters.References.Contains(asm)) compiler.Parameters.References.Add(asm);
                }
            }
            base.CustomizeCompiler(compiler, pipeline, urls);
            
        }
    }
}
