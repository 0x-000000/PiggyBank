using Bank.Models;
using System;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Bank.Controllers
{
    public class AccountController : Controller
    {
        private readonly BankSystem bank = new BankSystem();
        private static readonly char[] CharPool = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789@$^()".ToCharArray();
        private static readonly Random RNG = new Random();

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
                HttpOnly = true, // xss proof ggez
                Path = "/"
            };

            Response.Cookies.Add(userCookie);

            if (!string.IsNullOrWhiteSpace(redirect))
            {
                return Redirect(redirect);
            }

            return RedirectToAction("Index", "Member");
        }

        [HttpGet]
        public ActionResult Register()
        {
            PrepareCaptcha();
            return View();
        }

        [HttpPost]
        public ActionResult Register(string username, string password, string confirmPassword, string captchaText, string captchaAnswer)
        {
            ViewBag.Username = username;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Username and password are required.");
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "Passwords must match.");
            }

            if (!IsCaptchaValid(captchaText, captchaAnswer))
            {
                ModelState.AddModelError("captchaText", "Captcha text is incorrect.");
            }

            if (!ModelState.IsValid)
            {
                PrepareCaptcha();
                return View();
            }

            var account = bank.SignUp(username, password, "member", out var error);

            if (account == null)
            {
                ModelState.AddModelError(string.Empty, error ?? "Registration failed.");
                PrepareCaptcha();
                return View();
            }

            return RedirectToAction("Index", "Member");
        }

        // asu server requests go brrrr
        [HttpPost]
        public ActionResult RefreshCaptcha()
        {
            var code = GenerateCaptchaCode();
            return Json(new { imageUrl = BuildCaptchaUrl(code), answer = code });
        }

        [HttpGet]
        public ActionResult SuggestPassword()
        {
            var url = ConfigurationManager.AppSettings["PasswordServiceUrl"];

            try
            {
                using (var client = new WebClient())
                {
                    var payload = client.DownloadString(url);
                    return Content(payload, "application/json");
                }
            }
            catch
            {
                Response.StatusCode = 500;
                return Json(new { error = "Password generator is down?" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Logout()
        {
            ExpireCookie("user");

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
                Expires = DateTime.UtcNow.AddDays(-1),   // classic httponly cookie invalidation
                Path = "/"
            };
            Response.Cookies.Add(cookie);
        }

        private void PrepareCaptcha()
        {
            var code = GenerateCaptchaCode();
            ViewBag.CaptchaCode = code;
            ViewBag.CaptchaUrl = BuildCaptchaUrl(code);
        }

        private static string GenerateCaptchaCode()
        {
            var chars = new char[5];
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = CharPool[RNG.Next(CharPool.Length)];
            }

            return new string(chars);
        }

        private bool IsCaptchaValid(string input, string expected)
        {
            if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return string.Equals(expected.Trim(), input.Trim(), StringComparison.Ordinal);
        }

        private static string BuildCaptchaUrl(string code)
        {
            return $"https://venus.sod.asu.edu/WSRepository/Services/ImageVerifier/Service.svc/GetImage/{code}";
        }
    }
}
