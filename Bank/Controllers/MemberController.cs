using Bank.Models;
using System.Web.Mvc;

namespace Bank.Controllers
{
    public class MemberController : Controller
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
            ViewBag.Username = account?.Username ?? user;
            ViewBag.AccountType = account?.AccountType ?? "member";
            return View();
        }
    }
}
