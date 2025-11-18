using System.Web.Mvc;
using TaxCalculator.Models;

//SINCE IM CLOSE TO OUT OF TIME. FOR ASSIGNMENT 6, ADD A DIFFERENCE BETWEEN SINGLE AND MARRIED INCOME RATES. FINISH STATE RATES. FIX UI

namespace TaxCalculator.Controllers
{
    public class TaxCalculatorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]

        public ActionResult Calculate(TaxInfoInput request)
        {
            if (!ModelState.IsValid)
                return View("Index", request);

            decimal federalRate = FederalTaxRate(request.Income);
            decimal stateRate = StateTaxRate(request.State);

            decimal federalTax = request.Income * federalRate;
            decimal stateTax = request.Income * stateRate;

            var result = new TaxResults
            {
                FederalTax = federalTax,
                StateTax = stateTax,
                NetIncome = request.Income - (federalTax + stateTax)
            };

            return View("TaxResults", result);
        }

        private decimal FederalTaxRate(decimal income) //2025 Tax Rates from income bracket, sources by https://www.nerdwallet.com/taxes/learn/federal-income-tax-brackets
        {
            if (income < 11925)
            {
                return 0.10m;
            }
            else if (income > 11925 && income < 48475)
            {
                return 0.12m;
            }
            else if (income > 48476 && income < 103350)
            {
                return 0.22m;
            }
            else if (income > 103351 && income < 197300)
            {
                return 0.24m;
            }
            else if (income > 197301 && income < 250525)
            {
                return 0.32m;
            }
            else if (income > 250526 && income < 626350)
            {
                return 0.35m;
            }
            else
            {
                return 0.37m;
            }
        }

        private decimal StateTaxRate(string state) //2025 Tax Rates from States, sources by https://taxfoundation.org/data/all/state/state-income-tax-rates/
        {
            switch (state.ToUpper())
            { //DO THE REST OF THE STATES IN THE ASSIGNMENT 6 UPLOAD. NOTE: THESE ARE ESTIMATED TO ONLY 2 DECIMAL POINTS
                case "CA": return 0.13m;
                case "NY": return 0.10m;
                case "AZ": return 0.02m;
                case "TX": return 0.00m;
                case "NV": return 0.00m;
                case "FL": return 0.00m;
                case "TN": return 0.00m;
                case "WY": return 0.00m;
                case "SD": return 0.00m;
                case "AK": return 0.00m;
                case "WA": return 0.00m;
                case "NH": return 0.00m;
                default: return 0.05m;
            }
        }
    }
}