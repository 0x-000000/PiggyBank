using Bank.InterestSrv;
using Bank.Models;
using System;
using System.Net;
using System.Web.Mvc;

namespace Bank.Controllers
{
    public class InterestController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Title = "Interest Calculator";
            return View();
        }

        [HttpPost]
        public ActionResult Calculate(InterestCalculationRequest request)
        {
            var validationError = Validate(request);
            if (!string.IsNullOrEmpty(validationError))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { error = validationError });
            }

            try
            {
                double result;
                using (var client = new InterestServiceClient())
                {
                    result = client.CalculateCompoundInterest(
                        request.Balance.Value,
                        request.Rate.Value,
                        request.Years.Value,
                        request.CompoundsPerYear.Value);
                }

                return Json(new { result });
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadGateway;
                return Json(new { error = $"Interest service error: {ex.Message}" });
            }
        }

        private static string Validate(InterestCalculationRequest request)
        {
            if (request == null)
            {
                return "Request payload is required.";
            }

            if (!request.Balance.HasValue)
            {
                return "Balance is required.";
            }

            if (!request.Rate.HasValue)
            {
                return "Annual rate is required.";
            }

            if (!request.Years.HasValue)
            {
                return "Years is required.";
            }

            if (!request.CompoundsPerYear.HasValue)
            {
                return "Compounds per year is required.";
            }

            if (request.Balance.Value < 0)
            {
                return "Balance cannot be negative.";
            }

            if (request.Years.Value < 0)
            {
                return "Years cannot be negative.";
            }

            if (request.CompoundsPerYear.Value <= 0)
            {
                return "Compounds per year must be at least 1.";
            }

            return null;
        }
    }
}
