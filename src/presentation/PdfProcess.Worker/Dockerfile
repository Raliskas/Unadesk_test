# Простой вариант - копируем всё сразу
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

# Восстанавливаем зависимости (найдет все csproj автоматически)
RUN dotnet restore "src/presentation/PdfProcess.Worker/PdfProcess.Worker.csproj"

# Собираем
RUN dotnet publish "src/presentation/PdfProcess.Worker/PdfProcess.Worker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PdfProcess.Worker.dll"]