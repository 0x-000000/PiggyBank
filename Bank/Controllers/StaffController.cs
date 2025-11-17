using System.Web.Mvc;

namespace Bank.Controllers
{
    public class StaffController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Username = Request.Cookies["user"]?.Value;
            return View();
        }
    }
}
