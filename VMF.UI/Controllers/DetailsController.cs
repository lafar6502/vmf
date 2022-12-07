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
            var er = new EntityRef(entity, id);
            var ent = EntityResolver.Get(er);

            return View(ent);
        }

        [HttpPost]
        public ActionResult Postback()
        {
            return View();
        }
    }
}
