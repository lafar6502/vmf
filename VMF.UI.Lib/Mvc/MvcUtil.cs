using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.IO;
using Castle.Windsor;
using System.Reflection;
using Castle.MicroKernel.Registration;

namespace VMF.UI.Lib.Mvc
{
    public class MvcUtil
    {
        /// <summary>
        /// check if view exists
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static bool ExistsView(string name, ControllerContext cc)
        {
            return ViewEngines.Engines.Any(x => x.FindView(cc, name, null, false).View != null);
        }

        /// <summary>
        /// check if partial view exists
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static bool ExistsPartial(string name, ControllerContext cc)
        {
            
            return ViewEngines.Engines.Any(x => x.FindPartialView(cc, name, false).View != null);
        }

        /// <summary>
        /// return current view's name
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetViewName(HtmlHelper html)
        {
            var v = html.ViewContext.View;
            var vn = html.ViewContext.View as WebFormView;
            if (vn != null) return vn.ViewPath;
            var pi = v.GetType().GetProperty("ViewPath");
            if (pi != null) return (string)pi.GetValue(v);
            return v.GetType().Name;
        }

        /// <summary>
        /// render view body to string 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="viewPath"></param>
        /// <param name="model"></param>
        /// <param name="partial"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static string RenderViewToString(ControllerContext context,
                                    string viewPath,
                                    object model = null,
                                    bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException("View cannot be found.");

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(context, view,
                                            context.Controller.ViewData,
                                            context.Controller.TempData,
                                            sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }

        /// <summary>
        /// registers controller classes from an assembly in th econtainer
        /// </summary>
        /// <param name="asm"></param>
        /// <param name="wc"></param>
        public static void RegisterControllersFromAssembly(Assembly asm, IWindsorContainer wc)
        {
            foreach (var t in asm.GetTypes().Where(t => typeof(System.Web.Mvc.Controller).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType))
            {
                wc.Register(Component.For(t).ImplementedBy(t).LifeStyle.Transient);
            }
        }
    }
}
