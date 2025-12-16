FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/PointsEngine/PointsEngine.Api/PointsEngine.Api.csproj", "Services/PointsEngine/PointsEngine.Api/"]
COPY ["src/Services/PointsEngine/PointsEngine.Application/PointsEngine.Application.csproj", "Services/PointsEngine/PointsEngine.Application/"]
COPY ["src/Services/PointsEngine/PointsEngine.Domain/PointsEngine.Domain.csproj", "Services/PointsEngine/PointsEngine.Domain/"]
COPY ["src/Services/PointsEngine/PointsEngine.Infrastructure/PointsEngine.Infrastructure.csproj", "Services/PointsEngine/PointsEngine.Infrastructure/"]
COPY ["src/Shared/LoyaltyForge.Common/LoyaltyForge.Common.csproj", "Shared/LoyaltyForge.Common/"]
COPY ["src/Shared/LoyaltyForge.Contracts/LoyaltyForge.Contracts.csproj", "Shared/LoyaltyForge.Contracts/"]
COPY ["src/Shared/LoyaltyForge.Messaging/LoyaltyForge.Messaging.csproj", "Shared/LoyaltyForge.Messaging/"]
RUN dotnet restore "Services/PointsEngine/PointsEngine.Api/PointsEngine.Api.csproj"
COPY src/ .
WORKDIR "/src/Services/PointsEngine/PointsEngine.Api"
RUN dotnet build "PointsEngine.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PointsEngine.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PointsEngine.Api.dll"]
