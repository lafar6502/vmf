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
using VMF.Services.Util;

namespace VMF.UI.Lib.Mvc
{
    public class TransactionManagerModule : IHttpModule
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private static int _rqId = 0;
        private IVMFTransactionFactory TransactionFactory { get; set; }
        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += Context_BeginRequest;
            context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute;
            context.PostRequestHandlerExecute += Context_PostRequestHandlerExecute;
            context.PostAuthenticateRequest += Context_PostAuthenticateRequest;
            context.EndRequest += Context_EndRequest;
            context.Error += Context_Error;
            
        }

        private void Context_Error(object sender, EventArgs e)
        {
            throw new Exception("Error on error");
            log.Warn("Error ..");
        }

        private void Context_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var cx = HttpContext.Current;
            if (cx.User == null || !cx.User.Identity.IsAuthenticated)
            {
                AuthenticateBasic();
            }
            log.Info("AUTH request {0}, user {1}, authed {2}, type {3}", CurrentRequestId, cx.User == null ? "--no user--" : cx.User.Identity.Name, cx.User == null ? false : cx.User.Identity.IsAuthenticated, cx.User == null ? "-" : cx.User.GetType().FullName);
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            log.Warn("END request {0}", CurrentRequestId);
            //var cc = RQContext.Current;
            //RQContext.Current = null;
            //log.Warn("Cleared context {0}", cc.Id);

            TransUtil.CleanupAmbientTransaction(false);
            if (Transaction.Current != null)
            {
                log.Error("Transaction cleanup! {0}", RQContext.Current.Id);
                throw new Exception("1");
            }
            var sc = SessionContext.Current;

            CleanVMFTransaction();

            RQContext.Current = null;
        }

        private void CleanupContext()
        {

        }

        
        private void Context_BeginRequest(object sender, EventArgs e)
        {
            var hta = (HttpApplication)sender; 

            int id = Interlocked.Increment(ref _rqId);
            CurrentRequestId = id;
            log.Warn("Begin rq  {0}: {1}", id, HttpContext.Current.Request.Url);
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
            TransUtil.SetUpAmbientTransaction();

            
        }


        private void AuthenticateBasic()
        {
            var rq = HttpContext.Current.Request;
            var auth = rq.Headers["Authorization"];
            if (!String.IsNullOrEmpty(auth))
            {
                if (!auth.StartsWith("Basic "))
                {
                    log.Warn("Invalid auth hdr: " + auth);
                    return;
                }
                var cred = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(auth.Substring(6))).Split(':');
                if (cred.Length != 2)
                {
                    log.Warn("Basic auth invalid {0}", auth);
                    return;
                }
                log.Warn("Basic auth {0}:{1}", cred[0], cred[1]);
                if (!string.IsNullOrEmpty(cred[0]))
                {
                    AppUser.Current = new AppUser
                    {
                        Login = cred[0],
                        Name = cred[0]
                    };
                    HttpContext.Current.User = AppUser.Current;
                }
                /*var repo = AppGlobal.Container.Resolve<IUserRepository>();
                if (repo != null)
                {
                    if (cred.Length < 2)
                    {
                        log.Warn("Basic auth invalid creds: {0}", auth);
                    }

                    //var user = new { Name = cred[0], Pass = cred[1] };
                    var authed = repo.PasswordAuthenticate(cred[0], cred[1]);
                    log.Info("Basic auth for user {0}: {1}", cred[0], authed);
                    if (authed)
                    {
                        var au = repo.GetUserByUserId(cred[0]);
                        if (au != null)
                        {
                            log.Info("User set by basic auth: {0}", au.Id, au.Identity.Name);
                            AppUser.Current = au;
                            HttpContext.Current.User = au;
                        }
                        else log.Warn("User not found: {0}", cred[0]);
                    }
                }*/
            }
        }

        protected void CleanVMFTransaction()
        {
            var sc = SessionContext.Current;
            if (sc == null) return;
            var t = sc.Transaction;
            var c = sc.RequestDbConnection;
            try
            {
                
                if (t != null)
                {
                    t.Dispose();
                }
            }
            catch(Exception e)
            {
                log.Warn("Error disposing tran {0}", e);
            }
            try
            {

                if (c != null)
                {
                    c.Dispose();
                }
            }
            catch (Exception e)
            {
                log.Warn("Error disposing conn {0}", e);
            }
            SessionContext.Current = null;
        }

        private void Context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            log.Info("POST request {0}", CurrentRequestId);
            var sc = SessionContext.Current;
            var doCommit = false;
            if (sc == null)
            {
                log.Error("No session context {0}", CurrentRequestId);
            }
            else
            {
                if (sc.RequestId != null && sc.RequestId != CurrentRequestId.ToString())
                {
                    log.Error("post request - Session context in rq {0} - from {1}", CurrentRequestId, sc.RequestId);
                }
                doCommit = sc.CurrentTransactionMode == TransactionMode.Commit;
            }
            
            TransUtil.CleanupAmbientTransaction(doCommit);
            CleanVMFTransaction();
        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            var cx = HttpContext.Current;
            log.Info("PRE request {0}, user {1}, authed {2}, type {3}", CurrentRequestId, cx.User == null ? "--no user--" : cx.User.Identity.Name, cx.User == null ? false : cx.User.Identity.IsAuthenticated, cx.User == null ? "-" : cx.User.GetType().FullName);
            var sc = SessionContext.Current;
            if (sc != null)
            {
                log.Error("Session context  in rq {0} - from {1}", CurrentRequestId, sc.RequestId);
            }

            sc = new SessionContext();
            sc.RequestId = CurrentRequestId.ToString();

            try
            {
                sc.RequestDbConnection = DbUtil.CreateDefaultConnection();
                var tf = VMFGlobal.ResolveService<IVMFTransactionFactory>();
                var tran = tf.CreateTransaction(sc.RequestDbConnection);
                sc.Transaction = tran;

            }
            catch (Exception ex)
            {
                log.Error("Error creating transaction: {0}", ex.ToString());
                if (sc.Transaction != null) sc.Transaction.Dispose();
                if (sc.RequestDbConnection != null) sc.RequestDbConnection.Dispose();
                throw;
            }
            
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
