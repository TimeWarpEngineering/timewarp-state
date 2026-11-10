# Enable dev CLI telemetry with UseTelemetry

## Description

Every `dev` command in this repo should emit OpenTelemetry spans and metrics so we can see
which dev commands are slow across the fleet. Telemetry in TimeWarp.Nuru is a compile-time
opt-in: the `TimeWarp.Nuru.DevCli` package ships no telemetry code, so each repo's
`tools/dev-cli/dev.cs` must call `UseTelemetry` on the builder. Without it the source
generator emits nothing.

Reference implementation: timewarp-nuru `tools/dev-cli/dev.cs` (kanban 556) and
`tools/dev-cli/telemetry.md`. Nuru docs: `documentation/user/features/telemetry.md`.

Export is gated at runtime on `OTEL_EXPORTER_OTLP_ENDPOINT` (set in the developer shell to
`https://otlp.timewarp.work` with `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf`). No endpoint
means no export, so the change is safe in CI even without a collector.

## Requirements

- `tools/dev-cli/dev.cs` calls `.UseTelemetry(options => options.ServiceName = "timewarp-state-dev")`
  on the `NuruApp.CreateBuilder()` chain. The service name must be a string literal.
- Every route, including the DevCli-provided ones (clean, check-version, release, self-install),
  gets the automatic `TimeWarp.Nuru.Behavior` command span and duration metric.
- Copy `IncomingTraceContext` from timewarp-nuru `tools/dev-cli/endpoints/incoming-trace-context.cs`
  and wrap `app.RunAsync(args)` in `IncomingTraceContext.Restore()` so command spans nest under
  the caller's `TRACEPARENT` (ganda, CI).
- If the repo has its own `ActivitySource`, register it with `options.ActivitySources.Add("<literal>")`.
- `dev --capabilities` and `dev build` still work; AOT publish of the dev CLI still succeeds.

## Goal (implementer restatement)

Make every `dev` command in timewarp-state emit a Nuru command span and duration metric when an
OTLP endpoint is configured, and nest under a caller's `TRACEPARENT`.

Acceptance:
- `tools/dev-cli/dev.cs` calls `.UseTelemetry(options => options.ServiceName = "timewarp-state-dev")`
  (string literal) on the builder chain.
- `IncomingTraceContext` (copied from timewarp-nuru) restores `TRACEPARENT`/`TRACESTATE` onto
  `Activity.Current` around `app.RunAsync(args)`.
- Dev CLI has no ActivitySource of its own, so no `ActivitySources.Add` is needed (the product
  sources under `source/` are library code, not dev CLI code).
- `dev --capabilities` and `dev build` still work; the AOT dev CLI publish still succeeds.
- With `OTEL_EXPORTER_OTLP_ENDPOINT` set, a run exports a span; unset exports nothing.
- `tools/dev-cli/telemetry.md` documents the env vars.

## Checklist

- [x] Add `UseTelemetry` call to `tools/dev-cli/dev.cs`
- [x] Add `IncomingTraceContext` and wrap `RunAsync`
- [x] Register repo-specific ActivitySources if any
- [x] Build dev CLI (`bin/dev build`) and run one command with `OTEL_EXPORTER_OTLP_ENDPOINT` set; confirm a span arrives
- [x] Add a short `tools/dev-cli/telemetry.md` naming the env vars (copy from timewarp-nuru)

## Session

- Created: b3be0249-1852-425a-9e90-5a7e56049503 (2026-11-08)
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session e8c39c9a-e7d8-477a-bb07-4d8c3d4c3fd0, max-turns 500, Done — 2026-11-10T12:14:48Z
- Review oracle: Cursor Grok 4.7 (general, effort 2), conversation da2ec7f5-fe42-41c3-905d-79f17347de52, Grok session ffba6066-c22c-42a7-95c8-de75506a6687, ganda session 842767, turn cap 120 (cursor uncapped), 2026-11-10
- Review oracle: review by reviewer-cursor (cursor, vendor xai, model grok-4.7-high), session not reported, max-turns uncapped (no --max-turns flag; bound 120), artifacts general.md, merged.md, disposition.md — 2026-11-10T12:24:02Z

## Results

- `tools/dev-cli/dev.cs`: `.UseTelemetry(options => options.ServiceName = "timewarp-state-dev")`
  on the builder; `using IDisposable? incomingTrace = IncomingTraceContext.Restore();` before
  `app.RunAsync(args)`. Design region notes the telemetry behavior.
- `tools/dev-cli/endpoints/incoming-trace-context.cs`: copied verbatim from timewarp-nuru.
- `tools/dev-cli/global-usings.cs`: added `System.Diagnostics` and
  `System.Runtime.CompilerServices` (Activity, UnsafeAccessor).
- No `ActivitySources.Add`: the dev CLI has no ActivitySource of its own. The sources under
  `source/` (telemetry, devtools) are TimeWarp.State library code that `dev` never loads.
- `tools/dev-cli/telemetry.md`: env vars, span/metric names, TRACEPARENT behaviour. Trimmed from
  the nuru copy, which also lists workflow stage spans that this repo does not have.
- Gates: `./bin/dev self-install` (AOT publish) succeeded; `./bin/dev --capabilities` works;
  `./bin/dev build --quiet` succeeded; `ganda repo audit` passes (3 pre-existing advisory
  warnings: kanban-landed 098, kebab-path-names for wwwroot/.targets files, vscode-window-icon;
  none in scope of this change). Full test suite not run: no product or test code changed.
- Span proof: a local OTLP http/protobuf capture listener on 127.0.0.1:4318, running
  `dev build --quiet` with `OTEL_EXPORTER_OTLP_ENDPOINT` and a fixed `TRACEPARENT`, received
  `/v1/traces` and `/v1/metrics` posts carrying service name `timewarp-state-dev`, source
  `TimeWarp.Nuru.Behavior`, a `build` span, the caller's trace id and parent span id, and
  `nuru.behavior.commands.duration`.

### How to validate

Smoke:

```bash
./bin/dev self-install
./bin/dev --capabilities >/dev/null && echo capabilities-ok
OTEL_EXPORTER_OTLP_ENDPOINT=https://otlp.timewarp.work OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf \
  TRACEPARENT=00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01 ./bin/dev build --quiet
```

Expect: self-install reports success; `capabilities-ok` prints; the build succeeds and the
collector shows a `build` span for service `timewarp-state-dev` under trace
`0af7651916cd43dd8448eb211c80319c`, parented to span `b7ad6b7169203331`, plus a
`nuru.behavior.commands.duration` sample. With `OTEL_EXPORTER_OTLP_ENDPOINT` unset, nothing
is exported and output is unchanged.

### Review

Round 1, effort 2, roster general. Counts: bug 0 open / 0 fixed / 0 wontfix, suggestion 0, nit 0. Final open count 0. Disposition: **clean**. The implementer restatement matches the task; no misread. Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Review

- Round 1: Done

## Notes

Fleet rollout: Steve asked on 2026-11-07 for telemetry on all dev CLI commands in every live
reposet repo. The same task exists in each live repo that has a `tools/dev-cli`. Only
timewarp-nuru had it before this rollout.
