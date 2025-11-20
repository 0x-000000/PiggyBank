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
            if (string.IsNullOrWhiteSpace(user))
            {
                return RedirectToAction("Login", "Account");
            }

            var account = bank.GetAccount(user);
            if (account == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var role = string.IsNullOrWhiteSpace(account.AccountType) ? "member" : account.AccountType;

            if (!string.Equals(role, "admin"))
            {
                return RedirectToAction("Index", "Member");
            }

            ViewBag.Username = account.Username ?? user;
            ViewBag.AccountType = role;
            return View();
        }
    }
}
