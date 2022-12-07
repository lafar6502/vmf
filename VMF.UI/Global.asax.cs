using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using VMF.Core;
using Castle.Windsor;
using Castle.MicroKernel;
using VMF.UI.Lib.Mvc;

namespace VMF.UI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            VMF.Core.VMFGlobal.Container = new WindsorContainer();
            App_Start.ContainerConfig.Configure(VMFGlobal.Container);
            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(VMF.Core.VMFGlobal.Container.Kernel));
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var eng = ViewEngines.Engines.FirstOrDefault(x => x is RazorViewEngine) as RazorViewEngine;
            if (eng != null)
            {
                eng.ViewLocationFormats.Count();
                eng.ViewLocationFormats = new string[]
                {
                        "~/Views/" + VMFGlobal.AppProfile + "/{1}/{0}.cshtml",
                        "~/Views/{1}/{0}.cshtml",
                        "~/Views/FacileWeb/{1}/{0}.cshtml",
                        "~/Views/Shared/" + VMFGlobal.AppProfile + "/{0}.cshtml",
                        "~/Views/Shared/{0}.cshtml"
                };
                eng.PartialViewLocationFormats = new string[]
                {
                        "~/Views/" + VMFGlobal.AppProfile + "/{1}/{0}.cshtml",
                        "~/Views/{1}/{0}.cshtml",
                        "~/Views/FacileWeb/{1}/{0}.cshtml",
                        "~/Views/FacileWeb/Shared/{0}.cshtml",
                        "~/Views/Shared/" + VMFGlobal.AppProfile + "/{0}.cshtml",
                        "~/Views/Shared/{0}.cshtml"
                };
            }
        }
    }
}
