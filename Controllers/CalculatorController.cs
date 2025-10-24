using Microsoft.AspNetCore.Mvc;
using CalculatorApi.Models;
using CalculatorApi.Services;

namespace CalculatorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly ICalculatorService _calculatorService;
        private readonly ILogger<CalculatorController> _logger;

        public CalculatorController(
            ICalculatorService calculatorService,
            ILogger<CalculatorController> logger)
        {
            _calculatorService = calculatorService;
            _logger = logger;
        }

        [HttpPost("calculate")]
        public ActionResult<CalculationResponse> Calculate([FromBody] CalculationRequest request)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            
            _logger.LogInformation("Calculation request received from {ClientIP}: {Operand1} {Operation} {Operand2}", 
                clientIp, request.Operand1, request.Operation, request.Operand2);

            var result = _calculatorService.Calculate(request);

            if (result.Status == "Error")
            {
                _logger.LogWarning("Calculation failed for {ClientIP}: {Operand1} {Operation} {Operand2} - {Message}", 
                    clientIp, request.Operand1, request.Operation, request.Operand2, result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Calculation successful for {ClientIP}: {Operand1} {Operation} {Operand2} = {Result}", 
                clientIp, request.Operand1, request.Operation, request.Operand2, result.Result);
            
            return Ok(result);
        }

        [HttpGet("operations")]
        public ActionResult<object> GetSupportedOperations()
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            _logger.LogInformation("Operations list requested from {ClientIP}", clientIp);

            var operations = new
            {
                SupportedOperations = new[]
                {
                    new { Name = "Add", Symbols = new[] { "add", "+" } },
                    new { Name = "Subtract", Symbols = new[] { "subtract", "-" } },
                    new { Name = "Multiply", Symbols = new[] { "multiply", "*" } },
                    new { Name = "Divide", Symbols = new[] { "divide", "/" } }
                }
            };

            return Ok(operations);
        }
    }
}