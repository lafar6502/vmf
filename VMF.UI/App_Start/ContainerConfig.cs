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
using System.Reflection;
using Sooda;
using VMF.Services.Config;
using NGinnBPM.MessageBus;
using VMF.Services.Lists;
using System.IO;

namespace VMF.UI.App_Start
{
    public class ContainerConfig
    {
        public static void Configure(IWindsorContainer wc)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var cfg = new JsonConfig();
            wc.Register(Component.For<IConfigProvider>().Instance(cfg));
            wc.Register(Component.For<IServiceResolver>().Instance(new NGinnBPM.MessageBus.Windsor.WindsorServiceResolver(wc.Kernel)));
            wc.Register(Component.For<IEntityResolver>().ImplementedBy<SoodaEntityResolver>().LifeStyle.Singleton);
            wc.Register(Component.For<IVMFTransactionFactory>().ImplementedBy<TransactionFactory>().LifeStyle.Singleton);

            wc.Register(Component.For<ServiceCallRouteHandler>().ImplementedBy<ServiceCallRouteHandler>());
            wc.Register(Component.For<IUserRepository>().ImplementedBy<VMF.BusinessObjects.Services.UserRepository>().LifeStyle.Singleton);

            wc.Register(Component.For<IListDataProvider, IListProvider>().ImplementedBy<MasterListProvider>().LifeStyle.Singleton);
            wc.Register(Component.For<IListDataProvider, IListProvider>().ImplementedBy<SqlListDataProvider>().Named("Sql").DependsOn(
                new 
                {
                    BaseDir = Path.Combine(baseDir, "appdata\\SqlLists")
                }).LifeStyle.Singleton);

            RegisterControllersFromAssembly(typeof(Controllers.BaseTestController).Assembly, wc);
            SoodaConfig.SetConfigProvider(new VMFSoodaConfigProvider(cfg));
            //Sooda.TransactionStrategyMenager.SetTransactionStrategy(new Sooda.TransactionStrategy.SoodaThreadBoundTransactionStrategy());
            var st = SoodaTransaction.DefaultTransactionStrategy;
            SoodaTransaction.DefaultObjectsAssembly = typeof(VMF.BusinessObjects.ObjectClass).Assembly;
            
            //Sooda.Sql.SqlBuilderMenager.SetDefaultBuilder(new Sooda.SqlServer.SqlServerBuilder());

            VMFGlobal.Config = cfg;
        }

        public static void RegisterServicesFromAssembly<T, I>(Assembly asm, IWindsorContainer wc)
        {
            foreach (var t in asm.GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType))
            {
                wc.Register(Component.For(t).ImplementedBy(t).LifeStyle.Transient);
            }
        }

        public static void RegisterControllersFromAssembly(Assembly asm, IWindsorContainer wc)
        {
            foreach (var t in asm.GetTypes().Where(t => typeof(System.Web.Mvc.Controller).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType))
            {
                wc.Register(Component.For(t).ImplementedBy(t).LifeStyle.Transient);
            }
        }
    }
}
