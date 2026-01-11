# GitHub Actions CI/CD Troubleshooting

## Common Issues and Solutions

### Issue 1: Test Projects Not Found
**Symptom**: `error MSB1009: Project file does not exist`

**Solution**: Ensure all test project paths are correct:
```yaml
tests/Unit/PointsEngine.Tests/PointsEngine.Api.Tests/PointsEngine.Api.Tests.csproj
tests/Unit/PointsEngine.Tests/PointsEngine.Application.Tests/PointsEngine.Application.Tests.csproj
tests/Unit/Rewards.Tests/Rewards.Api.Tests/Rewards.Api.Tests.csproj
tests/Unit/AuthTenant.Tests/AuthTenant.Api.Tests/AuthTenant.Api.Tests.csproj
tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Api.Tests/EcommerceIntegration.Api.Tests.csproj
```

### Issue 2: Build Before Test
**Problem**: Tests run with `--no-build` but projects weren't built

**Solution**: Always build test projects explicitly before running tests

### Issue 3: Missing Dependencies
**Problem**: Test projects reference other projects that aren't restored

**Solution**: Run `dotnet restore` before building

## Verification Steps

1. **Check workflow file syntax**:
   ```bash
   cat .github/workflows/unit-tests.yml
   ```

2. **Test locally with same commands**:
   ```bash
   dotnet restore
   dotnet build tests/Unit/PointsEngine.Tests/PointsEngine.Api.Tests/PointsEngine.Api.Tests.csproj --configuration Release
   dotnet test tests/Unit/PointsEngine.Tests/PointsEngine.Api.Tests/PointsEngine.Api.Tests.csproj --no-build --configuration Release
   ```

3. **Check GitHub Actions logs**:
   - Go to: https://github.com/iaashu98/LoyaltyForge/actions
   - Click on the failed workflow run
   - Expand the failed step to see the exact error

## Quick Fix: Simplified Workflow

If issues persist, use this simplified approach:

```yaml
- name: Build and Test All
  run: |
    dotnet restore
    dotnet build --configuration Release
    dotnet test --configuration Release --no-build --verbosity normal
```

## Current Workflow Status

- ✅ Workflows updated with correct paths
- ✅ All 5 test projects included
- ✅ Local tests passing (23/23)
- ⏳ Waiting for GitHub Actions to run

## Next Steps

1. Check the GitHub Actions tab for the specific error
2. Look for the exact line that's failing
3. Common errors to look for:
   - Project file not found
   - Missing NuGet packages
   - Build errors in dependencies
   - Test execution failures
