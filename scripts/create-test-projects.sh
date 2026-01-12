#!/bin/bash

# Script to create all remaining test projects for LoyaltyForge

set -e

echo "Creating remaining test projects..."

# AuthTenant.Infrastructure.Tests
echo "Creating AuthTenant.Infrastructure.Tests..."
mkdir -p tests/Unit/AuthTenant.Tests/AuthTenant.Infrastructure.Tests
dotnet new xunit -n AuthTenant.Infrastructure.Tests -o tests/Unit/AuthTenant.Tests/AuthTenant.Infrastructure.Tests
dotnet add tests/Unit/AuthTenant.Tests/AuthTenant.Infrastructure.Tests/AuthTenant.Infrastructure.Tests.csproj reference src/Services/AuthTenant/AuthTenant.Infrastructure/AuthTenant.Infrastructure.csproj
dotnet add tests/Unit/AuthTenant.Tests/AuthTenant.Infrastructure.Tests/AuthTenant.Infrastructure.Tests.csproj package Moq
dotnet add tests/Unit/AuthTenant.Tests/AuthTenant.Infrastructure.Tests/AuthTenant.Infrastructure.Tests.csproj package FluentAssertions
dotnet add tests/Unit/AuthTenant.Tests/AuthTenant.Infrastructure.Tests/AuthTenant.Infrastructure.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 9.0.0
rm tests/Unit/AuthTenant.Tests/AuthTenant.Infrastructure.Tests/UnitTest1.cs

# PointsEngine.Infrastructure.Tests
echo "Creating PointsEngine.Infrastructure.Tests..."
mkdir -p tests/Unit/PointsEngine.Tests/PointsEngine.Infrastructure.Tests
dotnet new xunit -n PointsEngine.Infrastructure.Tests -o tests/Unit/PointsEngine.Tests/PointsEngine.Infrastructure.Tests
dotnet add tests/Unit/PointsEngine.Tests/PointsEngine.Infrastructure.Tests/PointsEngine.Infrastructure.Tests.csproj reference src/Services/PointsEngine/PointsEngine.Infrastructure/PointsEngine.Infrastructure.csproj
dotnet add tests/Unit/PointsEngine.Tests/PointsEngine.Infrastructure.Tests/PointsEngine.Infrastructure.Tests.csproj package Moq
dotnet add tests/Unit/PointsEngine.Tests/PointsEngine.Infrastructure.Tests/PointsEngine.Infrastructure.Tests.csproj package FluentAssertions
dotnet add tests/Unit/PointsEngine.Tests/PointsEngine.Infrastructure.Tests/PointsEngine.Infrastructure.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 9.0.0
rm tests/Unit/PointsEngine.Tests/PointsEngine.Infrastructure.Tests/UnitTest1.cs

# Rewards.Application.Tests
echo "Creating Rewards.Application.Tests..."
mkdir -p tests/Unit/Rewards.Tests/Rewards.Application.Tests
dotnet new xunit -n Rewards.Application.Tests -o tests/Unit/Rewards.Tests/Rewards.Application.Tests
dotnet add tests/Unit/Rewards.Tests/Rewards.Application.Tests/Rewards.Application.Tests.csproj reference src/Services/Rewards/Rewards.Application/Rewards.Application.csproj
dotnet add tests/Unit/Rewards.Tests/Rewards.Application.Tests/Rewards.Application.Tests.csproj package Moq
dotnet add tests/Unit/Rewards.Tests/Rewards.Application.Tests/Rewards.Application.Tests.csproj package FluentAssertions
rm tests/Unit/Rewards.Tests/Rewards.Application.Tests/UnitTest1.cs

# Rewards.Infrastructure.Tests
echo "Creating Rewards.Infrastructure.Tests..."
mkdir -p tests/Unit/Rewards.Tests/Rewards.Infrastructure.Tests
dotnet new xunit -n Rewards.Infrastructure.Tests -o tests/Unit/Rewards.Tests/Rewards.Infrastructure.Tests
dotnet add tests/Unit/Rewards.Tests/Rewards.Infrastructure.Tests/Rewards.Infrastructure.Tests.csproj reference src/Services/Rewards/Rewards.Infrastructure/Rewards.Infrastructure.csproj
dotnet add tests/Unit/Rewards.Tests/Rewards.Infrastructure.Tests/Rewards.Infrastructure.Tests.csproj package Moq
dotnet add tests/Unit/Rewards.Tests/Rewards.Infrastructure.Tests/Rewards.Infrastructure.Tests.csproj package FluentAssertions
dotnet add tests/Unit/Rewards.Tests/Rewards.Infrastructure.Tests/Rewards.Infrastructure.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 9.0.0
rm tests/Unit/Rewards.Tests/Rewards.Infrastructure.Tests/UnitTest1.cs

# EcommerceIntegration.Application.Tests
echo "Creating EcommerceIntegration.Application.Tests..."
mkdir -p tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Application.Tests
dotnet new xunit -n EcommerceIntegration.Application.Tests -o tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Application.Tests
dotnet add tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Application.Tests/EcommerceIntegration.Application.Tests.csproj reference src/Services/EcommerceIntegration/EcommerceIntegration.Application/EcommerceIntegration.Application.csproj
dotnet add tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Application.Tests/EcommerceIntegration.Application.Tests.csproj package Moq
dotnet add tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Application.Tests/EcommerceIntegration.Application.Tests.csproj package FluentAssertions
rm tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Application.Tests/UnitTest1.cs

# EcommerceIntegration.Infrastructure.Tests
echo "Creating EcommerceIntegration.Infrastructure.Tests..."
mkdir -p tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Infrastructure.Tests
dotnet new xunit -n EcommerceIntegration.Infrastructure.Tests -o tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Infrastructure.Tests
dotnet add tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Infrastructure.Tests/EcommerceIntegration.Infrastructure.Tests.csproj reference src/Services/EcommerceIntegration/EcommerceIntegration.Infrastructure/EcommerceIntegration.Infrastructure.csproj
dotnet add tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Infrastructure.Tests/EcommerceIntegration.Infrastructure.Tests.csproj package Moq
dotnet add tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Infrastructure.Tests/EcommerceIntegration.Infrastructure.Tests.csproj package FluentAssertions
dotnet add tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Infrastructure.Tests/EcommerceIntegration.Infrastructure.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 9.0.0
rm tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Infrastructure.Tests/UnitTest1.cs

echo "✅ All test projects created successfully!"
echo "Total new test projects: 6"
