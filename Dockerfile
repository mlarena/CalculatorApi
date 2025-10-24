# Этап 1: Сборка приложения
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CalculatorApi.csproj", "."]
RUN dotnet restore "CalculatorApi.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "CalculatorApi.csproj" -c Release -o /app/build

# Этап 2: Публикация
FROM build AS publish
RUN dotnet publish "CalculatorApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап 3: Runtime-окружение
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Порт для внешнего доступа (можно изменить)
EXPOSE 8080
ENTRYPOINT ["dotnet", "CalculatorApi.dll"]