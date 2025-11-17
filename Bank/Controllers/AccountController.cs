using Bank.Models;
using System;
using System.Web;
using System.Web.Mvc;

namespace Bank.Controllers
{
    public class AccountController : Controller
    {
        private readonly BankSystem bank = new BankSystem();

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            var redirect = HttpUtility.UrlDecode(returnUrl);

            var account = bank.Authenticate(username, password, out var error);

            if (account == null)
            {
                ViewBag.Error = error ?? "Login failed.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var userCookie = new HttpCookie("user", account.Username)
            {
                HttpOnly = true // xss proof ggez
            };

            var roleCookie = new HttpCookie("role", account.AccountType)
            {
                HttpOnly = true
            };

            Response.Cookies.Add(userCookie);
            Response.Cookies.Add(roleCookie);

            if (!string.IsNullOrWhiteSpace(redirect))
            {
                return Redirect(redirect);
            }

            return RedirectToAction("Index", "Member");
        }

        [HttpPost]
        public ActionResult Logout()
        {
            ExpireCookie("user");
            ExpireCookie("role");

            return RedirectToAction("Index", "Home");
        }

        private void ExpireCookie(string name)
        {
            if (Request.Cookies[name] == null)
            {
                return;
            }

            var cookie = new HttpCookie(name)
            {
                Expires = DateTime.UtcNow.AddDays(-1)   // classic httponly cookie invalidation
            };
            Response.Cookies.Add(cookie);
        }
    }
}
