using Sooda.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;

namespace VMF.Services.Config
{
    public class VMFSoodaConfigProvider : ISoodaConfigProvider
    {
        private IConfigProvider _cfg;
        public VMFSoodaConfigProvider(IConfigProvider cfg)
        {
            _cfg = cfg;
        }
        public string GetString(string key)
        {
            return _cfg.Get(key, (string)null);
        }
    }
}
