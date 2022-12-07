using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using VMF.Core;

namespace VMF.UI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.Add(new Route("servicecall/{*pathInfo}", VMFGlobal.Container.Resolve<VMF.UI.Lib.Web.ServiceCallRouteHandler>()));

            routes.MapRoute(name: "Details",
                url: "Details/Show/{entity}/{id}",
                defaults: new { controller = "Details", action = "Show" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
