# 🚀 Quick Start Guide - LoyaltyForge

## Build & Run in One Command

```bash
# Build and run all services
./scripts/run.sh -b

# Build and run in background (detached mode)
./scripts/run.sh -b -d
```

That's it! Your entire application will be built and running.

---

## 📋 Common Commands

### Build & Run
```bash
./scripts/run.sh -b              # Build & run all services
./scripts/run.sh -b -d           # Build & run in background
./scripts/run.sh -b -i -t        # Build & run infra + Auth service
```

### Build Only
```bash
./scripts/build.sh               # Build all services
./scripts/build.sh -t -p         # Build Auth + Points Engine only
./scripts/build.sh -c -a         # Clean and build all
```

### Run Only (without building)
```bash
./scripts/run.sh                 # Run all services
./scripts/run.sh -d              # Run in background
./scripts/run.sh -i              # Run infrastructure only
```

### Testing
```bash
./scripts/test.sh                # Run all tests (23 tests)
./scripts/test.sh -p             # Run Points Engine tests only
./scripts/test.sh -c             # Run with code coverage
```

### View Logs & Stop
```bash
./scripts/run.sh -l              # View all logs
./scripts/run.sh -l -t           # View Auth service logs
./scripts/run.sh -s              # Stop all services
docker compose ps                # View running containers
```

---

## 🌐 Service URLs

Once running, access your services at:

| Service | URL | Description |
|---------|-----|-------------|
| **API Gateway** | http://localhost:5000 | Main entry point |
| **Auth+Tenant** | http://localhost:5001 | Authentication & multi-tenancy |
| **E-commerce** | http://localhost:5002 | Shopify integration |
| **Points Engine** | http://localhost:5003 | Points calculation & rules |
| **Rewards** | http://localhost:5004 | Reward catalog & redemptions |
| **RabbitMQ UI** | http://localhost:15672 | Message queue (guest/guest) |
| **PostgreSQL** | localhost:5432 | Database |

---

## 📖 Detailed Documentation

- **Architecture**: [`docs/ARCHITECTURE_ANALYSIS.md`](docs/ARCHITECTURE_ANALYSIS.md)
- **Testing Guide**: [`docs/TESTING_GUIDE.md`](docs/TESTING_GUIDE.md)
- **Testing Strategy**: [`docs/TESTING_STRATEGY.md`](docs/TESTING_STRATEGY.md)
- **EDA Implementation**: [`docs/EDA_IMPLEMENTATION_GUIDE.md`](docs/EDA_IMPLEMENTATION_GUIDE.md)
- **API Testing**: [`docs/api-testing/`](docs/api-testing/)

---

## 🛠️ Development Workflow

### 1. First Time Setup
```bash
# Clone the repository
git clone https://github.com/iaashu98/LoyaltyForge.git
cd LoyaltyForge

# Build and run
./scripts/run.sh -b
```

### 2. Daily Development
```bash
# Make your changes...

# Run tests
./scripts/test.sh

# Build and run
./scripts/run.sh -b

# View logs
./scripts/run.sh -l
```

### 3. Before Committing
```bash
# Run all tests
./scripts/test.sh

# Ensure everything builds
./scripts/build.sh

# Commit your changes
git add .
git commit -m "your message"
git push
```

---

## 🎯 Script Options Reference

### `./scripts/run.sh` Options
- `-a, --all` - Run all services (default)
- `-i, --infra` - Run infrastructure only (postgres, rabbitmq)
- `-g, --gateway` - Run API Gateway
- `-t, --auth` - Run Auth+Tenant service
- `-e, --ecommerce` - Run E-commerce Integration
- `-p, --points` - Run Points Engine
- `-r, --rewards` - Run Rewards service
- `-d, --detached` - Run in background
- `-b, --build` - Build before running
- `-s, --stop` - Stop services
- `-l, --logs` - Show logs
- `-h, --help` - Show help

### `./scripts/build.sh` Options
- `-a, --all` - Build all services (default)
- `-g, --gateway` - Build API Gateway
- `-t, --auth` - Build Auth+Tenant
- `-e, --ecommerce` - Build E-commerce Integration
- `-p, --points` - Build Points Engine
- `-r, --rewards` - Build Rewards
- `-s, --shared` - Build shared libraries only
- `-c, --clean` - Clean before building
- `-d, --docker` - Build Docker images
- `-h, --help` - Show help

### `./scripts/test.sh` Options
- `-a, --all` - Run all tests (default)
- `-r, --rewards` - Run Rewards tests
- `-p, --points` - Run Points Engine tests
- `-t, --authtenant` - Run AuthTenant tests
- `-e, --ecommerce` - Run EcommerceIntegration tests
- `-c, --coverage` - Run with code coverage
- `-v, --verbose` - Verbose output
- `-w, --watch` - Watch mode
- `--clean` - Clean test artifacts

---

## 🐛 Troubleshooting

### Services won't start
```bash
# Stop everything and try again
./scripts/run.sh -s
docker compose down -v
./scripts/run.sh -b
```

### Tests failing
```bash
# Clean and rebuild
./scripts/build.sh -c -a
./scripts/test.sh
```

### Port conflicts
Check if ports 5000-5004, 5432, 5672, or 15672 are already in use:
```bash
lsof -i :5000
lsof -i :5432
```

---

## 📚 More Information

For detailed information, see:
- [Full README](README.md)
- [Scripts Documentation](scripts/README.md)
- [Testing Documentation](docs/TESTING_GUIDE.md)

---

**Need help?** Run any script with `-h` or `--help` flag:
```bash
./scripts/run.sh -h
./scripts/build.sh -h
./scripts/test.sh -h
```
