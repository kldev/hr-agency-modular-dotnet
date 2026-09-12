#!/bin/bash

# Start Script
# ==============================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=========================================="
echo "  HR Agency API "
echo "=========================================="
echo ""

# Parse arguments
BUILD_ARG=""
DETACH_ARG="-d"

while [[ $# -gt 0 ]]; do
    case $1 in
        --b)
            BUILD_ARG="--build"
            shift
            ;;    
        --build)
            BUILD_ARG="--build"
            shift
            ;;
        --foreground|-f)
            DETACH_ARG=""
            shift
            ;;
        --l)
            docker-compose logs -f webapi           
            exit 0
            ;;
        --logs)
            docker-compose logs -f webapi           
            exit 0
            ;;
        --stop)
            docker-compose down
            exit 0
            ;;  
        --sa)
            docker-compose stop webapi
            exit 0
            ;;              
        --c)
            echo "Cleaning up volumes and containers..."
            docker-compose down -v --remove-orphans
            echo "Clean complete."
            exit 0
            ;;                      
        --clean)
            echo "Cleaning up volumes and containers..."
            docker-compose down -v --remove-orphans
            echo "Clean complete."
            exit 0
            ;;
        --help|-h)
            echo "Usage: ./start.sh [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --build       Force rebuild of all images"
            echo "  --foreground  Run in foreground (see logs)"
            echo "  --clean       Remove all containers and volumes"
            echo "  --help        Show this help"
            echo ""
            echo "After starting, access:"
            echo "  Web API:    http://localhost:8080"
            echo ""
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "Starting services..."
echo ""

docker-compose up $BUILD_ARG $DETACH_ARG

if [ -n "$DETACH_ARG" ]; then
    echo ""
    echo "=========================================="
    echo "  Services started successfully!"
    echo "=========================================="
    echo ""
    echo "Endpoints:"
    echo "  Frontend:   http://localhost:8080"
    echo "              http://localhost:8080/admin"
    echo ""
    echo "  Web API:    http://localhost:5000"
    echo "     Docs:    http://localhost:5000/docs"
    echo ""
    echo "     Seed:    http://localhost:5000/api/development/seed"
    echo "              http://localhost:5000/api/development/seed-sales?count=1000"
    echo "  Health checks:"
    echo "    - API:    http://localhost:5000/healthz"
    echo ""
    echo ""
    echo ""
    echo "Commands:"
    echo "  View logs:      docker-compose logs -f"
    echo "  Stop services:  docker-compose down"
    echo "  Clean all:      ./start.sh --clean"
    echo ""
fi