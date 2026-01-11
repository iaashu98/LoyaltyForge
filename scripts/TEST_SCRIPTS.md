# Test Scripts

This directory contains scripts for running tests in the LoyaltyForge project.

## test.sh - Test Runner Script

A comprehensive bash script for running unit tests with various options.

### Quick Start

```bash
# Run all tests
./scripts/test.sh

# Run with coverage
./scripts/test.sh -c

# Run specific service tests
./scripts/test.sh -r    # Rewards only
./scripts/test.sh -p    # Points Engine only
```

### Usage

```bash
./scripts/test.sh [options]
```

### Options

| Option | Description |
|--------|-------------|
| `-h, --help` | Show help message |
| `-a, --all` | Run all unit tests (default) |
| `-r, --rewards` | Run Rewards service tests only |
| `-p, --points` | Run Points Engine tests only |
| `-m, --messaging` | Run Messaging tests only |
| `-c, --coverage` | Run tests with code coverage |
| `-v, --verbose` | Verbose output |
| `-w, --watch` | Run tests in watch mode |
| `--clean` | Clean test artifacts before running |

### Examples

```bash
# Run all tests
./scripts/test.sh

# Run all tests with coverage
./scripts/test.sh -c

# Run Rewards tests only
./scripts/test.sh -r

# Run Points Engine tests with coverage
./scripts/test.sh -p -c

# Run tests in watch mode (auto-rerun on changes)
./scripts/test.sh -w

# Run with verbose output
./scripts/test.sh -v

# Clean test artifacts
./scripts/test.sh --clean
```

### Code Coverage

When running with `-c` flag, coverage reports are generated in `./TestResults/`.

To view HTML coverage report:

```bash
# Install report generator (one-time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
./scripts/test.sh -c

# Generate HTML report
reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:"TestResults/CoverageReport" \
  -reporttypes:Html

# Open report
open TestResults/CoverageReport/index.html  # macOS
xdg-open TestResults/CoverageReport/index.html  # Linux
```

### Watch Mode

Watch mode automatically reruns tests when code changes:

```bash
# Watch all tests
./scripts/test.sh -w

# Watch specific service
./scripts/test.sh -r -w
```

### Output

The script provides colored output:
- 🔵 Blue: Information messages
- ✅ Green: Success messages
- ❌ Red: Error messages
- ⚠️ Yellow: Warning messages

### Exit Codes

- `0`: All tests passed
- `1`: One or more test suites failed

### Integration with CI/CD

This script can be used in CI/CD pipelines:

```yaml
# GitHub Actions example
- name: Run tests
  run: ./scripts/test.sh -c
```

### Troubleshooting

**Script not executable:**
```bash
chmod +x scripts/test.sh
```

**.NET not found:**
```bash
# Install .NET 9.0 SDK
# https://dotnet.microsoft.com/download
```

**Tests failing:**
```bash
# Run with verbose output
./scripts/test.sh -v

# Clean and retry
./scripts/test.sh --clean
./scripts/test.sh
```

### Related Scripts

- `run.sh` - Service runner script
- `init-databases.sh` - Database initialization
- `schema.sql` - Database schema

---

**Happy Testing! 🧪**
