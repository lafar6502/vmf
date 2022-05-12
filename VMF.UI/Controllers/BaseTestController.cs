using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using VMF.Core.Transactions;

namespace VMF.UI.Controllers
{
    public class BaseTestController : Controller
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        

        public BaseTestController():base()
        {
            
        }
        private static readonly string _ky = "vmf_rq_id";
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            log.Warn("Action executing {1} {0}", filterContext.ActionDescriptor.ActionName, RQContext.Current.Id);
            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext); ;
            log.Warn("Action executed {0}", RQContext.Current.Id);
        }

        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            log.Warn("Result executing {0}", RQContext.Current.Id);
            base.OnResultExecuting(filterContext);
        }

        protected override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            log.Warn("Result executing {0}", RQContext.Current.Id);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            log.Warn("Exception {0}", RQContext.Current.Id);
            base.OnException(filterContext);
        }
    }
}