using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VMF.UI.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Static1()
        {
            return View();
        }

        public ActionResult Static2()
        {
            return View();
        }

        public ActionResult Static2_Part()
        {
            return PartialView("Static2");
        }


        public ActionResult Static2_Json()
        {
            return Json(new
            {
                Values = new
                {
                    FirstName = "Olo",
                    LastName = "Zienkiewicz",
                    Age = 33,
                    Sex = 'M',
                    DateOfBirth = new DateTime(),
                    Groups = new int[] { 2, 3 }
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Static3()
        {
            return View();
        }
    }
}