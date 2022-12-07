using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using NLog;

namespace VMF.UI.Lib.Web
{
    public class ServiceCallRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new ServiceCallHandler();
        }
    }
}
