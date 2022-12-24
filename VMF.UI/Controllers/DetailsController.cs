using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.UI.Lib.Mvc;
using System.Web.Mvc;
using VMF.Core;

namespace VMF.UI.Controllers
{
    public class DetailsController : BaseController
    {
        public IEntityResolver EntityResolver { get; set; }
        public ActionResult Show(string entity, string id)
        {
            bool renderFormModel = false; 
            var er = new EntityRef(entity, id);
            var ent = EntityResolver.Get(er);
            var sc = SessionContext.Current;
            if (sc == null) throw new Exception();
            var vv = ent as ISelectView;
            IEnumerable<string> viewNames = null;
            if (vv != null)
            {
                viewNames = vv.GetViewNames();
            }
            else
            {
                viewNames = ent.GetType().WithBaseTypes().Where(x => EntityResolver.KnowsEntityType(x)).Select(x => x.Name + ".Details");
            }
            var viewName = viewNames.FirstOrDefault(x => MvcUtil.ExistsPartial(x, this.ControllerContext));
            if (viewName == null) throw new Exception("View not found: " + String.Join(",", viewNames));
            ViewBag.FormViewUrl = Url.Action("Show", "Details", new { entity=er.Entity, id=er.Id, partial = true, view = viewName });

            if (renderFormModel)
            {
                throw new NotImplementedException();
            }
            else
            {
                return PartialView(viewName, ent);
            }

        }

        [HttpPost]
        public ActionResult Postback()
        {
            return View();
        }
    }
}
