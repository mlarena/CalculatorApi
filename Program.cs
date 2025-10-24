using CalculatorApi.Middleware;
using CalculatorApi.Services;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;

var builder = WebApplication.CreateBuilder(args);

// Bootstrap-логгер для ранней инициализации (предотвращает hangs)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    // Создание папок для логов (относительно корня проекта)
    string logsPath = Path.Combine(builder.Environment.ContentRootPath, "logs");
    Directory.CreateDirectory(Path.Combine(logsPath, "app"));
    Directory.CreateDirectory(Path.Combine(logsPath, "err"));

    // Очищаем стандартные провайдеры ДО добавления Serilog
    builder.Logging.ClearProviders();

    // Настройка Serilog через AddSerilog с правильной сигнатурой делегата
    builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration) // Опционально: из appsettings.json
        .ReadFrom.Services(services) // Интеграция с DI
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext() // Добавляет контекст (включая IP из middleware)
        // Общие логи в /logs/app (исключая ошибки)
        .WriteTo.Logger(lc => lc
            .Filter.ByExcluding(e => e.Level == LogEventLevel.Error || e.Level == LogEventLevel.Fatal)
            .WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(logsPath, "app", "log-.json"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true, // Совместный доступ к файлу
                flushToDiskInterval: TimeSpan.FromSeconds(1) // Периодическая запись
            )
        )
        // Ошибки в /logs/err
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error || e.Level == LogEventLevel.Fatal)
            .WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(logsPath, "err", "error-.json"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true, // Совместный доступ к файлу
                flushToDiskInterval: TimeSpan.FromSeconds(1) // Периодическая запись
            )
        )
        // Консоль для отладки
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    );

    // Дополнительные сервисы
    builder.Services.AddControllers(); // Обязательно для API-контроллеров
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Calculator API",
            Version = "v1",
            Description = "Калькулятор с логированием"
        });
    });
    builder.Services.AddScoped<ICalculatorService, CalculatorService>();

    var app = builder.Build();

    // Пайплайн
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Calculator API V1"));
    }

    app.UseMiddleware<LoggingMiddleware>(); // Включает IP в логи
    // app.UseAuthorization(); // Закомментировано, если не нужно
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}