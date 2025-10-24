namespace CalculatorApi.Models
{
    public class CalculationRequest
    {
        public double Operand1 { get; set; }
        public double Operand2 { get; set; }
        public string Operation { get; set; } = string.Empty;
    }
}