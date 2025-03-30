# Этап сборки: используем SDK (не ASP.NET!)
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

# Этап выполнения: используем ASP.NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Копируем опубликованный проект ИЗ ЭТАПА "build"
COPY --from=build /app/publish .

# Указываем порт
ENV ASPNETCORE_URLS=http://*:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "XtalPlayer.dll"]