FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/Rewards/Rewards.Api/Rewards.Api.csproj", "Services/Rewards/Rewards.Api/"]
COPY ["src/Services/Rewards/Rewards.Application/Rewards.Application.csproj", "Services/Rewards/Rewards.Application/"]
COPY ["src/Services/Rewards/Rewards.Domain/Rewards.Domain.csproj", "Services/Rewards/Rewards.Domain/"]
COPY ["src/Services/Rewards/Rewards.Infrastructure/Rewards.Infrastructure.csproj", "Services/Rewards/Rewards.Infrastructure/"]
COPY ["src/Shared/LoyaltyForge.Common/LoyaltyForge.Common.csproj", "Shared/LoyaltyForge.Common/"]
COPY ["src/Shared/LoyaltyForge.Contracts/LoyaltyForge.Contracts.csproj", "Shared/LoyaltyForge.Contracts/"]
COPY ["src/Shared/LoyaltyForge.Messaging/LoyaltyForge.Messaging.csproj", "Shared/LoyaltyForge.Messaging/"]
RUN dotnet restore "Services/Rewards/Rewards.Api/Rewards.Api.csproj"
COPY src/ .
WORKDIR "/src/Services/Rewards/Rewards.Api"
RUN dotnet build "Rewards.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rewards.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rewards.Api.dll"]
