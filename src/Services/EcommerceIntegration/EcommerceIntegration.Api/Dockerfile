FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/EcommerceIntegration/EcommerceIntegration.Api/EcommerceIntegration.Api.csproj", "Services/EcommerceIntegration/EcommerceIntegration.Api/"]
COPY ["src/Services/EcommerceIntegration/EcommerceIntegration.Application/EcommerceIntegration.Application.csproj", "Services/EcommerceIntegration/EcommerceIntegration.Application/"]
COPY ["src/Services/EcommerceIntegration/EcommerceIntegration.Domain/EcommerceIntegration.Domain.csproj", "Services/EcommerceIntegration/EcommerceIntegration.Domain/"]
COPY ["src/Services/EcommerceIntegration/EcommerceIntegration.Infrastructure/EcommerceIntegration.Infrastructure.csproj", "Services/EcommerceIntegration/EcommerceIntegration.Infrastructure/"]
COPY ["src/Shared/LoyaltyForge.Common/LoyaltyForge.Common.csproj", "Shared/LoyaltyForge.Common/"]
COPY ["src/Shared/LoyaltyForge.Contracts/LoyaltyForge.Contracts.csproj", "Shared/LoyaltyForge.Contracts/"]
COPY ["src/Shared/LoyaltyForge.Messaging/LoyaltyForge.Messaging.csproj", "Shared/LoyaltyForge.Messaging/"]
RUN dotnet restore "Services/EcommerceIntegration/EcommerceIntegration.Api/EcommerceIntegration.Api.csproj"
COPY src/ .
WORKDIR "/src/Services/EcommerceIntegration/EcommerceIntegration.Api"
RUN dotnet build "EcommerceIntegration.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EcommerceIntegration.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcommerceIntegration.Api.dll"]
