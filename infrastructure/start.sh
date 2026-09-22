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
OBSERVABILITY=""

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
        --observability|-o)
            OBSERVABILITY="$2"
            if [[ "$OBSERVABILITY" != "aspire" && "$OBSERVABILITY" != "grafana" ]]; then
                echo "--observability takes aspire or grafana"
                exit 1
            fi
            shift 2
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
        --logs-worker)
            docker-compose logs -f feeds-worker
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
            echo "  --observability aspire|grafana"
            echo "                Start a telemetry backend and make every service export to it"
            echo "  --logs        Follow webapi logs"
            echo "  --logs-worker Follow feeds worker logs"
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

# One profile at a time: both backends listen on 4317/4318 and answer to the alias "otel".
if [ -n "$OBSERVABILITY" ]; then
    export COMPOSE_PROFILES="$OBSERVABILITY"
    export OtelEndpoint="http://otel:4317"
fi

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
    echo "              http://localhost:5000/api/development/seed/random?count=50"
    echo "  RabbitMQ:   amqp://localhost:5672"
    echo "  Management: http://localhost:15672  (user: $RabbitMqUser)"
    echo ""
    echo ""
    echo "  MailPit:    http://localhost:8025"
    if [ "$OBSERVABILITY" = "aspire" ]; then
        echo "  Telemetry:  http://localhost:18888  (Aspire dashboard)"
    elif [ "$OBSERVABILITY" = "grafana" ]; then
        echo "  Telemetry:  http://localhost:3000   (Grafana, admin/admin - folder HR Agency)"
    fi
    echo ""
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