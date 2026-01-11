#!/bin/bash

# LoyaltyForge Test Runner Script
# Usage: ./scripts/test.sh [options]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
COVERAGE=false
VERBOSE=false
SERVICE=""
WATCH=false

# Function to print colored output
print_info() {
    echo -e "${BLUE}ℹ ${1}${NC}"
}

print_success() {
    echo -e "${GREEN}✓ ${1}${NC}"
}

print_error() {
    echo -e "${RED}✗ ${1}${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ ${1}${NC}"
}

# Function to display help
show_help() {
    cat <<EOF
LoyaltyForge Test Runner

Usage: ./scripts/test.sh [options]

Options:
    -h, --help              Show this help message
    -a, --all               Run all unit tests (default)
    -r, --rewards           Run Rewards service tests only
    -p, --points            Run Points Engine tests only
    -t, --authtenant        Run AuthTenant tests only
    -e, --ecommerce         Run EcommerceIntegration tests only
    -c, --coverage          Run tests with code coverage
    -v, --verbose           Verbose output
    -w, --watch             Run tests in watch mode
    --clean                 Clean test artifacts before running

Examples:
    ./scripts/test.sh                    # Run all tests
    ./scripts/test.sh -r                 # Run Rewards tests only
    ./scripts/test.sh -c                 # Run all tests with coverage
    ./scripts/test.sh -p -c              # Run Points Engine tests with coverage
    ./scripts/test.sh -w                 # Run all tests in watch mode

EOF
}

# Function to clean test artifacts
clean_artifacts() {
    print_info "Cleaning test artifacts..."
    rm -rf tests/**/bin tests/**/obj
    rm -rf TestResults
    print_success "Test artifacts cleaned"
}

# Function to run tests
run_tests() {
    local project=$1
    local project_name=$2
    
    print_info "Running ${project_name} tests..."
    
    local cmd="dotnet test ${project}"
    
    if [ "$COVERAGE" = true ]; then
        cmd="${cmd} --collect:\"XPlat Code Coverage\" --results-directory ./TestResults/${project_name}"
    fi
    
    if [ "$VERBOSE" = true ]; then
        cmd="${cmd} --verbosity detailed"
    else
        cmd="${cmd} --verbosity normal"
    fi
    
    if [ "$WATCH" = true ]; then
        cmd="dotnet watch test ${project}"
    fi
    
    if eval $cmd; then
        print_success "${project_name} tests passed"
        return 0
    else
        print_error "${project_name} tests failed"
        return 1
    fi
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            show_help
            exit 0
            ;;
        -a|--all)
            SERVICE="all"
            shift
            ;;
        -r|--rewards)
            SERVICE="rewards"
            shift
            ;;
        -p|--points)
            SERVICE="points"
            shift
            ;;
        -t|--authtenant)
            SERVICE="authtenant"
            shift
            ;;
        -e|--ecommerce)
            SERVICE="ecommerce"
            shift
            ;;
        -c|--coverage)
            COVERAGE=true
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -w|--watch)
            WATCH=true
            shift
            ;;
        --clean)
            clean_artifacts
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Default to all tests if no service specified
if [ -z "$SERVICE" ]; then
    SERVICE="all"
fi

# Print header
echo ""
echo "╔════════════════════════════════════════════╗"
echo "║     LoyaltyForge Test Runner               ║"
echo "╚════════════════════════════════════════════╝"
echo ""

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK is not installed"
    exit 1
fi

print_info "Using .NET version: $(dotnet --version)"
echo ""

# Run tests based on service selection
failed=0

case $SERVICE in
    all)
        print_info "Running all unit tests..."
        echo ""
        
        run_tests "tests/Unit/PointsEngine.Tests/PointsEngine.Api.Tests/PointsEngine.Api.Tests.csproj" "PointsEngine.Api" || failed=$((failed + 1))
        echo ""
        
        run_tests "tests/Unit/PointsEngine.Tests/PointsEngine.Application.Tests/PointsEngine.Application.Tests.csproj" "PointsEngine.Application" || failed=$((failed + 1))
        echo ""
        
        run_tests "tests/Unit/Rewards.Tests/Rewards.Api.Tests/Rewards.Api.Tests.csproj" "Rewards.Api" || failed=$((failed + 1))
        echo ""
        
        run_tests "tests/Unit/AuthTenant.Tests/AuthTenant.Api.Tests/AuthTenant.Api.Tests.csproj" "AuthTenant.Api" || failed=$((failed + 1))
        echo ""
        
        run_tests "tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Api.Tests/EcommerceIntegration.Api.Tests.csproj" "EcommerceIntegration.Api" || failed=$((failed + 1))
        echo ""
        ;;
    rewards)
        run_tests "tests/Unit/Rewards.Tests/Rewards.Api.Tests/Rewards.Api.Tests.csproj" "Rewards.Api" || failed=$((failed + 1))
        ;;
    points)
        run_tests "tests/Unit/PointsEngine.Tests/PointsEngine.Api.Tests/PointsEngine.Api.Tests.csproj" "PointsEngine.Api" || failed=$((failed + 1))
        echo ""
        run_tests "tests/Unit/PointsEngine.Tests/PointsEngine.Application.Tests/PointsEngine.Application.Tests.csproj" "PointsEngine.Application" || failed=$((failed + 1))
        ;;
    authtenant)
        run_tests "tests/Unit/AuthTenant.Tests/AuthTenant.Api.Tests/AuthTenant.Api.Tests.csproj" "AuthTenant.Api" || failed=$((failed + 1))
        ;;
    ecommerce)
        run_tests "tests/Unit/EcommerceIntegration.Tests/EcommerceIntegration.Api.Tests/EcommerceIntegration.Api.Tests.csproj" "EcommerceIntegration.Api" || failed=$((failed + 1))
        ;;
esac

# Print coverage report location if coverage was enabled
if [ "$COVERAGE" = true ] && [ $failed -eq 0 ]; then
    echo ""
    print_info "Coverage reports generated in: ./TestResults/"
    print_info "To view coverage, install reportgenerator:"
    echo "  dotnet tool install -g dotnet-reportgenerator-globaltool"
    echo "  reportgenerator -reports:\"TestResults/**/coverage.cobertura.xml\" -targetdir:\"TestResults/CoverageReport\" -reporttypes:Html"
    echo "  open TestResults/CoverageReport/index.html"
fi

# Print summary
echo ""
echo "╔════════════════════════════════════════════╗"
echo "║            Test Summary                    ║"
echo "╚════════════════════════════════════════════╝"

if [ $failed -eq 0 ]; then
    print_success "All tests passed! ✨"
    exit 0
else
    print_error "${failed} test suite(s) failed"
    exit 1
fi
