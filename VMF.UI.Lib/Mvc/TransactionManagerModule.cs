using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NLog;
using System.Transactions;
using System.Runtime.Remoting.Messaging;
using VMF.Core.Transactions;
using VMF.Core;

namespace VMF.UI.Lib.Mvc
{
    public class TransactionManagerModule : IHttpModule
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private static int _cnt = 0;
        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            
            log.Info("A");

            context.PostRequestHandlerExecute += Context_PostRequestHandlerExecute;
            context.BeginRequest += Context_BeginRequest;
            context.EndRequest += Context_EndRequest;
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            var cc = RQContext.Current;
            RQContext.Current = null;
            log.Warn("Cleared context {0}", cc.Id);
        }

        private void CleanupContext()
        {

        }

        private void InitTransaction()
        {
            var to = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(10)
            };
            var t0 = new CommittableTransaction(to);
            t0.TransactionCompleted += T0_TransactionCompleted;
            Transaction.Current = t0;
        }

        private void T0_TransactionCompleted(object sender, TransactionEventArgs e)
        {
            
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            var hta = (HttpApplication)sender; 

            int id = Interlocked.Increment(ref _cnt);
            log.Warn("Begin rq  {0}", id);
            hta.Context.Items.Add("vmf_rq_id", id);
            var t0 = Transaction.Current;
            if (t0 != null)
            {
                log.Error("Request {0} has transaction left {1}", id, t0.TransactionInformation.LocalIdentifier);
            }
            var cc = RQContext.Current;
            if (cc != null)
            {
                log.Error("Request {0}, has context left from {1}", id, cc.Id);
            }
            var ctx = new RQContext
            {
                Id = id
            };
            RQContext.Current = ctx;
        }

        

        private void Context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            var hta = (HttpApplication)sender;
            log.Warn("PostRequest {0}", RQContext.Current.Id);
        }
    }
}
