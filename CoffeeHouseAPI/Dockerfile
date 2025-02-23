# Base image cho runtime ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Tạo user không phải root để chạy ứng dụng an toàn
RUN useradd -m appuser
USER appuser

# Image dùng để build project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Sao chép các file project
COPY ["CoffeeHouseAPI/CoffeeHouseAPI.csproj", "CoffeeHouseAPI/"]
COPY ["CoffeeHouseLib/CoffeeHouseLib.csproj", "CoffeeHouseLib/"]

# Khôi phục dependencies
RUN dotnet restore "CoffeeHouseAPI/CoffeeHouseAPI.csproj"

# Sao chép toàn bộ mã nguồn (chỉ thư mục cần thiết)
COPY CoffeeHouseAPI/ CoffeeHouseAPI/
COPY CoffeeHouseLib/ CoffeeHouseLib/

# Build project
WORKDIR "/src/CoffeeHouseAPI"
RUN dotnet build "CoffeeHouseAPI.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

FROM build AS publish
# Publish với cấu hình tối ưu
RUN dotnet publish CoffeeHouseAPI.csproj -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

# Image production cuối cùng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy thư mục File nếu cần
COPY CoffeeHouseAPI/File /app/File

ENTRYPOINT ["dotnet", "CoffeeHouseAPI.dll"]
