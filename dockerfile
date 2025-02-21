# Этап сборки: собираем проект
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Копируем все файлы проекта
COPY . .

# Восстанавливаем зависимости
RUN dotnet restore "XtalPlayer.csproj"

# Публикуем проект
RUN dotnet publish "XtalPlayer.csproj" -c Release -o out

# Этап выполнения: используем .NET 8.0 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Копируем опубликованный проект
COPY --from=build /app/out .

# Запуск приложения
ENTRYPOINT ["dotnet", "XtalPlayer.dll"]
