using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace InterestService
{
    public partial class InterestTryIt : System.Web.UI.Page
    {
        protected void btnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                double balance = double.Parse(txtBalance.Text);
                double rate = double.Parse(txtRate.Text);
                int years = int.Parse(txtYears.Text);
                int n = int.Parse(txtCompounds.Text);

                var svc = new InterestService();
                double amount = svc.CalculateCompoundInterest(balance, rate, years, n);

                lblResult.Text = $"Calculated Interest: {amount:C}";
            }
            catch (Exception ex)
            {
                lblResult.Text = "Error: " + ex.Message;
            }
        }
    }
}