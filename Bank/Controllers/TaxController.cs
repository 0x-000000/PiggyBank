using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Web.Mvc;

namespace Bank.Controllers
{
    public class TaxController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.ServiceUrl = ConfigurationManager.AppSettings["TaxServiceUrl"];
            return View();
        }

        [HttpPost]
        public ActionResult Calculate(decimal? income, string state)
        {
            var url = ConfigurationManager.AppSettings["TaxServiceUrl"];

            if (string.IsNullOrWhiteSpace(url))
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Content("{\"error\":\"Tax service URL missing.\"}", "application/json");
            }

            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Accept] = "application/json";

                    var payload = JsonConvert.SerializeObject(new
                    {
                        income,
                        state
                    });

                    var response = client.UploadString(url, "POST", payload ?? string.Empty);
                    return Content(response, "application/json");
                }
            }
            catch
            {
                Response.StatusCode = (int)HttpStatusCode.BadGateway;
                return Content("{\"error\":\"Tax service error.\"}", "application/json");
            }
        }
    }
}
