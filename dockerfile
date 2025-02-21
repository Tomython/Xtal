# Этап сборки: собираем проект
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Копируем проект
COPY XtalPlayer.csproj .

# Восстанавливаем зависимости
RUN dotnet restore "XtalPlayer.csproj" --verbosity detailed

# Копируем остальные файлы
COPY . .

# Публикуем проект
RUN dotnet publish "XtalPlayer.csproj" -c Release -o /app/publish

# Этап выполнения: используем .NET 8.0 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Копируем опубликованный проект
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "XtalPlayer.dll"]