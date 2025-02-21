# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем файл проекта и восстанавливаем зависимости
COPY XtalPlayer.csproj ./
RUN dotnet restore

# Копируем все файлы проекта
COPY . ./
RUN dotnet publish -c Release -o out

# Этап выполнения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Указываем порт
EXPOSE 2025

# Запуск приложения
ENTRYPOINT ["dotnet", "XtalPlayer.dll"]

