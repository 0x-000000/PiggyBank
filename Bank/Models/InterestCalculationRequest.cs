namespace Bank.Models
{
    public class InterestCalculationRequest
    {
        public double? Balance { get; set; }
        public double? Rate { get; set; }
        public int? Years { get; set; }
        public int? CompoundsPerYear { get; set; }
    }
}
