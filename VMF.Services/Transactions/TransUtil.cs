using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;
using System.Transactions;
using NLog;
using System.Data;

namespace VMF.Services.Transactions
{
    public class TransUtil
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        static TransUtil()
        {
            var tm = TransactionManager.DefaultTimeout;
            log.Info("Default transaction timeout: {0}", tm);
            TransactionManager.DistributedTransactionStarted += TransactionManager_DistributedTransactionStarted;
        }

        private static void TransactionManager_DistributedTransactionStarted(object sender, TransactionEventArgs e)
        {
            var tid = e.Transaction.TransactionInformation.LocalIdentifier;
            log.Warn("Transaction {0} distributed. new id {1}", e.Transaction.TransactionInformation.DistributedIdentifier);    
        }

        public static void CommitAndReEnlist(IVMFTransaction ct)
        {
            var c0 = Transaction.Current as CommittableTransaction;
            
            if (c0 == null) throw new Exception("There is no ambient transaction");
            
            try
            {
                c0.Commit();
            }
            finally
            {
                c0.Dispose();
                Transaction.Current = null;
            }

            SetUpAmbientTransaction();
            c0 = Transaction.Current as CommittableTransaction;
            if (c0 == null) throw new Exception("No ambient transaction #2");
            if (ct != null) ct.ReEnlist();
        }

        public static void SetUpAmbientTransaction()
        {
            var to = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(10)
            };
            var c0 = new CommittableTransaction(to);
            c0.TransactionCompleted += C0_TransactionCompleted;
            
            Transaction.Current = c0;
        }

        private static void C0_TransactionCompleted(object sender, TransactionEventArgs e)
        {
            log.Debug("Tran completed {0}", e.Transaction.TransactionInformation.LocalIdentifier, e.Transaction.TransactionInformation.Status);
        }

        public static void CleanupAmbientTransaction(bool commit)
        {
            var c0 = Transaction.Current as CommittableTransaction;
            if (c0 == null) return;
            
            try
            {
                if (commit)
                {
                    c0.Commit();
                }
            }
            finally
            {
                c0.Dispose();
                Transaction.Current = null;
            }
        }

        public static void EnsureInVMFTransaction(IVMFTransactionFactory tf)
        {

        }


        protected static void InAmbientTransaction(Func<bool> act)
        {
            var tc = Transaction.Current;
            if (tc != null)
            {
                act();
                return;
            }
            bool commit = false;
            try
            {
                SetUpAmbientTransaction();
                commit = act();
            }
            finally
            {
                CleanupAmbientTransaction(commit);
            }
        }

        public static bool InSessionContext(IVMFTransactionFactory tf, IDbConnection cn, Action act)
        {
            if (Transaction.Current != null) throw new Exception();
            
            var sc = SessionContext.Current;
            if (sc != null)
            {
                act();
                return sc.CurrentTransactionMode == TransactionMode.Commit; ;
            }
            IVMFTransaction tran = null;
            try
            {
                var au = AppUser.Current;
                tran = tf.CreateTransaction(cn);
                sc = new SessionContext
                {
                    Transaction = tran,
                    CurrentTransactionMode = TransactionMode.Discard,
                    RequestDbConnection = cn,
                    User = au
                };
                SessionContext.Current = sc;

                act();
                if (SessionContext.Current.CurrentTransactionMode == TransactionMode.Commit)
                {
                    tran.Commit();
                    return true;
                }
                return false;
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
                sc.Transaction = null;
                SessionContext.Current = null;
            }
        }
    }
}
