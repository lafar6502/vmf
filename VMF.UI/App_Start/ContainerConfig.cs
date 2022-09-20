using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using VMF.Core;
using VMF.Core.Config;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;

namespace VMF.UI.App_Start
{
    public class ContainerConfig
    {
        public static void Configure(IWindsorContainer wc)
        {
            var cfg = new JsonConfig();
            wc.Register(Component.For<IConfigProvider>().Instance(cfg));
            VMFGlobal.Config = cfg;

        }
    }
}
