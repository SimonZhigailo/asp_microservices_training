# FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
# WORKDIR /app

# COPY *.csproj ./
# RUN dotnet restore

# COPY . ./
# RUN dotnet publish -c Development -o out

# FROM mcr.microsoft.com/dotnet/aspnet:9.0
# WORKDIR /app
# COPY --from=build-env /app/out .
# ENTRYPOINT [ "dotnet", "PlatformService.dll" ]

# 1. Базовый образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# 2. Копируем проект и собираем
COPY . . 
RUN dotnet restore
RUN dotnet publish -c Release -o /publish

# 3. Базовый образ .NET Runtime для запуска
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /publish .
# 4. Запуск приложения
CMD ["dotnet", "PlatformService.dll"]