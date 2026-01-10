# Testing Strategy for LoyaltyForge EDA

> **Purpose**: Comprehensive testing approach for Event-Driven Architecture  
> **Priority**: High - Ensures reliability and maintainability

## Testing Pyramid

```
        /\
       /  \      E2E Tests (5%)
      /____\     - Full system flows
     /      \    Integration Tests (25%)
    /________\   - Service boundaries, RabbitMQ, DB
   /          \  Unit Tests (70%)
  /____________\ - Business logic, handlers, sagas
```

---

## 1. Unit Tests (Priority: HIGH)

### What to Test

#### Saga Logic
**File**: `RedemptionSaga.cs`

**Tests:**
- ✅ `StartRedemptionAsync_WithValidData_CreatesRedemption`
- ✅ `StartRedemptionAsync_WithDuplicateIdempotencyKey_ReturnsExisting`
- ✅ `StartRedemptionAsync_WithInactiveReward_ReturnsFailed`
- ✅ `HandlePointsDeductedAsync_UpdatesRedemptionToFulfilled`
- ✅ `HandlePointsDeductionFailedAsync_UpdatesRedemptionToFailed`

**Example:**
```csharp
public class RedemptionSagaTests
{
    private readonly Mock<IRedemptionRepository> _redemptionRepo;
    private readonly Mock<IRewardRepository> _rewardRepo;
    private readonly Mock<ICommandPublisher> _commandPublisher;
    private readonly RedemptionSaga _saga;

    [Fact]
    public async Task StartRedemptionAsync_WithValidData_CreatesRedemption()
    {
        // Arrange
        var reward = CreateActiveReward(pointsCost: 50);
        _rewardRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(reward);
        
        // Act
        var result = await _saga.StartRedemptionAsync(
            tenantId, customerId, rewardId, "key-1");
        
        // Assert
        Assert.NotNull(result.RedemptionId);
        Assert.Equal("Pending", result.Status);
        _redemptionRepo.Verify(x => x.AddAsync(
            It.Is<RewardRedemption>(r => r.Status == "Pending"), 
            default), Times.Once);
        _commandPublisher.Verify(x => x.SendAsync(
            It.IsAny<DeductPointsCommand>(), 
            "points.commands", 
            default), Times.Once);
    }
}
```

#### Event Handlers
**Files**: `OrderPlacedEventHandler.cs`, `DeductPointsCommandHandler.cs`

**Tests:**
- ✅ `HandleAsync_WithValidOrder_EarnsPoints`
- ✅ `HandleAsync_WithDuplicateEventId_SkipsProcessing`
- ✅ `HandleAsync_WithSufficientBalance_DeductsPoints`
- ✅ `HandleAsync_WithInsufficientBalance_PublishesFailureEvent`

#### Domain Entities
**Files**: `RewardRedemption.cs`, `RewardCatalog.cs`

**Tests:**
- ✅ `Create_WithValidData_ReturnsRedemption`
- ✅ `MarkFulfilled_UpdatesStatusAndTimestamp`
- ✅ `MarkFailed_UpdatesStatus`

---

## 2. Integration Tests (Priority: MEDIUM)

### What to Test

#### RabbitMQ Integration
**Test**: Message publishing and consumption

```csharp
public class RabbitMQIntegrationTests : IClassFixture<RabbitMQFixture>
{
    [Fact]
    public async Task EventPublisher_PublishesEvent_ConsumerReceives()
    {
        // Arrange
        var @event = new OrderPlacedEvent { /* ... */ };
        var received = new TaskCompletionSource<OrderPlacedEvent>();
        
        await _consumer.SubscribeAsync<OrderPlacedEvent>(
            "test.queue", 
            e => { received.SetResult(e); return Task.CompletedTask; });
        
        // Act
        await _publisher.PublishAsync(@event);
        
        // Assert
        var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(@event.EventId, result.EventId);
    }
}
```

#### Database Integration
**Test**: Repository operations with real database

```csharp
public class RedemptionRepositoryTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task AddAsync_SavesRedemption_CanBeRetrieved()
    {
        // Arrange
        var redemption = RewardRedemption.Create(/* ... */);
        
        // Act
        await _repository.AddAsync(redemption);
        var retrieved = await _repository.GetByIdAsync(redemption.Id);
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(redemption.Id, retrieved.Id);
    }
}
```

#### Outbox Pattern
**Test**: Outbox publisher processes messages

```csharp
public class OutboxPublisherTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task OutboxPublisher_ProcessesPendingMessages()
    {
        // Arrange
        await _outboxRepo.AddAsync(new OutboxMessage { /* ... */ });
        
        // Act
        await _outboxPublisher.PublishPendingAsync();
        
        // Assert
        var pending = await _outboxRepo.GetPendingAsync(10);
        Assert.Empty(pending); // All processed
    }
}
```

---

## 3. End-to-End Tests (Priority: LOW)

### What to Test

#### Complete Saga Flow
```csharp
public class RedemptionE2ETests : IClassFixture<WebApplicationFactory>
{
    [Fact]
    public async Task CompleteRedemptionFlow_Success()
    {
        // Arrange - Create customer with points
        await CreateCustomerWithPoints(customerId, 100);
        var reward = await CreateReward(pointsCost: 50);
        
        // Act - Redeem reward
        var response = await _client.PostAsJsonAsync(
            $"/api/tenants/{tenantId}/redemptions",
            new { customerId, rewardId = reward.Id, idempotencyKey = "test-1" });
        
        // Assert - Verify redemption completed
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RedeemRewardResult>();
        Assert.True(result.Success);
        
        // Verify points deducted
        var balance = await GetCustomerBalance(customerId);
        Assert.Equal(50, balance);
    }
}
```

