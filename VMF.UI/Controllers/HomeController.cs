using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NLog;
using VMF.Core.Transactions;

namespace VMF.UI.Controllers
{
    [Authorize]
    public class HomeController : BaseTestController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        // GET: Home

        
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
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

        public ActionResult Modal1()
        {
            return View();
        }

        public ActionResult Modal1_V()
        {
            return PartialView("Static2");
        }

        public ActionResult Modal2()
        {
            return View();
        }

        public ActionResult Modal2_V()
        {
            return PartialView();
        }

        public ActionResult Test1()
        {
            return View();
        }

        public async Task<ActionResult> Test2()
        {
            Task<ActionResult> t2 =  Task.Run(() =>
            {
                log.Warn("In async call view {0}", RQContext.Current.Id);
                return (ActionResult)View();
            });

            Task.Run(() =>
            {
                Task.Delay(2000).Wait();
                log.Warn("And this is after we waited... {0}", RQContext.Current.Id);
            });
            return t2.Result;
        }
    }
}