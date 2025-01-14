# Base image cho runtime ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Image dùng để build project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Sao chép các file project từ thư mục gốc của repo
COPY ["CoffeeHouseAPI/CoffeeHouseAPI.csproj", "CoffeeHouseAPI/"]
COPY ["CoffeeHouseLib/CoffeeHouseLib.csproj", "CoffeeHouseLib/"]

# Khôi phục dependencies cho dự án
RUN dotnet restore "CoffeeHouseAPI/CoffeeHouseAPI.csproj"

# Sao chép toàn bộ mã nguồn từ repo vào container
COPY . .

# Build dự án
WORKDIR "/src/CoffeeHouseAPI"
RUN dotnet build "CoffeeHouseAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish CoffeeHouseAPI.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Image production cuối cùng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeeHouseAPI.dll"]
