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

    [TestClass]
    public class ListTests 
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        [TestInitialize]
        public void Init()
        {
            NLog.Config.SimpleConfigurator.ConfigureForConsoleLogging(LogLevel.Debug);
            TestContainerConfig.SetupContainer();
        }



        public void InTransaction(Action act)
        {
            var fact = VMFGlobal.Container.Resolve<IVMFTransactionFactory>();
            var dbfact = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var cs = VMFGlobal.Config.Get("default.connectionString", "");

            TransUtil.SetUpAmbientTransaction();
            DbConnection cn = null;
            var commit = false;
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
                sc.CurrentTransactionMode = TransactionMode.Discard;
                act();
                commit = sc.CurrentTransactionMode == TransactionMode.Commit;
                if (commit)
                {
                    sc.Transaction.Commit();
                }
                log.Warn("Commited Sooda Tran");
            }
            catch (Exception ex)
            {
                commit = false;
                log.Error("Error: {0}", ex);
                throw;
            }
            finally
            {
                var sc = SessionContext.Current;
                try
                {
                    TransUtil.CleanupAmbientTransaction(commit);
                    log.Warn("Closed ambient tran");
                }
                finally
                {
                    MessageBusContext.AppManagedConnection = null;
                    SessionContext.Current = null;
                    sc.Transaction.Dispose();
                    sc.Transaction = null;
                    if (cn != null)
                    {
                        cn.Dispose();
                    }
                }
            }
        }

        [TestMethod]
        public void SqlListTest1()
        {
            var lp = VMFGlobal.ResolveService<IListProvider>();
            var ld = VMFGlobal.ResolveService<IListDataProvider>();
            InTransaction(() =>
            {
                var l1 = lp.GetList("List1");
                foreach(var c in l1.Columns) Console.WriteLine("col:" + c.Name);

                var lq = new ListQuery
                {
                    ListId = l1.ListId,
                    Limit = 1000,
                    Start = 0,
                    WithCount = l1.CountSupported,
                    SelectColumns = null,
                    Filters = new ListQuery.Filter[] { new ListQuery.Filter
                        {
                            Name = "query",
                            Args = "Ozo"
                        } 
                    }
                };

                var res = ld.Query(lq);


            });
        }
    }
}
