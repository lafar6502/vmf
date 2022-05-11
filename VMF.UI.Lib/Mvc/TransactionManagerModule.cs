using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NLog;
using VMF.Core;

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
            log.Info("A");
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

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            var id = Interlocked.Increment(ref _rqId);
            CurrentRequestId = id;
            log.Info("New request {0} {1} {2}", id, HttpContext.Current.Request.Url, HttpContext.Current.User == null || HttpContext.Current.User.Identity == null ? "-no user-" : HttpContext.Current.User.Identity.Name);
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
