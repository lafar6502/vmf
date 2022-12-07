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
using VMF.Core.Util;
using VMF.Services;
using VMF.Services.Transactions;
using VMF.UI.Lib.Web;

namespace VMF.UI.App_Start
{
    public class ContainerConfig
    {
        public static void Configure(IWindsorContainer wc)
        {

            var cfg = new JsonConfig();
            wc.Register(Component.For<IConfigProvider>().Instance(cfg));
            wc.Register(Component.For<IEntityResolver>().ImplementedBy<SoodaEntityResolver>().LifeStyle.Singleton);
            wc.Register(Component.For<IVMFTransactionFactory>().ImplementedBy<TransactionFactory>().LifeStyle.Singleton);

            wc.Register(Component.For<ServiceCallRouteHandler>().ImplementedBy<ServiceCallRouteHandler>());
            VMFGlobal.Config = cfg;


        }
    }
}
