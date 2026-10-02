# 030 · Observability

**Status:** done · **Built:** 2026-09-25 → 2026-10-02 · **Verify:** `./verify observability` · **Plan:** backend 025 (local, not in git)

## Why
One click in the panel can travel API → outbox → RabbitMQ → worker → SMTP, or API → file service →
S3, and none of it was visible: logs were plain console text, the RabbitMQ URI was printed with its
password, candidate e-mails were logged, there were no traces or metrics and the workers had no health
signal. An operator needs to go from a dashboard to a trace to the logs and the tenant it happened to.

## What it promises
- Every host (API, job board, file service, reports service, feeds worker, notification worker) logs
  through Serilog with levels from the `Serilog` section; `Observability:ConsoleFormat=json` in
  containers.
- Logs, traces and metrics leave through one OTLP exporter with one resource - and only when
  `OTEL_EXPORTER_OTLP_ENDPOINT` is set. Tests and a bare run never dial out.
- Every log line and span of a request carries `OrganizationId`, `UserId` and `ImpersonatedBy`.
- ProblemDetails carry `traceId`, the bare W3C id searchable in the trace UI.
- HTTP hosts expose `/health/live` (anonymous, asks no dependency) and `/health/ready` (names,
  statuses and timings only, never the exception). The file and reports services are `Degraded` for
  the API, not `Unhealthy`; workers publish health as a metric. `/healthz` keeps its old shape.
- Domain metrics come from the event store (`marten.event.append{event.type}`); hand-written meters
  exist only where Marten sees nothing: mails (sent/duplicate/failed, duration), uploads (stored,
  rejected with a closed set of reason codes, size), feed generations (count, duration, size, a span
  per generation). Runtime metrics from `System.Runtime`, exemplars on histograms, a memory limit
  gauge.
- `./infrastructure/start.sh --build --observability` brings up an OpenTelemetry Collector,
  Prometheus, Grafana with five provisioned dashboards (overview, emails worker, file service,
  RabbitMQ, PostgreSQL) and Rootprint over Quickwit storing logs and traces in RustFS.
- k6 scripts generate traffic for the panel, mail, file uploads and the public job board.
- No personal data or secrets in logs; identifiers instead.

## Surface
- Backend: `src/Observability/HrAgencySystem.Observability` (Serilog, OTel, health as metric, no
  ASP.NET Core), `src/Observability/HrAgencySystem.Observability.AspNetCore` (request span, one-line
  request log, health endpoints), `TenantTelemetryMiddleware`, `NotificationMetrics`, `FileMetrics`,
  `FeedTelemetry`, `ProcessLimitsMetrics`.
- Infra: `infrastructure/observability/` (collector, Prometheus, Grafana dashboards, Rootprint,
  RustFS, Postgres exporter role), compose profile `observability`, anchor `x-otel-env`.
- Traffic: `k6/` (`panel.js`, `emails.js`, `files.js`, `job-board.js`, `k6/README.md`).
- Tests: `tests/HrAgencySystem.IntegrationTests.Platform/Observability`,
  `UnitTests/Notifications/ProcessedEventGuardTests.cs`, `FileService.UnitTests/FileStoreTests.cs`.

## Out of scope for this feature
- Browser telemetry (OTel web SDK, trace id in the error toast) - not yet (plan step 7, deferred).
- `OrganizationId` as a metric tag - not ever (unbounded Prometheus series).
- Per-host `/metrics` scraping - not ever here (workers have no HTTP port; `/metrics` would need a
  hole in the fallback-deny policy).
- Alerting - not yet; nothing in the repo.

## Acceptance criteria
- [x] Liveness answers without a token and without asking dependencies — `./verify observability` (`Liveness_answers_without_a_token_and_without_asking_dependencies`)
- [x] ProblemDetails carry the W3C trace id — `./verify observability` (`Problem_details_carry_the_w3c_trace_id`)
- [ ] Without an endpoint no exporter is registered; with one it is — unproven (no test found)
- [x] Mail metric counts sent / duplicate / failed around the idempotency guard — `./verify emails` (`ProcessedEventGuardTests`, `MetricCollector`)
- [x] Upload metric counts stored and rejected uploads — `./verify files` (`FileStoreTests`)
- [ ] Tenant ids appear on every log line and span of a request — unproven (no test found)
- [ ] `/health/ready` hides exception text — unproven (no test found)
- [ ] A trace survives the trip through RabbitMQ to the worker — unproven (no test; plan 025 lists it as a manual check)
- [ ] Dashboards query metric names that exist — unproven (no automated check; CLAUDE.md asks to check every `expr` by hand)

## Decisions
- Serilog owns console and levels, OTel is the transport, a collector instead of per-host scraping: [ADR-0020](../../../docs/adr/0020-observability-via-collector.md).
- Hosts stay thin and modules carry no telemetry code: [ADR-0001](../../../docs/adr/0001-modular-monolith.md).
- Two projects, not one: the workers run on the `runtime` image without ASP.NET Core, so a shared `FrameworkReference` would break them in a container.
- Logs go Serilog → `writeToProviders: true` → OTel logger provider, not `Serilog.Sinks.OpenTelemetry`: one exporter and one resource for all three signals.
- Wolverine's meter is `Wolverine:<service>`, subscribed with `Wolverine*` (16f4f0e0).

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-25 | dd799d04 | No console writes, secrets and personal data out of logs, W3C trace id in ProblemDetails |
| 2026-09-25 | aef213dc | Serilog + OpenTelemetry in every host, tenant on logs and spans, health endpoints |
| 2026-09-25 | 4064aedd | Mail, upload and feed metrics, a span per feed generation, feed failures logged |
| 2026-09-25 | 01367fd0 | Runtime metrics, trace exemplars, memory limit gauge |
| 2026-09-25 | b01e1545 | A file the file service refuses is a 400 with its reason, not a 503 |
| 2026-09-25 | 16f4f0e0 | Subscribe to Wolverine's per-service meter |
| 2026-09-25 | 413b33ad | Compose profile: collector, Prometheus, Grafana dashboards, Rootprint on RustFS |
| 2026-09-25 | e44c628d | k6 traffic scripts |

## Notes from reconstruction
- Plan 025 chose Aspire Dashboard plus a Grafana LGTM image, and its post-implementation notes
  (dated 2026-09-22, on a side branch) describe that. What reached `main` on 2026-09-25 is a different
  stack: collector + Prometheus + Grafana + Rootprint/Quickwit on RustFS. The plan's notes are stale.
- The plan's hand-written domain counters (with `channel`/`source` tags) were replaced by Marten's
  `TrackEventCounters()`.
- `JobFeedProcessor` swallowed exceptions without a log; fixed on the way (4064aedd).
- The observability code is kept in sync by hand with a shared library outside this repository.
