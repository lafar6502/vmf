using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;
using System.Transactions;

namespace VMF.Services.Transactions
{
    public class TransUtil
    {
        public static void CommitAndReEnlist(IVMFTransaction ct)
        {
            var c0 = Transaction.Current as CommittableTransaction;
            if (c0 == null) throw new Exception("There is no ambient transaction");
            Transaction.Current = null;
            try
            {
                c0.Commit();
            }
            finally
            {
                c0.Dispose();
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
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(10)
            };
            var c0 = new CommittableTransaction(to);
            c0.TransactionCompleted += C0_TransactionCompleted;
            Transaction.Current = c0;
        }

        private static void C0_TransactionCompleted(object sender, TransactionEventArgs e)
        {
            
        }

        public static void CleanupAmbientTransaction()
        {
            var c0 = Transaction.Current as CommittableTransaction;
            if (c0 == null) return;
            Transaction.Current = null;
            try
            {
                c0.Commit();
            }
            finally
            {
                c0.Dispose();
            }
        }
    }
}
