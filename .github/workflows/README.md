# CI/CD Pipeline Configuration

This directory contains GitHub Actions workflows for the LoyaltyForge project.

## Workflows

### 1. `ci-cd.yml` - Full CI/CD Pipeline
**Triggers**: Push/PR to `main` or `develop` branches

**Jobs**:
- **build-and-test**: Runs all unit tests with code coverage
- **build-services**: Builds all microservices in parallel
- **code-quality**: Runs code analysis and formatting checks

**Features**:
- ✅ Unit test execution
- ✅ Code coverage reporting (Codecov)
- ✅ Test result publishing
- ✅ Multi-service parallel builds
- ✅ Code quality checks

### 2. `unit-tests.yml` - Quick Unit Test Feedback
**Triggers**: Push/PR to any branch (including feature branches)

**Jobs**:
- **unit-tests**: Runs unit tests for all services

**Features**:
- ✅ Fast feedback on PRs
- ✅ Continues on test failures (reports all results)
- ✅ Test result comments on PRs

## Test Execution

### Local Testing
```bash
# Run all unit tests
dotnet test tests/Unit/**/*.csproj

# Run specific service tests
dotnet test tests/Unit/Rewards.Application.Tests/Rewards.Application.Tests.csproj
dotnet test tests/Unit/PointsEngine.Application.Tests/PointsEngine.Application.Tests.csproj

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### CI/CD Testing
Tests run automatically on:
- Every push to `main` or `develop`
- Every pull request
- Feature branch pushes (unit tests only)

## Test Results

Test results are:
- Published as GitHub Actions artifacts
- Displayed in PR comments
- Uploaded to Codecov for coverage tracking

## Adding New Tests

1. Create test project in `tests/Unit/`
2. Add to solution
3. Update workflows to include new test project
4. Tests will run automatically on next push

## Code Coverage

Coverage reports are generated using:
- `coverlet.collector` package
- XPlat Code Coverage format
- Uploaded to Codecov

Target coverage: **70% overall, 90% for critical business logic**

## Status Badges

Add these to your README.md:

```markdown
![CI/CD](https://github.com/iaashu98/LoyaltyForge/workflows/CI/CD%20Pipeline/badge.svg)
![Unit Tests](https://github.com/iaashu98/LoyaltyForge/workflows/Unit%20Tests%20Only/badge.svg)
[![codecov](https://codecov.io/gh/iaashu98/LoyaltyForge/branch/main/graph/badge.svg)](https://codecov.io/gh/iaashu98/LoyaltyForge)
```
