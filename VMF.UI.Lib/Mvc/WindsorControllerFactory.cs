using Castle.MicroKernel;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace VMF.UI.Lib.Mvc
{
    public class WindsorControllerFactory : DefaultControllerFactory
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IKernel _kernel;

        public WindsorControllerFactory(IKernel kernel)
        {
            _kernel = kernel;
        }

        public override void ReleaseController(IController controller)
        {
            Log.Trace("WindsorControllerFactory.ReleaseController(controller: {0})", controller.GetType());
            _kernel.ReleaseComponent(controller);
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                throw new HttpException(404, string.Format("The controller for path '{0}' could not be found.", requestContext.HttpContext.Request.Path));
            }

            Log.Trace("WindsorControllerFactory.GetControllerInstance(requestContext, controllerType: {0})", controllerType);
            return (IController)_kernel.Resolve(controllerType);
        }
    }
}
