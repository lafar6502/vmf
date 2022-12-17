using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VMF.Core.Util;
using System.IO;
using VMF.Core;
using VMF.Services.Transactions;
using Castle.Windsor;
using NLog;
using VMF.BusinessObjects;
using System.Data.Common;
using NGinnBPM.MessageBus;

namespace VMFTest
{
    public class MTest
    {
        public string Id { get; set; }
    }
    public class MTest2
    {
        public string Id { get; set; }
    }

    [TestClass]
    public class EntityTests : IMessageConsumer<MTest>, IOutgoingMessageHandler<MTest>, IMessageConsumer<MTest2>
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        [TestInitialize]
        public void Init()
        {
            NLog.Config.SimpleConfigurator.ConfigureForConsoleLogging(LogLevel.Debug);
            TestContainerConfig.SetupContainer();
        }

        public static void InTransaction(Action act)
        {
            


        }

        [TestMethod]

        public void ManTranTest1()
        {
            var fact = VMFGlobal.Container.Resolve<IVMFTransactionFactory>();
            try
            {
                TransUtil.SetUpAmbientTransaction();
                using (var tran = fact.CreateTransaction())
                {
                    var sc = new SessionContext
                    {
                        Transaction = tran,
                    };
                    SessionContext.Current = sc;


                    var su = SysUser.GetRef(195);

                    Console.WriteLine("NAME IZ {0}", su.Name);
                }
            }
            finally
            {
                var commit = SessionContext.Current.CurrentTransactionMode == TransactionMode.Commit;
                TransUtil.CleanupAmbientTransaction(commit);
            }
        }

        [TestMethod]
        public void AppConnectionTest()
        {
            var fact = VMFGlobal.Container.Resolve<IVMFTransactionFactory>();
            var dbfact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var cs = VMFGlobal.Config.Get("default.connectionString", "");

            TransUtil.SetUpAmbientTransaction();
            DbConnection cn = null;
            try
            {
                cn = dbfact.CreateConnection();
                cn.ConnectionString = cs;
                cn.Open();
                var tran = fact.CreateTransaction(cn);
                var sc = new SessionContext
                {
                    Transaction = tran,
                    RequestDbConnection = cn
                };
                MessageBusContext.AppManagedConnection = cn;
                SessionContext.Current = sc;


                var su = SysUser.GetRef(195);
                su.Email = "test@test.org";
                var mb = VMFGlobal.ResolveService<IMessageBus>();
                mb.Notify(new MTest { Id = "T1" });
                log.Warn("Sent message");
                sc.CurrentTransactionMode = TransactionMode.Commit;
                var commit = sc.CurrentTransactionMode == TransactionMode.Commit;
                if (commit)
                {
                    sc.Transaction.Commit();
                }
                log.Warn("Commited Sooda Tran");
            }
            finally
            {
                var sc = SessionContext.Current;

                var commit = sc.CurrentTransactionMode == TransactionMode.Commit;
                if (commit)
                {
                    log.Warn("Will commit ambient tran");
                    sc.Transaction.Commit();
                    log.Warn("Committed ambient tran");
                }
                
                TransUtil.CleanupAmbientTransaction(commit);
                log.Warn("Closed ambient tran");
                MessageBusContext.AppManagedConnection = null;
                SessionContext.Current = null;
                sc.Transaction.Dispose();
                sc.Transaction = null;
                if (cn != null)
                {
                    cn.Dispose();
                }
            }
            System.Threading.Thread.Sleep(5000);
        }
        [TestMethod]
        public void EntityResolve1()
        {
            TransUtil.SetUpAmbientTransaction();
        }

        public void Handle(MTest message)
        {
            log.Info("MTest {0} sc: {1}", message.Id, SessionContext.Current != null);
        }

        public void OnMessageSend(MTest message)
        {
            log.Info("Send MTest {0}. sc: {1}", message.Id, SessionContext.Current != null);
            var mb = VMFGlobal.ResolveService<IMessageBus>();
            mb.Notify(new MTest2 { Id = message.Id + "/2" });
        }

        public void Handle(MTest2 message)
        {
            log.Info("M2Test {0}", message);
        }
    }
}
