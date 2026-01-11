# CI/CD Workflows

This directory contains GitHub Actions workflows for the LoyaltyForge project.

## Active Workflows

### 1. `.NET CI/CD` (`dotnet.yml`)
**Triggers**: Push/PR to `main` or `develop` branches

**Jobs**:
- **build-and-test**: 
  - Restores dependencies
  - Builds entire solution
  - Runs all unit tests with code coverage
  - Publishes test results to PR
  - Uploads coverage to Codecov
  
- **build-services**: 
  - Builds all 4 microservices in parallel
  - Publishes build artifacts

**Features**:
- ✅ Full solution build
- ✅ Unit test execution
- ✅ Code coverage reporting
- ✅ Multi-service parallel builds
- ✅ Test result publishing
- ✅ Continues on test failures (reports all results)

### 2. `Unit Tests Only` (`unit-tests.yml`)
**Triggers**: Push/PR to any branch (including feature branches)

**Jobs**:
- **unit-tests**: Runs unit tests for all services

**Features**:
- ✅ Fast feedback on feature branches
- ✅ Separate test runs per service
- ✅ Test result comments on PRs
- ✅ Continues on failures

**Use Case**: Quick validation during development

### 3. `Dependabot` (`dependabot.yml`)
**Triggers**: Weekly schedule

**Updates**:
- NuGet packages
- GitHub Actions versions

---

## Workflow Strategy

### Main/Develop Branches
- **dotnet.yml** runs full CI/CD
- Builds all services
- Runs all tests
- Publishes artifacts

### Feature Branches
- **unit-tests.yml** runs for quick feedback
- Faster execution
- Test-focused

### Pull Requests
- Both workflows run
- Test results published
- Coverage tracked

---

## Test Execution

### In CI/CD
```yaml
# All unit tests
dotnet test tests/Unit/**/*.csproj

# With coverage
--collect:"XPlat Code Coverage"
```

### Locally
```bash
# Run all tests
dotnet test tests/Unit/**/*.csproj

# Run specific service
dotnet test tests/Unit/Rewards.Application.Tests/Rewards.Application.Tests.csproj
dotnet test tests/Unit/PointsEngine.Application.Tests/PointsEngine.Application.Tests.csproj

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Test Results

Test results are:
- ✅ Published as GitHub Actions check runs
- ✅ Displayed in PR comments
- ✅ Uploaded to Codecov for coverage tracking
- ✅ Available as downloadable artifacts

---

## Adding New Tests

1. Create test project in `tests/Unit/`
2. Add project reference to tested project
3. Add test packages (xUnit, Moq, FluentAssertions)
4. Tests will run automatically on next push

No workflow changes needed - the `tests/Unit/**/*.csproj` pattern catches all test projects!

---

## Status Badges

Add these to your README.md:

```markdown
![.NET CI/CD](https://github.com/iaashu98/LoyaltyForge/workflows/.NET%20CI/CD/badge.svg)
![Unit Tests](https://github.com/iaashu98/LoyaltyForge/workflows/Unit%20Tests%20Only/badge.svg)
[![codecov](https://codecov.io/gh/iaashu98/LoyaltyForge/branch/main/graph/badge.svg)](https://codecov.io/gh/iaashu98/LoyaltyForge)
```

---

## Coverage Goals

- **Overall**: 70% code coverage
- **Critical Business Logic**: 90% coverage
  - Command handlers
  - Saga orchestration
  - Domain entities
  - Event handlers

---

## Troubleshooting

### Tests failing in CI but passing locally?
- Check .NET version (should be 9.0.x)
- Ensure all dependencies are restored
- Check for environment-specific issues

### Coverage not uploading?
- Verify Codecov token is set in repository secrets
- Check coverage file generation in test output

### Workflow not triggering?
- Verify branch names match trigger patterns
- Check workflow file syntax (YAML)
- Ensure workflows are enabled in repository settings
