FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/AuthTenant/AuthTenant.Api/AuthTenant.Api.csproj", "Services/AuthTenant/AuthTenant.Api/"]
COPY ["src/Services/AuthTenant/AuthTenant.Application/AuthTenant.Application.csproj", "Services/AuthTenant/AuthTenant.Application/"]
COPY ["src/Services/AuthTenant/AuthTenant.Domain/AuthTenant.Domain.csproj", "Services/AuthTenant/AuthTenant.Domain/"]
COPY ["src/Services/AuthTenant/AuthTenant.Infrastructure/AuthTenant.Infrastructure.csproj", "Services/AuthTenant/AuthTenant.Infrastructure/"]
COPY ["src/Shared/LoyaltyForge.Common/LoyaltyForge.Common.csproj", "Shared/LoyaltyForge.Common/"]
COPY ["src/Shared/LoyaltyForge.Contracts/LoyaltyForge.Contracts.csproj", "Shared/LoyaltyForge.Contracts/"]
COPY ["src/Shared/LoyaltyForge.Messaging/LoyaltyForge.Messaging.csproj", "Shared/LoyaltyForge.Messaging/"]
RUN dotnet restore "Services/AuthTenant/AuthTenant.Api/AuthTenant.Api.csproj"
COPY src/ .
WORKDIR "/src/Services/AuthTenant/AuthTenant.Api"
RUN dotnet build "AuthTenant.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthTenant.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthTenant.Api.dll"]
