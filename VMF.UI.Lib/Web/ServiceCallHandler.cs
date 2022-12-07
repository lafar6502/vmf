using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NLog;

namespace VMF.UI.Lib.Web
{
    public class ServiceCallHandler : IHttpHandler
    {
        public bool IsReusable => true;
        private static Logger log = LogManager.GetCurrentClassLogger();

        public void ProcessRequest(HttpContext context)
        {
            log.Warn("Processs request {0}", context.Request.Url);
            var resp = context.Response;
            resp.Write("HELLO");
            if (context.Request.Url.Query.Contains("x"))
            {
                context.Response.End();
            }
        }
    }
}
