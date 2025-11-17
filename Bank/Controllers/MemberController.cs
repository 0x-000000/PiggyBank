using System.Web.Mvc;

namespace Bank.Controllers
{
    public class MemberController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Username = Request.Cookies["user"]?.Value;
            return View();
        }
    }
}
