using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.UI.Lib.Mvc;
using System.Web.Mvc;
using VMF.Core;
using VMF.UI.Lib;

namespace VMF.UI.Controllers
{
    public class ListController : BaseController
    {
        public IListDataProvider DataProvider { get; set; }
        
        [HttpPost]
        public ActionResult Query(ListQuery q)
        {
            var dr = DataProvider.Query(q);

            return new JsonNetResult(dr);
        }
    }
}
