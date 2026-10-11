# DevTools Aspire resource has no endpoints: launch profile folder casing and explicit http endpoint

## Description

Running the telemetry sample AppHost on Linux (`dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj`,
Aspire 13.6.1) after task 103 merged (PR #635):

- `timewarp-state-devtools` starts, but with no endpoints. Its log says `Hosting environment: Production`
  and `Now listening on: http://localhost:5000` (Kestrel default, no `ASPNETCORE_URLS`), and `aspire describe`
  shows no URLs for it.
- `sample-04-server` is `FailedToStart`:
  `Failed to resolve environment variable 'TIMEWARP_STATE_DEVTOOLS_ENDPOINT' ... System.InvalidOperationException:
  The endpoint 'http' is not defined for the resource 'timewarp-state-devtools'. The resource has no endpoints defined.`

Root cause: Aspire reads launch settings only from `Properties/launchSettings.json` (capital P). Decompiled
Aspire.Hosting 13.6.1: `LaunchProfileExtensions.cs:76` and `LaunchSettingsReader.cs:18` both
`Path.Combine(dir, "Properties", "launchSettings.json")`, no lowercase fallback. The DevTools app keeps its file at
`source/timewarp-state-devtools/properties/launchSettings.json` (kebab-case rule), so on Linux Aspire finds no
profile, creates no `http` endpoint, and does not set `ASPNETCORE_URLS` / `ASPNETCORE_ENVIRONMENT`.
`TimeWarpStateDevToolsHostingExtensions.AddTimeWarpStateDevTools` relies on that profile for
`UiEndpointName = "http"` and only labels it with `WithUrlForEndpoint`.

`tests/timewarp-state-devtools-tests/hosting-registration-tests.cs` seeds an `http` `EndpointAnnotation` by hand
(line ~86), so launch-profile discovery was never exercised.

Same latent casing issue: `samples/04-telemetry/server/sample-04-server/properties`, and samples 00-03. The
test-app server and samples 05/06 already use `Properties/`. tw-csharp lists ASP.NET folder names as a
kebab-case exception, so `Properties/` is the sanctioned spelling.

Filed by Claude (cockpit) at Steven's request, 2026-10-12, after watching task 103 land.

## Requirements

- `AddTimeWarpStateDevTools` declares the UI endpoint itself (`WithHttpEndpoint(name: "http")`) so the resource
  has an `http` endpoint whether or not a launch profile is found. Aspire then injects `ASPNETCORE_URLS` for
  the project resource. Keep the dashboard label "TimeWarp.State DevTools". Do not double-declare when the
  profile is also found (guard on an existing `http` `EndpointAnnotation`, or declare with the same name and
  verify Aspire merges).
- Rename `source/timewarp-state-devtools/properties/` -> `Properties/` and
  `samples/04-telemetry/server/sample-04-server/properties/` -> `Properties/` (git mv; case-only rename, so do
  it in two steps or with `git mv -f` on a case-insensitive filesystem). Samples 00-03 may follow in the same PR
  if the diff stays small; otherwise note them.
- DevTools app should run as Development under Aspire when the profile is applied, and must work (static web
  assets, no dev exception page) when it is not. Check `program.cs` lines ~41-46.
- A hosting test that builds the DevTools resource through `AddTimeWarpStateDevTools` and asserts the `http`
  endpoint exists without seeding an `EndpointAnnotation`.
- Manual check on Linux: AppHost up, both resources Running, dashboard shows the "TimeWarp.State DevTools" link,
  `/counter` clicks appear in the inspector. Record the result in `## Results`.

## Checklist

- [x] `WithHttpEndpoint("http")` in `add-timewarp-state-dev-tools.cs`; Design region updated
- [x] `git mv properties Properties` for DevTools app and sample-04-server (and 00-03 if cheap)
- [x] Hosting test: endpoint exists without a seeded annotation
- [x] `program.cs` environment handling verified for both profile / no-profile
- [x] `./bin/dev pack` then run the AppHost on Linux; both resources Running; inspector shows clicks
- [x] Docs: hosting readme notes the explicit endpoint (and `Properties/` for external AppHosts)
- [x] `ganda repo audit` clean; build warning-free for touched projects; tests green

## Session

- Created: 183858 (2026-10-12)
- Implementation: grok 01a12c00-1bbe-7cb1-8593-1963f29d0f4e (2026-10-12)
- Review: Claude Opus 5.5 review oracle, effort 1, general (2026-10-12)

## Results

`AddTimeWarpStateDevTools` declares the UI endpoint with `WithHttpEndpoint(name: "http")`. Aspire updates an endpoint of the same name, so a launch profile that already added `http` is not duplicated. Without a profile the resource still has an `http` endpoint and Aspire can set `ASPNETCORE_URLS`.

`properties/` is now `Properties/` for the DevTools app, `sample-04-server`, and samples 00-03. Aspire reads launch settings only from that capital-P path, so the profile supplies `ASPNETCORE_ENVIRONMENT=Development` and the profile URL.

`source/timewarp-state-devtools/program.cs` keeps the non-Development exception handler and calls `StaticWebAssetsLoader.UseStaticWebAssets` in every environment. With the profile applied the host runs as Development. Without it the host stays Production, skips the developer exception page, and still serves static web assets.

Sample 04 calls `UseReduxDevTools()` so Development container validation can construct `CommitHandler`. The sample does not render the Redux DevTools component.

### How to validate

Smoke:

1. `./bin/dev pack`
2. `dotnet run --project samples/04-telemetry/apphost/sample-04-apphost.csproj`
3. `aspire describe --apphost samples/04-telemetry/apphost/sample-04-apphost.csproj --format Json`
4. Open the dashboard URL from the AppHost log. On the resources page, `timewarp-state-devtools` is Running and its link reads **TimeWarp.State DevTools**.
5. Open the sample `http` URL `/counter` and click **Click me**. Open the DevTools link. The action list shows `IncrementCountActionSet.Action` for `CounterState`.

Expect:

- `aspire describe` reports `sample-04-server` and `timewarp-state-devtools` as Running. The DevTools URL object has `displayName` `TimeWarp.State DevTools` (this run: `http://localhost:5288`). The server URL is the `http` profile endpoint (this run: `http://localhost:5086`).
- DevTools and server logs say `Hosting environment: Development`. DevTools is not stuck on Kestrel's default `http://localhost:5000` without `ASPNETCORE_URLS`.
- The counter starts at 3. One click makes it 8, and the inspector shows that action as Ok.
- `dotnet fixie timewarp-state-devtools-tests` passes. `DeclareTheHttpEndpointWithoutALaunchProfile` sees one `http` endpoint and a null port on a csproj that has no `Properties/launchSettings.json`. `LabelTheDashboardLinkAndPublishTheEndpoint` sees one `http` endpoint whose port stays 5288 when the in-repo profile is also applied.

Linux check on 2026-10-12, after `./bin/dev pack`: both resources Running, dashboard link **TimeWarp.State DevTools**, counter 3 → 8, inspector row `IncrementCountActionSet.Action` / `CounterState`. DevTools tests: 30 passed. `./bin/dev test` completed successfully. `ganda repo audit` passed (one pre-existing advisory, `kebab-path-names`, unrelated to `Properties/`). Hosting and DevTools projects build with no warnings of their own.

### Review

- Rounds: 1. Effort 1, roster: general.
- Final counts: bug 0, suggestion 0, nit 0 (all statuses 0).
- Disposition: **clean**. No wontfix or escalations.
- Artifacts: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Notes

- Related: 103 (companion DevTools app, PR #635), 058 (telemetry).
- Evidence on TWE-001: `aspire logs sample-04-server` stack trace from `EndpointReference.get_EndpointAnnotation`;
  decompiled Aspire.Hosting under the cockpit scratchpad `aspire-decomp/`.
- Aspire CLI: `aspire describe`, `aspire logs <resource>` are enough to verify; `aspire mcp tools` reported no
  MCP-exposing resources for this AppHost.
