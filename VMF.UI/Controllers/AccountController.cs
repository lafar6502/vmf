using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.UI.Lib.Mvc;
using System.Web.Mvc;
using VMF.Core;
using System.Web.Security;
using NLog;

namespace VMF.UI.Controllers
{
    public class AccountController : Controller
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public IUserRepository UserRepository { get; set; }

        public AccountController()
        {
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(Models.LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var authed = UserRepository.PasswordAuthenticate(model.Login, model.Password);
            if (authed)
            {
                //SetAuthCookie(model.Login, false);
                log.Info("Login OK for {0}", model.Login);
                FormsAuthentication.SetAuthCookie(model.Login, model.RememberMe);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                log.Warn("Login failed for {0}", model.Login);
                ModelState.AddModelError("", "Invalid user or password");
                return View(model);
            }
        }


        public ActionResult Impersonate(string userId)
        {
            if (AppUser.Current == null || !AppUser.Current.HasPermission("Impersonate"))
            {
                throw new Exception("No permission to log on as another user");
            }
            var usr = UserRepository.GetUserByUserId(userId);
            if (usr == null)
            {
                throw new Exception("User not found: " + userId);
            }
            /*if (string.IsNullOrEmpty(usr.Login) || !usr.HasPermission(Telebuddies.BusinessModel.Permission.Login.Name))
            {
                throw new Exception("User not authorized to logon");
            }*/
            Session.Abandon();
            //SetAuthCookie(usr.Login, false);
            return RedirectToAction("Index", "Home");
        }



        //
        // POST: /Account/LogOff

        public ActionResult LogOff()
        {
            var cook = Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthentication.SignOut();
            Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

    }
}
