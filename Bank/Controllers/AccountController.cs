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

            var account = bank.GetBalance(new BalanceRequest
            {
                Username = username,
                Password = password
            }, out var error);

            if (account == null)
            {
                ViewBag.Error = error ?? "Login failed.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var cookie = new HttpCookie("user", account.Username)
            {
                HttpOnly = true // xss proof ggez
            };
            Response.Cookies.Add(cookie);

            if (!string.IsNullOrWhiteSpace(redirect))
            {
                return Redirect(redirect);
            }

            return RedirectToAction("Index", "Member");
        }

        [HttpPost]
        public ActionResult Logout()
        {
            if (Request.Cookies["user"] != null)
            {
                var cookie = new HttpCookie("user")
                {
                    Expires = DateTime.UtcNow.AddDays(-1)   // classic httponly cookie invalidation
                };
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
