using System;

namespace InterestService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class InterestService : IInterestService
    {
        public double CalculateCompoundInterest(double balance, double annualIntRatePercent, int years, int compoundsPerYear)
        {
            if (balance < 0 || annualIntRatePercent < 0 || years < 0 || compoundsPerYear < 0)
            {
                throw new ArgumentException("Starting values must be greater than 0!");
            }

            double intRateDecimal = annualIntRatePercent / 100.0;
            double output = balance * Math.Pow(1 + intRateDecimal / compoundsPerYear, compoundsPerYear * years);

            output = Math.Round(output, 2);
            return output;
        }
    }
}
