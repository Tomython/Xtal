# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем все файлы проекта
COPY . ./
RUN dotnet restore

# Публикация приложения
RUN dotnet publish -c Release -o out

# Этап выполнения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Запуск приложения
ENTRYPOINT ["dotnet", "XtalPlayer.dll"]

