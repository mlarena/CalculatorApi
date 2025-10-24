namespace CalculatorApi.Models
{
    public class CalculationResponse
    {
        public double Result { get; set; }
        public string Status { get; set; } = "Success";
        public string Message { get; set; } = string.Empty;
    }
}