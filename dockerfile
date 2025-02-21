# Используем базовый образ с .NET
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Скопируем необходимые файлы
COPY . .

# Указываем порт, на котором будет работать приложение
EXPOSE 2025

# Запуск приложения
ENTRYPOINT ["dotnet", "XtalPlayer.dll"]
