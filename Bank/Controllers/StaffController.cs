using Bank.Models;
using System.Web.Mvc;

namespace Bank.Controllers
{
    public class StaffController : Controller
    {
        private readonly BankSystem bank = new BankSystem();

        public ActionResult Index()
        {
            var user = Request.Cookies["user"]?.Value;
            var role = Request.Cookies["role"]?.Value ?? "member";

            if (string.IsNullOrWhiteSpace(user))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!string.Equals(role, "admin"))
            {
                return RedirectToAction("Index", "Member");
            }

            var account = bank.GetAccount(user);
            ViewBag.Username = account?.Username ?? user;
            ViewBag.AccountType = account?.AccountType ?? role;
            return View();
        }
    }
}
