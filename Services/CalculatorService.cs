using CalculatorApi.Models;

namespace CalculatorApi.Services
{
    public class CalculatorService : ICalculatorService
    {
        private readonly ILogger<CalculatorService> _logger;

        public CalculatorService(ILogger<CalculatorService> logger)
        {
            _logger = logger;
        }

        public CalculationResponse Calculate(CalculationRequest request)
        {
            try
            {
                _logger.LogInformation("Starting calculation: {Operand1} {Operation} {Operand2}", 
                    request.Operand1, request.Operation, request.Operand2);

                double result = request.Operation.ToLower() switch
                {
                    "add" or "+" => request.Operand1 + request.Operand2,
                    "subtract" or "-" => request.Operand1 - request.Operand2,
                    "multiply" or "*" => request.Operand1 * request.Operand2,
                    "divide" or "/" => request.Operand2 != 0 ? 
                        request.Operand1 / request.Operand2 : 
                        throw new DivideByZeroException("Division by zero is not allowed"),
                    _ => throw new ArgumentException($"Unsupported operation: {request.Operation}")
                };

                _logger.LogInformation("Calculation successful: {Operand1} {Operation} {Operand2} = {Result}", 
                    request.Operand1, request.Operation, request.Operand2, result);

                return new CalculationResponse 
                { 
                    Result = Math.Round(result, 6), // Округляем для красоты
                    Status = "Success"
                };
            }
            catch (Exception ex)
            {
                // Используем LogError чтобы ошибка попала в error-логи
                _logger.LogError(ex, "Calculation error: {Operand1} {Operation} {Operand2} - Error: {ErrorMessage}", 
                    request.Operand1, request.Operation, request.Operand2, ex.Message);
                
                return new CalculationResponse 
                { 
                    Result = 0,
                    Status = "Error",
                    Message = ex.Message
                };
            }
        }
    }
}