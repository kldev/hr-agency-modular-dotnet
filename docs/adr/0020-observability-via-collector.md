# ADR-0020: Serilog owns the console, OpenTelemetry exports only when an endpoint is set, and every host pushes to one collector

- **Status:** Accepted
- **Date:** 2026-09-25
- **Evidence:** `aef213dc` feat(observability): Serilog + OpenTelemetry in every host, tenant on logs and spans, health endpoints,
  `4064aedd` feat(observability): mail, upload and feed metrics, a span per feed generation, log feed failures,
  `16f4f0e0` fix(observability): subscribe to Wolverine's per-service meter,
  `413b33ad` feat(infra): observability profile - OpenTelemetry Collector, Prometheus, Grafana dashboards and Rootprint on RustFS
- **Specs:** [030-observability](../../.shipit/specs/done/030-observability.md)

## Context
Six hosts (API, file service, reports service, job board, feeds worker, notification worker) had
console logs and nothing that kept logs, traces and metrics together. Two hosts are workers without
HTTP; `/metrics` on the API would sit behind the fallback-deny policy. Tests and a bare `dotnet run`
must not dial a collector. Workers run on the plain `runtime` image without ASP.NET Core.

## Decision
- Two projects in `src/Observability/`: `HrAgencySystem.Observability` (Serilog, OpenTelemetry,
  health as a metric, no ASP.NET Core) and `HrAgencySystem.Observability.AspNetCore` (request span,
  one-line request log, `/health/live`, `/health/ready`).
- Serilog owns levels and the console (section `Serilog`); every event goes on with
  `writeToProviders: true` to the OpenTelemetry logger provider, so logs, traces and metrics leave
  through one OTLP exporter with one resource.
- No `OTEL_EXPORTER_OTLP_ENDPOINT`, no exporter.
- Every host **pushes** over OTLP to `otel-collector`; logs and traces go to Rootprint over Quickwit
  (bucket on RustFS), metrics are exposed by the collector and scraped by Prometheus with
  `honor_labels`; Grafana has provisioned, generated dashboards. Compose profile `observability`.
- Domain metrics come from Marten's `TrackEventCounters()`; hand-written meters only where Marten
  sees nothing (`NotificationMetrics`, `FileMetrics`, `FeedTelemetry`). `OrganizationId` goes on
  logs and spans, never on a metric tag. Log identifiers, not people.

## Alternatives considered
Plan 025 (local, not in git):
- Plain `builder.Logging.AddOpenTelemetry()` without Serilog - rejected (worse local console, no
  one-line request log).
- `Serilog.Sinks.OpenTelemetry` - planned, replaced by `writeToProviders` so one exporter and one
  resource serve all signals (`### uwagi do planu`).
- One `Observability` project - split in two because a `FrameworkReference` would break the workers.
- Viewing stack: the plan proposed mutually exclusive compose profiles with the Aspire dashboard or
  `grafana/otel-lgtm`; the committed stack is the collector + Prometheus + Grafana + Rootprint
  (`413b33ad`). The LGTM/Aspire variant was never committed to `main`.
- Hand-written per-module counters with `channel`/`source` tags - replaced by Marten event counters.

## Consequences
- Wolverine's meter is `Wolverine:<service>`; subscribing to `Wolverine` got nothing (`16f4f0e0`).
- Dashboard JSON is generated; a panel on a wrong metric name is silently empty, so every `expr` is
  checked against a running Prometheus.
- `/health/ready` is anonymous and prints names, statuses and timings only.
- Background work needs its own span, or each SQL/S3 call becomes a root trace.

## Revisit when
- The stack moves to a cluster with its own collector.
- Prometheus series grow from a tag with an open value set.
