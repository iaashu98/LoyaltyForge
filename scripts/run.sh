#!/bin/bash

# LoyaltyForge Run Script
# Usage: ./run.sh [options]
#
# Options:
#   -a, --all           Run all services (default)
#   -i, --infra         Run infrastructure only (postgres, rabbitmq)
#   -g, --gateway       Run API Gateway
#   -t, --auth          Run Auth+Tenant service
#   -e, --ecommerce     Run E-commerce Integration service
#   -p, --points        Run Points Engine service
#   -r, --rewards       Run Rewards service
#   -d, --detached      Run in detached mode
#   -b, --build         Build before running
#   -s, --stop          Stop services
#   -l, --logs          Show logs (use with service flags)
#   -h, --help          Show this help message

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Flags
RUN_ALL=false
RUN_INFRA=false
RUN_GATEWAY=false
RUN_AUTH=false
RUN_ECOMMERCE=false
RUN_POINTS=false
RUN_REWARDS=false
DETACHED=false
BUILD_FIRST=false
STOP_SERVICES=false
SHOW_LOGS=false
NO_FLAGS=true

# Service names (compatible with bash 3.x on macOS)
SVC_INFRA="postgres rabbitmq"
SVC_GATEWAY="api-gateway"
SVC_AUTH="auth-tenant"
SVC_ECOMMERCE="ecommerce-integration"
SVC_POINTS="points-engine"
SVC_REWARDS="rewards"

# Function to print colored output
print_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Function to show help
show_help() {
    echo "LoyaltyForge Run Script"
    echo ""
    echo "Usage: ./run.sh [options]"
    echo ""
    echo "Options:"
    echo "  -a, --all           Run all services (default)"
    echo "  -i, --infra         Run infrastructure only (postgres, rabbitmq)"
    echo "  -g, --gateway       Run API Gateway"
    echo "  -t, --auth          Run Auth+Tenant service"
    echo "  -e, --ecommerce     Run E-commerce Integration service"
    echo "  -p, --points        Run Points Engine service"
    echo "  -r, --rewards       Run Rewards service"
    echo "  -d, --detached      Run in detached mode"
    echo "  -b, --build         Build before running"
    echo "  -s, --stop          Stop services"
    echo "  -l, --logs          Show logs (use with service flags)"
    echo "  -h, --help          Show this help message"
    echo ""
    echo "Examples:"
    echo "  ./run.sh                      # Run all services"
    echo "  ./run.sh -d                   # Run all in detached mode"
    echo "  ./run.sh -i                   # Run infrastructure only"
    echo "  ./run.sh -i -t                # Run infra + Auth+Tenant"
    echo "  ./run.sh -b -t -p             # Build and run Auth + Points"
    echo "  ./run.sh -s                   # Stop all services"
    echo "  ./run.sh -s -t                # Stop Auth+Tenant only"
    echo "  ./run.sh -l -t                # Show Auth+Tenant logs"
    echo ""
    echo "Service Ports:"
    echo "  API Gateway:          http://localhost:5000"
    echo "  Auth+Tenant:          http://localhost:5001"
    echo "  E-commerce:           http://localhost:5002"
    echo "  Points Engine:        http://localhost:5003"
    echo "  Rewards:              http://localhost:5004"
    echo "  PostgreSQL:           localhost:5432"
    echo "  RabbitMQ:             localhost:5672 (AMQP), localhost:15672 (UI)"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -a|--all)
            RUN_ALL=true
            NO_FLAGS=false
            shift
            ;;
        -i|--infra)
            RUN_INFRA=true
            NO_FLAGS=false
            shift
            ;;
        -g|--gateway)
            RUN_GATEWAY=true
            NO_FLAGS=false
            shift
            ;;
        -t|--auth)
            RUN_AUTH=true
            NO_FLAGS=false
            shift
            ;;
        -e|--ecommerce)
            RUN_ECOMMERCE=true
            NO_FLAGS=false
            shift
            ;;
        -p|--points)
            RUN_POINTS=true
            NO_FLAGS=false
            shift
            ;;
        -r|--rewards)
            RUN_REWARDS=true
            NO_FLAGS=false
            shift
            ;;
        -d|--detached)
            DETACHED=true
            shift
            ;;
        -b|--build)
            BUILD_FIRST=true
            shift
            ;;
        -s|--stop)
            STOP_SERVICES=true
            shift
            ;;
        -l|--logs)
            SHOW_LOGS=true
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

# If no service flags provided, run all
if [ "$NO_FLAGS" = true ]; then
    RUN_ALL=true
fi

# Build the list of services to run
SERVICES=""

if [ "$RUN_ALL" = true ]; then
    SERVICES="" # Empty means all services in docker-compose
else
    [ "$RUN_INFRA" = true ] && SERVICES="$SERVICES $SVC_INFRA"
    [ "$RUN_GATEWAY" = true ] && SERVICES="$SERVICES $SVC_GATEWAY"
    [ "$RUN_AUTH" = true ] && SERVICES="$SERVICES $SVC_AUTH"
    [ "$RUN_ECOMMERCE" = true ] && SERVICES="$SERVICES $SVC_ECOMMERCE"
    [ "$RUN_POINTS" = true ] && SERVICES="$SERVICES $SVC_POINTS"
    [ "$RUN_REWARDS" = true ] && SERVICES="$SERVICES $SVC_REWARDS"
fi

# Change to project root
cd "$PROJECT_ROOT"

# Handle logs
if [ "$SHOW_LOGS" = true ]; then
    if [ -z "$SERVICES" ]; then
        print_info "Showing logs for all services..."
        docker compose logs -f
    else
        print_info "Showing logs for:$SERVICES"
        docker compose logs -f $SERVICES
    fi
    exit 0
fi

# Handle stop
if [ "$STOP_SERVICES" = true ]; then
    if [ -z "$SERVICES" ]; then
        print_info "Stopping all services..."
        docker compose down
    else
        print_info "Stopping:$SERVICES"
        docker compose stop $SERVICES
    fi
    print_success "Services stopped"
    exit 0
fi

# Print header
echo ""
echo "======================================"
echo "  LoyaltyForge Run Script"
echo "======================================"
echo ""

# Build command
COMPOSE_CMD="docker compose"

if [ "$BUILD_FIRST" = true ]; then
    COMPOSE_CMD="$COMPOSE_CMD up --build"
    print_info "Build enabled"
else
    COMPOSE_CMD="$COMPOSE_CMD up"
fi

if [ "$DETACHED" = true ]; then
    COMPOSE_CMD="$COMPOSE_CMD -d"
    print_info "Detached mode"
fi

# Add services if specific ones selected
if [ -n "$SERVICES" ]; then
    COMPOSE_CMD="$COMPOSE_CMD $SERVICES"
    print_info "Services:$SERVICES"
else
    print_info "Running all services"
fi

echo ""
print_info "Executing: $COMPOSE_CMD"
echo ""

# Execute
eval $COMPOSE_CMD

if [ "$DETACHED" = true ]; then
    echo ""
    print_success "Services started in background"
    echo ""
    echo "Useful commands:"
    echo "  ./scripts/run.sh -l            # View all logs"
    echo "  ./scripts/run.sh -l -t         # View Auth+Tenant logs"
    echo "  ./scripts/run.sh -s            # Stop all services"
    echo "  docker compose ps              # View running containers"
fi
