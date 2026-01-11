# LoyaltyForge

![.NET CI/CD](https://github.com/iaashu98/LoyaltyForge/workflows/.NET%20CI/CD/badge.svg)
![Unit Tests](https://github.com/iaashu98/LoyaltyForge/workflows/Unit%20Tests%20Only/badge.svg)

> A modern, event-driven multi-tenant Loyalty & Rewards SaaS platform built with .NET 9.0

## 🚀 Overview

LoyaltyForge is a production-ready microservices-based loyalty and rewards platform designed for e-commerce businesses. It features a complete **Event-Driven Architecture (EDA)** with CQRS, Saga patterns, and comprehensive testing.

### Key Features

- ✅ **Event-Driven Architecture** - RabbitMQ-based async communication
- ✅ **Multi-Tenant SaaS** - Complete tenant isolation
- ✅ **Saga Pattern** - Distributed transaction orchestration
- ✅ **CQRS** - Command/Query separation
- ✅ **Outbox Pattern** - Reliable event publishing
- ✅ **Comprehensive Testing** - 28+ unit tests with CI/CD integration
- ✅ **E-commerce Integration** - Shopify webhooks support
- ✅ **Points Engine** - Flexible points earning and redemption
- ✅ **Rewards Catalog** - Configurable reward management

---

## 🏗️ Architecture

### Microservices

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Auth & Tenant  │────▶│   API Gateway    │◀────│   E-commerce    │
│    Service      │     │   (Ocelot)       │     │   Integration   │
└─────────────────┘     └──────────────────┘     └─────────────────┘
                               │                           │
                               │                           ▼
                        ┌──────▼──────────┐      ┌─────────────────┐
                        │  Points Engine  │◀─────│   RabbitMQ      │
                        │    Service      │      │  Message Bus    │
                        └─────────────────┘      └─────────────────┘
                               │                           ▲
                               │                           │
                        ┌──────▼──────────┐               │
                        │    Rewards      │───────────────┘
                        │    Service      │
                        └─────────────────┘
```

### Event-Driven Flows

**Order Processing Flow:**
```
Shopify → OrderPlacedEvent → Points Engine → PointsEarnedEvent
```

**Reward Redemption Flow (Saga):**
```
Customer → RedemptionSaga → DeductPointsCommand → Points Engine
                ↓
    PointsDeductedEvent / PointsDeductionFailedEvent
                ↓
        Redemption Complete/Failed
```

---

## 🛠️ Technology Stack

### Backend
- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Primary database
- **RabbitMQ** - Message broker
- **Serilog** - Structured logging

### Testing
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Fluent test assertions
- **Codecov** - Code coverage tracking

### DevOps
- **Docker & Docker Compose** - Containerization
- **GitHub Actions** - CI/CD pipeline
- **Dependabot** - Automated dependency updates

---

## 🚦 Getting Started

### Prerequisites

- .NET 9.0 SDK
- Docker Desktop
- PostgreSQL 16
- RabbitMQ 3.x

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/iaashu98/LoyaltyForge.git
   cd LoyaltyForge
   ```

2. **Start infrastructure services**
   ```bash
   docker-compose up -d postgres rabbitmq
   ```

3. **Run database migrations**
   ```bash
   dotnet ef database update --project src/Services/AuthTenant/AuthTenant.Infrastructure
   dotnet ef database update --project src/Services/PointsEngine/PointsEngine.Infrastructure
   dotnet ef database update --project src/Services/Rewards/Rewards.Infrastructure
   ```

4. **Start all services**
   ```bash
   # Terminal 1 - Auth & Tenant
   dotnet run --project src/Services/AuthTenant/AuthTenant.Api

   # Terminal 2 - E-commerce Integration
   dotnet run --project src/Services/EcommerceIntegration/EcommerceIntegration.Api

   # Terminal 3 - Points Engine
   dotnet run --project src/Services/PointsEngine/PointsEngine.Api

   # Terminal 4 - Rewards Service
   dotnet run --project src/Services/Rewards/Rewards.Api
   ```

5. **Access services**
   - Auth & Tenant: http://localhost:5001
   - E-commerce Integration: http://localhost:5002
   - Points Engine: http://localhost:5003
   - Rewards Service: http://localhost:5004
   - RabbitMQ Management: http://localhost:15672 (guest/guest)

---

## 🧪 Testing

### Run Unit Tests

```bash
# Run all tests
dotnet test tests/Unit/**/*.csproj

# Run specific service tests
dotnet test tests/Unit/Rewards.Application.Tests/Rewards.Application.Tests.csproj
dotnet test tests/Unit/PointsEngine.Application.Tests/PointsEngine.Application.Tests.csproj

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage

- **Total Tests**: 28+ unit tests
- **Coverage Goal**: 70% overall, 90% for critical business logic
- **CI/CD**: Tests run automatically on every push/PR

**Tested Components:**
- ✅ Command Handlers (DeductPointsCommandHandler)
- ✅ Event Handlers (OrderPlacedEventHandler)
- ✅ Saga Orchestration (RedemptionSaga)
- ✅ Domain Entities (RewardRedemption)

---

## 📚 Documentation

### Architecture & Design
- [**EDA Implementation Guide**](docs/EDA_IMPLEMENTATION_GUIDE.md) - Complete EDA architecture
- [**Architecture Analysis**](docs/ARCHITECTURE_ANALYSIS.md) - System design decisions
- [**Testing Strategy**](docs/TESTING_STRATEGY.md) - Comprehensive testing approach

### Testing & Quality
- [**Testing Guide**](docs/TESTING_GUIDE.md) - End-to-end testing scenarios
- [**Unit Testing Summary**](docs/UNIT_TESTING_SUMMARY.md) - Test implementation details
- [**CI/CD Update Summary**](docs/CICD_UPDATE_SUMMARY.md) - Pipeline configuration

### API Documentation
- [**Auth & Tenant API Tests**](docs/api-testing/AuthTenant-API-Tests.md)
- Swagger UI available at `/swagger` on each service

---

## 🔄 CI/CD Pipeline

### Automated Workflows

**Main Pipeline** (`.NET CI/CD`)
- ✅ Full solution build
- ✅ Unit test execution with coverage
- ✅ Multi-service parallel builds
- ✅ Test result publishing
- ✅ Codecov integration

**Quick Feedback** (`Unit Tests Only`)
- ✅ Fast test execution on feature branches
- ✅ PR test result comments

**Automation** (`Dependabot`)
- ✅ Weekly dependency updates
- ✅ Security vulnerability patches

### Workflow Triggers
- **Main/Develop**: Full CI/CD pipeline
- **Feature Branches**: Quick unit tests
- **Pull Requests**: Both workflows + test results

---

## 🏛️ Project Structure

```
LoyaltyForge/
├── src/
│   ├── Services/
│   │   ├── AuthTenant/              # Authentication & tenant management
│   │   ├── EcommerceIntegration/    # Shopify webhook integration
│   │   ├── PointsEngine/            # Points earning & deduction
│   │   ├── Rewards/                 # Reward catalog & redemption
│   │   └── ApiGateway/              # API gateway (Ocelot)
│   └── Shared/
│       ├── LoyaltyForge.Common/     # Shared utilities
│       ├── LoyaltyForge.Contracts/  # Event & command contracts
│       └── LoyaltyForge.Messaging/  # RabbitMQ infrastructure
├── tests/
│   └── Unit/
│       ├── Rewards.Application.Tests/
│       ├── PointsEngine.Application.Tests/
│       └── LoyaltyForge.Messaging.Tests/
├── docs/                            # Comprehensive documentation
└── .github/workflows/               # CI/CD pipelines
```

---

## 🎯 Key Patterns Implemented

### 1. Event-Driven Architecture (EDA)
- Asynchronous communication via RabbitMQ
- Event sourcing ready
- Loose coupling between services

### 2. Saga Pattern
- Orchestration-based distributed transactions
- Compensation logic for failures
- Idempotency handling

### 3. Outbox Pattern
- Atomic message publishing
- Guaranteed event delivery
- Database transaction consistency

### 4. CQRS
- Command/Query separation
- Optimized read/write models
- Event-driven updates

---

## 🔐 Configuration

### Environment Variables

```bash
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=loyaltyforge_auth;...

# RabbitMQ
RabbitMQ__HostName=localhost
RabbitMQ__Port=5672
RabbitMQ__UserName=guest
RabbitMQ__Password=guest

# Logging
Serilog__MinimumLevel__Default=Information
```

### appsettings.json

Each service has its own `appsettings.json` for service-specific configuration.

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Write unit tests for new features
- Follow existing code patterns
- Update documentation
- Ensure CI/CD passes

---

## 📊 Project Status

### Completed ✅
- [x] Multi-tenant architecture
- [x] Event-Driven Architecture (EDA)
- [x] Saga pattern implementation
- [x] Outbox pattern for reliability
- [x] Unit testing infrastructure (28+ tests)
- [x] CI/CD pipeline with automated testing
- [x] E-commerce integration (Shopify)
- [x] Points earning and redemption
- [x] Reward catalog management

### In Progress 🚧
- [ ] Integration tests
- [ ] E2E tests
- [ ] Analytics service
- [ ] Admin dashboard
- [ ] Mobile API optimization

### Planned 📋
- [ ] Event sourcing implementation
- [ ] Read model optimization
- [ ] Advanced reporting
- [ ] Multi-platform integrations
- [ ] Performance optimization

---

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 👤 Author

**Ashutosh Ranjan**
- GitHub: [@iaashu98](https://github.com/iaashu98)

---

## 🙏 Acknowledgments

- Event-Driven Architecture patterns from industry best practices
- RabbitMQ for reliable message brokering
- .NET community for excellent tooling and support

---

**Built with ❤️ using .NET 9.0 and Event-Driven Architecture**
