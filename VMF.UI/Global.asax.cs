using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using VMF.Core;
using Castle.Windsor;
using Castle.MicroKernel;

namespace VMF.UI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            VMF.Core.VMFGlobal.Container = new WindsorContainer();
            App_Start.ContainerConfig.Configure(VMFGlobal.Container);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