---

## 4. Test Infrastructure

### Required NuGet Packages

```xml
<ItemGroup>
  <!-- Unit Testing -->
  <PackageReference Include="xunit" Version="2.6.6" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  
  <!-- Integration Testing -->
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
  <PackageReference Include="Testcontainers" Version="3.7.0" />
  <PackageReference Include="Testcontainers.PostgreSql" Version="3.7.0" />
  <PackageReference Include="Testcontainers.RabbitMq" Version="3.7.0" />
  
  <!-- Test Data -->
  <PackageReference Include="Bogus" Version="35.4.0" />
</ItemGroup>
```

### Test Fixtures

#### DatabaseFixture (for Integration Tests)
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    public RewardsDbContext DbContext { get; private set; }
    
    public DatabaseFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var options = new DbContextOptionsBuilder<RewardsDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
            
        DbContext = new RewardsDbContext(options);
        await DbContext.Database.MigrateAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
```

#### RabbitMQFixture
```csharp
public class RabbitMQFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container;
    public IEventPublisher Publisher { get; private set; }
    public IEventConsumer Consumer { get; private set; }
    
    public async Task InitializeAsync()
    {
        _container = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .Build();
            
        await _container.StartAsync();
        
        var options = new RabbitMQOptions
        {
            HostName = _container.Hostname,
            Port = _container.GetMappedPublicPort(5672),
            UserName = "guest",
            Password = "guest"
        };
        
        Publisher = new RabbitMQEventPublisher(Options.Create(options), /* ... */);
        Consumer = new RabbitMQEventConsumer(Options.Create(options), /* ... */);
    }
}
```

---

## 5. Implementation Plan

### Phase 1: Unit Tests (Week 1)
- [ ] Set up test projects for each service
- [ ] Add Moq, xUnit, FluentAssertions packages
- [ ] Write saga tests (RedemptionSaga)
- [ ] Write event handler tests
- [ ] Write domain entity tests
- [ ] Target: 70% code coverage

### Phase 2: Integration Tests (Week 2)
- [ ] Add Testcontainers packages
- [ ] Create database fixtures
- [ ] Create RabbitMQ fixtures
- [ ] Write repository integration tests
- [ ] Write outbox publisher tests
- [ ] Write RabbitMQ integration tests

### Phase 3: E2E Tests (Week 3)
- [ ] Set up WebApplicationFactory
- [ ] Write complete saga flow tests
- [ ] Write failure scenario tests
- [ ] Add performance tests

---

## 6. Recommended Test Structure

```
LoyaltyForge/
├── tests/
│   ├── Unit/
│   │   ├── Rewards.Application.Tests/
│   │   │   ├── Sagas/
│   │   │   │   └── RedemptionSagaTests.cs
│   │   │   └── EventHandlers/
│   │   │       ├── PointsDeductedEventHandlerTests.cs
│   │   │       └── PointsDeductionFailedEventHandlerTests.cs
│   │   ├── PointsEngine.Application.Tests/
│   │   │   ├── EventHandlers/
│   │   │   │   └── OrderPlacedEventHandlerTests.cs
│   │   │   └── CommandHandlers/
│   │   │       └── DeductPointsCommandHandlerTests.cs
│   │   └── LoyaltyForge.Messaging.Tests/
│   │       ├── RabbitMQEventPublisherTests.cs
│   │       └── OutboxPublisherTests.cs
│   ├── Integration/
│   │   ├── Rewards.Integration.Tests/
│   │   │   ├── Fixtures/
│   │   │   │   └── DatabaseFixture.cs
│   │   │   └── Repositories/
│   │   │       └── RedemptionRepositoryTests.cs
│   │   └── Messaging.Integration.Tests/
│   │       ├── Fixtures/
│   │       │   └── RabbitMQFixture.cs
│   │       └── RabbitMQIntegrationTests.cs
│   └── E2E/
│       └── LoyaltyForge.E2E.Tests/
│           ├── Fixtures/
│           │   └── WebApplicationFactory.cs
│           └── Flows/
│               └── RedemptionFlowTests.cs
```

---

## 7. CI/CD Integration

### GitHub Actions Workflow
```yaml
name: Tests

on: [push, pull_request]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - run: dotnet test tests/Unit/**/*.csproj
      
  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - run: dotnet test tests/Integration/**/*.csproj
      
  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - run: dotnet test tests/E2E/**/*.csproj
```

---

## 8. Benefits

### Immediate
- ✅ Catch bugs early in development
- ✅ Prevent regressions
- ✅ Document expected behavior
- ✅ Enable confident refactoring

### Long-term
- ✅ Reduce production incidents
- ✅ Faster onboarding for new developers
- ✅ Improved code quality
- ✅ Better architecture decisions

---

## Recommendation

**Start with Unit Tests** for critical paths:
1. **RedemptionSaga** - Core business logic
2. **DeductPointsCommandHandler** - Critical for redemptions
3. **OrderPlacedEventHandler** - Revenue-impacting

Then add **Integration Tests** for:
1. **RabbitMQ** - Ensure messages flow correctly
2. **Outbox Pattern** - Verify reliability
3. **Repositories** - Database operations

**E2E tests** can wait until you have good unit/integration coverage.

**Target Coverage**: 70% overall, 90% for critical business logic
