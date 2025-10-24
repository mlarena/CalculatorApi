# CalculatorApi: Учебный проект Web API с Docker-развёртыванием

## Описание

Этот проект представляет собой учебное приложение на базе .NET 9.0, демонстрирующее процесс создания простого Web API для выполнения базовых математических операций (сложение, вычитание, умножение, деление). Основная цель — иллюстрировать этапы разработки API, его публикации в Docker-контейнере и, в первую очередь, настройку логирования, где файлы логов сохраняются на хостовой машине для удобства мониторинга и анализа вне контейнера.

Проект использует ASP.NET Core для API, Serilog для структурированного логирования с разделением на уровни (app и err), Swagger для документации и Docker Compose для оркестрации. Это позволяет сосредоточиться на ключевых практиках: dependency injection, middleware, обработка ошибок и персистентность данных в контейнеризированной среде.

## Требования

- **.NET SDK 9.0** (установите с [официального сайта Microsoft](https://dotnet.microsoft.com/download/dotnet/9.0)).
- **Docker Desktop** (версия 4.0+ с поддержкой WSL 2 или Hyper-V).
- **Visual Studio Code** или **Visual Studio Community** (рекомендуется для разработки).
- **Git** (для клонирования и управления репозиторием).

## Установка и сборка

1. Клонируйте репозиторий:
   ```
   git clone <URL-РЕПОЗИТОРИЯ>
   cd CalculatorApi
   ```

2. Восстановите пакеты NuGet:
   ```
   dotnet restore
   ```

3. Соберите проект:
   ```
   dotnet build
   ```

Проект использует минимальный шаблон Web API с контроллером `CalculatorController` для обработки запросов в формате JSON.

## Запуск локально

Для запуска без Docker выполните:
```
dotnet run
```

- API доступно по адресу: `http://localhost:5000/swagger` (Swagger UI для тестирования).
- Пример запроса (POST `/api/calculator/calculate`):
  ```json
  {
    "operand1": 5,
    "operand2": 3,
    "operation": "+"
  }
  ```
  Ожидаемый ответ:
  ```json
  {
    "result": 8,
    "status": "Success"
  }
  ```

Логи по умолчанию записываются в локальную папку `./logs/app/` и `./logs/err/`.

## Развёртывание в Docker

Проект настроен для контейнеризации с акцентом на персистентность логов. Основные файлы:
- `Dockerfile`: Многоэтапная сборка (build → publish → runtime).
- `docker-compose.yml`: Оркестрация с монтированием volumes.

### Шаги по запуску в Docker

1. Создайте директорию для логов на хосте:
   ```
   mkdir -p C:\Docker_directories\logs\app
   mkdir -p C:\Docker_directories\logs\err
   ```

2. Соберите и запустите контейнер:
   ```
   docker-compose up --build
   ```

- API доступно по адресу: `http://localhost:5111/swagger`.
- Логи сохраняются на хосте в `C:\Docker_directories\logs\app\log-YYYYMMDD.json` (успешные) и `C:\Docker_directories\logs\err\error-YYYYMMDD.json` (ошибки), с ротацией по дням и лимитом в 30 файлов.
- Остановка: `docker-compose down`.

### Ключевые особенности Docker-развёртывания

- **Порт**: Хост: 5111 → Контейнер: 8080.
- **Volumes**: Монтирование `C:/Docker_directories/logs:/app/logs` обеспечивает запись логов напрямую на хост, без дублирования в контейнере.
- **Окружение**: `ASPNETCORE_ENVIRONMENT=Development` для активации Swagger.
- **Размер образа**: ~200–300 МБ (логи не влияют на размер, так как хранятся вне образа).

## Логирование

Логирование реализовано с использованием Serilog, с разделением по уровням:
- **/logs/app/**: Логи Information и Warning (успешные запросы, метрики).
- **/logs/err/**: Логи Error и Fatal (исключения, ошибки).
- **Формат**: Compact JSON, с обогащением контекста (включая IP-адрес клиента через middleware).
- **Ротация**: Ежедневная, с хранением 30 файлов.
- **В Docker**: Благодаря volume, файлы доступны на хосте для анализа (например, через ELK Stack или простые инструменты).

Пример лога (JSON):
```json
{
  "Timestamp": "2025-10-24T11:09:49",
  "Level": "Information",
  "Message": "Incoming request from 127.0.0.1: POST /api/calculator/calculate",
  "Properties": { "ClientIP": "127.0.0.1" }
}
```

## Структура проекта

```
CalculatorApi/
├── Controllers/          # API-контроллеры (CalculatorController.cs)
├── Middleware/           # Кастомный middleware для логирования (LoggingMiddleware.cs)
├── Models/               # Модели запросов/ответов (CalculationRequest.cs, CalculationResponse.cs)
├── Services/             # Бизнес-логика (CalculatorService.cs, ICalculatorService.cs)
├── Properties/           # Настройки запуска (launchSettings.json)
├── Dockerfile            # Инструкции сборки контейнера
├── docker-compose.yml    # Оркестрация Docker
├── Program.cs            # Входная точка с настройкой Serilog и пайплайна
├── CalculatorApi.csproj  # Зависимости NuGet
└── README.md             # Данная документация
```

## Лицензия

Этот проект распространяется под лицензией MIT. Вы можете свободно использовать, модифицировать и распространять его для образовательных или коммерческих целей, с указанием авторства.

## Автор и контакты

Проект создан в образовательных целях. За дополнительными вопросами обращайтесь к репозиторию или автору.

---

*Обновлено: 24 октября 2025 г.*
