FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/ApiGateway/ApiGateway.Api/ApiGateway.Api.csproj", "Services/ApiGateway/ApiGateway.Api/"]
COPY ["src/Shared/LoyaltyForge.Common/LoyaltyForge.Common.csproj", "Shared/LoyaltyForge.Common/"]
RUN dotnet restore "Services/ApiGateway/ApiGateway.Api/ApiGateway.Api.csproj"
COPY src/ .
WORKDIR "/src/Services/ApiGateway/ApiGateway.Api"
RUN dotnet build "ApiGateway.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiGateway.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.Api.dll"]
