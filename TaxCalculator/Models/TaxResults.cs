using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxCalculator.Models
{
    public class TaxResults
    {
        public decimal FederalTax { get; set; }
        public decimal StateTax { get; set; }
        public decimal TotalTax => FederalTax + StateTax;
        public decimal NetIncome { get; set; }
    }
}