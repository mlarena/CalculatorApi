using CalculatorApi.Models;

namespace CalculatorApi.Services
{
    public interface ICalculatorService
    {
        CalculationResponse Calculate(CalculationRequest request);
    }
}