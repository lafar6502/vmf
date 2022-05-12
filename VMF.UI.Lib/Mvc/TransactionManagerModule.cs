using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NLog;
using VMF.Core;
using System.Transactions;
using System.Runtime.Remoting.Messaging;
using VMF.Core.Transactions;
using VMF.Services.Transactions;

namespace VMF.UI.Lib.Mvc
{
    public class TransactionManagerModule : IHttpModule
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private static int _rqId = 0;

        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += Context_BeginRequest;
            context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute;
            context.PostRequestHandlerExecute += Context_PostRequestHandlerExecute;
            context.EndRequest += Context_EndRequest;
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            //var cc = RQContext.Current;
            //RQContext.Current = null;
            //log.Warn("Cleared context {0}", cc.Id);
        }

        private void CleanupContext()
        {

        }

        
        private void Context_BeginRequest(object sender, EventArgs e)
        {
            var hta = (HttpApplication)sender; 

            int id = Interlocked.Increment(ref _rqId);
            CurrentRequestId = id;
            log.Warn("Begin rq  {0}", id);
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
            TransUtil.SetUpAmbientTransaction();

            var ctx = new RQContext
            {
                Id = id
            };
            RQContext.Current = ctx;
        }

        


        private void Context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            log.Info("POST request {0}", CurrentRequestId);
            var sc = SessionContext.Current;
            if (sc == null)
            {
                log.Warn("No session context {0}", CurrentRequestId);
            }
            else
            {
                if (sc.RequestId != null && sc.RequestId != CurrentRequestId.ToString())
                {
                    log.Error("post request - Session context in rq {0} - from {1}", CurrentRequestId, sc.RequestId);
                }
            }
            SessionContext.Current = null;
            TransUtil.CleanupAmbientTransaction();
        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            log.Info("PRE request {0}", CurrentRequestId);
            var sc = SessionContext.Current;
            if (sc != null)
            {
                log.Error("Session context in rq {0} - from {1}", CurrentRequestId, sc.RequestId);
            }
            sc = new SessionContext();
            sc.RequestId = CurrentRequestId.ToString();
            SessionContext.Current = sc;
            //will make transaction here
        }

        

        public static readonly string _ky = "__vmfrqid";

        public static int? CurrentRequestId
        {
            get
            {
                var ctx = HttpContext.Current;
                if (ctx == null) return null;
                if (!ctx.Items.Contains(_ky)) return null;
                return (int) ctx.Items[_ky];
            }
            set
            {
                var ctx = HttpContext.Current;
                if (ctx == null) throw new Exception("no http request");
                ctx.Items[_ky] = value;
            }
        }
    }
}
