#!/bin/bash

# LoyaltyForge Build Script
# Usage: ./build.sh [options]
#
# Options:
#   -a, --all           Build all services (default if no flags provided)
#   -g, --gateway       Build API Gateway
#   -t, --auth          Build Auth+Tenant service
#   -e, --ecommerce     Build E-commerce Integration service
#   -p, --points        Build Points Engine service
#   -r, --rewards       Build Rewards service
#   -s, --shared        Build shared libraries only
#   -c, --clean         Clean before building
#   -d, --docker        Build Docker images instead of dotnet build
#   -h, --help          Show this help message

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Project root (script location's parent)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Service paths
SERVICES_DIR="$PROJECT_ROOT/src/Services"
SHARED_DIR="$PROJECT_ROOT/src/Shared"

# Flags
BUILD_ALL=false
BUILD_GATEWAY=false
BUILD_AUTH=false
BUILD_ECOMMERCE=false
BUILD_POINTS=false
BUILD_REWARDS=false
BUILD_SHARED=false
CLEAN_BUILD=false
DOCKER_BUILD=false
NO_FLAGS=true

# Function to print colored output
print_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Function to show help
show_help() {
    echo "LoyaltyForge Build Script"
    echo ""
    echo "Usage: ./build.sh [options]"
    echo ""
    echo "Options:"
    echo "  -a, --all           Build all services (default if no flags provided)"
    echo "  -g, --gateway       Build API Gateway"
    echo "  -t, --auth          Build Auth+Tenant service"
    echo "  -e, --ecommerce     Build E-commerce Integration service"
    echo "  -p, --points        Build Points Engine service"
    echo "  -r, --rewards       Build Rewards service"
    echo "  -s, --shared        Build shared libraries only"
    echo "  -c, --clean         Clean before building"
    echo "  -d, --docker        Build Docker images instead of dotnet build"
    echo "  -h, --help          Show this help message"
    echo ""
    echo "Examples:"
    echo "  ./build.sh                  # Build all services"
    echo "  ./build.sh -t -p            # Build Auth+Tenant and Points Engine"
    echo "  ./build.sh -c -a            # Clean and build all"
    echo "  ./build.sh -d -t            # Build Auth+Tenant Docker image"
    echo "  ./build.sh --clean --auth   # Clean and build Auth+Tenant"
}

# Function to build a .NET project
build_dotnet() {
    local name=$1
    local path=$2
    
    print_info "Building $name..."
    
    if [ "$CLEAN_BUILD" = true ]; then
        dotnet clean "$path" --verbosity quiet 2>/dev/null || true
    fi
    
    if dotnet build "$path" --verbosity minimal; then
        print_success "$name built successfully"
    else
        print_error "Failed to build $name"
        exit 1
    fi
}

# Function to build a Docker image
build_docker() {
    local name=$1
    local service_name=$2
    
    print_info "Building Docker image for $name..."
    
    if docker compose -f "$PROJECT_ROOT/docker-compose.yml" build "$service_name"; then
        print_success "$name Docker image built successfully"
    else
        print_error "Failed to build $name Docker image"
        exit 1
    fi
}

# Function to build shared libraries
build_shared() {
    print_info "Building shared libraries..."
    build_dotnet "LoyaltyForge.Common" "$SHARED_DIR/LoyaltyForge.Common/LoyaltyForge.Common.csproj"
    build_dotnet "LoyaltyForge.Contracts" "$SHARED_DIR/LoyaltyForge.Contracts/LoyaltyForge.Contracts.csproj"
    build_dotnet "LoyaltyForge.Messaging" "$SHARED_DIR/LoyaltyForge.Messaging/LoyaltyForge.Messaging.csproj"
}

# Function to build API Gateway
build_gateway() {
    if [ "$DOCKER_BUILD" = true ]; then
        build_docker "API Gateway" "api-gateway"
    else
        build_dotnet "API Gateway" "$SERVICES_DIR/ApiGateway/ApiGateway.Api/ApiGateway.Api.csproj"
    fi
}

# Function to build Auth+Tenant
build_auth() {
    if [ "$DOCKER_BUILD" = true ]; then
        build_docker "Auth+Tenant" "auth-tenant"
    else
        build_dotnet "Auth+Tenant" "$SERVICES_DIR/AuthTenant/AuthTenant.Api/AuthTenant.Api.csproj"
    fi
}

# Function to build E-commerce Integration
build_ecommerce() {
    if [ "$DOCKER_BUILD" = true ]; then
        build_docker "E-commerce Integration" "ecommerce-integration"
    else
        build_dotnet "E-commerce Integration" "$SERVICES_DIR/EcommerceIntegration/EcommerceIntegration.Api/EcommerceIntegration.Api.csproj"
    fi
}

# Function to build Points Engine
build_points() {
    if [ "$DOCKER_BUILD" = true ]; then
        build_docker "Points Engine" "points-engine"
    else
        build_dotnet "Points Engine" "$SERVICES_DIR/PointsEngine/PointsEngine.Api/PointsEngine.Api.csproj"
    fi
}

# Function to build Rewards
build_rewards() {
    if [ "$DOCKER_BUILD" = true ]; then
        build_docker "Rewards" "rewards"
    else
        build_dotnet "Rewards" "$SERVICES_DIR/Rewards/Rewards.Api/Rewards.Api.csproj"
    fi
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -a|--all)
            BUILD_ALL=true
            NO_FLAGS=false
            shift
            ;;
        -g|--gateway)
            BUILD_GATEWAY=true
            NO_FLAGS=false
            shift
            ;;
        -t|--auth)
            BUILD_AUTH=true
            NO_FLAGS=false
            shift
            ;;
        -e|--ecommerce)
            BUILD_ECOMMERCE=true
            NO_FLAGS=false
            shift
            ;;
        -p|--points)
            BUILD_POINTS=true
            NO_FLAGS=false
            shift
            ;;
        -r|--rewards)
            BUILD_REWARDS=true
            NO_FLAGS=false
            shift
            ;;
        -s|--shared)
            BUILD_SHARED=true
            NO_FLAGS=false
            shift
            ;;
        -c|--clean)
            CLEAN_BUILD=true
            shift
            ;;
        -d|--docker)
            DOCKER_BUILD=true
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# If no service flags provided, build all
if [ "$NO_FLAGS" = true ]; then
    BUILD_ALL=true
fi

# Print build configuration
echo ""
echo "======================================"
echo "  LoyaltyForge Build Script"
echo "======================================"
echo ""

if [ "$CLEAN_BUILD" = true ]; then
    print_info "Clean build enabled"
fi

if [ "$DOCKER_BUILD" = true ]; then
    print_info "Docker build mode"
else
    print_info ".NET build mode"
fi

echo ""

# Execute builds
START_TIME=$(date +%s)

# Always build shared libraries first (unless Docker mode or only specific services)
if [ "$DOCKER_BUILD" = false ]; then
    if [ "$BUILD_ALL" = true ] || [ "$BUILD_SHARED" = true ]; then
        build_shared
        echo ""
    fi
fi

if [ "$BUILD_ALL" = true ]; then
    build_gateway
    build_auth
    build_ecommerce
    build_points
    build_rewards
else
    [ "$BUILD_GATEWAY" = true ] && build_gateway
    [ "$BUILD_AUTH" = true ] && build_auth
    [ "$BUILD_ECOMMERCE" = true ] && build_ecommerce
    [ "$BUILD_POINTS" = true ] && build_points
    [ "$BUILD_REWARDS" = true ] && build_rewards
fi

END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

echo ""
echo "======================================"
print_success "Build completed in ${DURATION}s"
echo "======================================"
