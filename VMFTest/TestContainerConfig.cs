using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;
using VMF.Core.Util;
using NLog;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using VMF.Services.Config;
using Sooda;
using VMF.Services;
using VMF.Services.Transactions;
using NGinnBPM.MessageBus;
using NGinnBPM.MessageBus.Windsor;
using VMF.Services.Lists;
using System.IO;

namespace VMFTest
{
    public class TestContainerConfig
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// configures the container for tests
        /// </summary>
        public static void SetupContainer()
        {
            var c = new JsonConfigProvider("test");
            var bd = AppDomain.CurrentDomain.BaseDirectory;
            VMFGlobal.Config = c;
            var wc = new WindsorContainer();
            wc.Register(Component.For<IConfigProvider>().Instance(c));
            wc.Register(Component.For<IEntityResolver>().ImplementedBy<SoodaEntityResolver>().LifeStyle.Singleton);
            wc.Register(Component.For<IVMFTransactionFactory>().ImplementedBy<TransactionFactory>().LifeStyle.Singleton);
            SoodaConfig.SetConfigProvider(new VMFSoodaConfigProvider(c));

            wc.Register(Component.For<IListDataProvider, IListProvider>().ImplementedBy<SqlListDataProvider>().LifeStyle.Singleton
                .DependsOn(Dependency.OnValue("BaseDir", Path.Combine(bd, "SqlLists"))).Named("SqlLists"));

            MessageBusConfigurator.Begin(wc)
                .SetEndpoint("sql://default/MQ_Test")
                .SetConnectionStrings(new Dictionary<string, string>
                {
                    {"default", c.Get("default.connectionString", "") }
                })
                .AutoCreateDatabase(true)
                .SetMaxConcurrentMessages(1)
                .BatchOutgoingMessages(true)
                .SetExposeReceiveConnectionToApplication(true)
                .AutoStartMessageBus(true)
                .AddMessageHandlersFromAssembly(typeof(TestContainerConfig).Assembly)
                .UseSqlSubscriptions()
                .FinishConfiguration();

            SoodaTransaction.DefaultObjectsAssembly = typeof(VMF.BusinessObjects.ObjectClass).Assembly;

            VMFGlobal.Container = wc;

        }
    }
}
