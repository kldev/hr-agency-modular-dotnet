#!/bin/bash

# Start Script
# ==============================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$SCRIPT_DIR"

# Prefer the Compose v2 plugin, fall back to the standalone binary.
if docker compose version >/dev/null 2>&1; then
    COMPOSE=(docker compose)
else
    COMPOSE=(docker-compose)
fi

# Values printed in the summary (RabbitMqUser, Grafana credentials, ...).
if [ -f .env ]; then
    set -a
    # shellcheck disable=SC1091
    source .env
    set +a
fi

usage() {
    echo "Usage: ./start.sh [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  --build, --b          Force rebuild of all images"
    echo "  --observability, -o   Also start Grafana, Prometheus, the OpenTelemetry Collector and"
    echo "                        Rootprint (logs + traces), and make every service export to them"
    echo "  --foreground, -f      Run in foreground (see logs)"
    echo "  --logs, --l [svc]     Follow logs (default: webapi), e.g. --logs emails-worker"
    echo "  --logs-worker         Follow feeds worker logs"
    echo "  --status, --ps        Show containers"
    echo "  --traffic             k6: recruiters working in the panel (k6/panel.js)"
    echo "  --emails              k6: password resets -> RabbitMQ -> emails worker (k6/emails.js)"
    echo "  --files               k6: avatar uploads through the file service (k6/files.js)"
    echo "  --job-board           k6: candidates browsing the public job board (k6/job-board.js)"
    echo "  --sa                  Stop only webapi (e.g. before 'dotnet run')"
    echo "  --stop                Stop and remove containers (data is kept)"
    echo "  --clean, --c          Remove containers AND volumes (all data)"
    echo "  --help, -h            Show this help"
    echo ""
    echo "k6 options are passed through the environment, e.g.:"
    echo "  VUS=50 DURATION=5m ./start.sh --traffic"
}

# k6 reads -e values; forward the ones the scripts understand when they are set.
run_k6() {
    local script="$1"
    local args=()
    for name in BASE_URL WEB_URL ORG_COUNT RUN_ID VUS DURATION RATE REJECT_RATE SLUGS OWNER_EMAIL OWNER_PASSWORD; do
        if [ -n "${!name}" ]; then
            args+=(-e "$name=${!name}")
        fi
    done
    k6 run "${args[@]}" "$REPO_DIR/k6/$script"
}

# Parse arguments
BUILD_ARG=""
DETACH_ARG="-d"
OBSERVABILITY=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --b|--build)
            BUILD_ARG="--build"
            shift
            ;;
        --observability|-o)
            OBSERVABILITY=true
            shift
            ;;
        --foreground|-f)
            DETACH_ARG=""
            shift
            ;;
        --l|--logs)
            SERVICE="webapi"
            if [[ -n "$2" && "$2" != -* ]]; then
                SERVICE="$2"
            fi
            COMPOSE_PROFILES=observability "${COMPOSE[@]}" logs -f "$SERVICE"
            exit 0
            ;;
        --logs-worker)
            "${COMPOSE[@]}" logs -f feeds-worker
            exit 0
            ;;
        --ps|--status)
            COMPOSE_PROFILES=observability "${COMPOSE[@]}" ps -a
            exit 0
            ;;
        --traffic)
            run_k6 panel.js
            exit 0
            ;;
        --emails)
            run_k6 emails.js
            exit 0
            ;;
        --files)
            run_k6 files.js
            exit 0
            ;;
        --job-board)
            run_k6 job-board.js
            exit 0
            ;;
        --sa)
            "${COMPOSE[@]}" stop webapi
            exit 0
            ;;
        --stop)
            # With the profile on, so that the observability containers go down too.
            COMPOSE_PROFILES=observability "${COMPOSE[@]}" down
            exit 0
            ;;
        --c|--clean)
            echo "Cleaning up volumes and containers..."
            COMPOSE_PROFILES=observability "${COMPOSE[@]}" down -v --remove-orphans
            echo "Clean complete."
            exit 0
            ;;
        --help|-h)
            usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            echo ""
            usage
            exit 1
            ;;
    esac
done

echo "=========================================="
echo "  HR Agency API "
echo "=========================================="
echo ""

# The collector is the one address every service exports to. Without the profile the variable stays
# empty, and an empty endpoint means no exporter - the services run the same either way.
if [ "$OBSERVABILITY" = true ]; then
    export COMPOSE_PROFILES=observability
    export OtelEndpoint="http://otel-collector:4317"
    echo "Observability on: Grafana, Prometheus, OpenTelemetry Collector, Rootprint"
    echo ""
fi

echo "Starting services..."
echo ""

"${COMPOSE[@]}" up $BUILD_ARG $DETACH_ARG

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
    echo "  Job board:  http://localhost:5050/hr-agency/jobs.html"
    echo "  RabbitMQ:   amqp://localhost:5672"
    echo "  Management: http://localhost:15672  (user: $RabbitMqUser)"
    echo ""
    echo "  MailPit:    http://localhost:8025"
    if [ "$OBSERVABILITY" = true ]; then
        echo ""
        echo "  Grafana:    http://localhost:3000   (user: ${GrafanaUser:-admin}, folder HR Agency)"
        echo "  Prometheus: http://localhost:9090   (targets: /targets)"
        echo "  Rootprint:  http://localhost:8282   (logs + traces, user: ${RootprintAdminEmail:-admin@hr-agency.com})"
        echo "  OTLP:       localhost:4317 (gRPC), localhost:4318 (HTTP) - for 'dotnet run' hosts"
    fi
    echo ""
    echo "  Health checks:"
    echo "    - API:    http://localhost:5000/healthz"
    echo "    - Ready:  http://localhost:5000/health/ready"
    echo ""
    echo "Commands:"
    echo "  View logs:      ./start.sh --logs [service]"
    echo "  Traffic (k6):   ./start.sh --traffic | --emails | --files | --job-board"
    echo "  Stop services:  ./start.sh --stop"
    echo "  Clean all:      ./start.sh --clean"
    echo ""
fi
