using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core.Configurator
{
    public interface IProductConfigValidator
    {
        ConfigModelInfo GetConfigModelInfo(string productId);
        ConfigModelInfo Validate(string productId, IDictionary<string, object> config, ValidationOptions options, object context);

    }
}
