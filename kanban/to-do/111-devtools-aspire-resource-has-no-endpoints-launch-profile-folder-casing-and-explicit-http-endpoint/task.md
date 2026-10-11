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

- [ ] `WithHttpEndpoint("http")` in `add-timewarp-state-dev-tools.cs`; Design region updated
- [ ] `git mv properties Properties` for DevTools app and sample-04-server (and 00-03 if cheap)
- [ ] Hosting test: endpoint exists without a seeded annotation
- [ ] `program.cs` environment handling verified for both profile / no-profile
- [ ] `./bin/dev pack` then run the AppHost on Linux; both resources Running; inspector shows clicks
- [ ] Docs: hosting readme notes the explicit endpoint (and `Properties/` for external AppHosts)
- [ ] `ganda repo audit` clean; build warning-free for touched projects; tests green

## Session

- Created: 183858 (2026-10-12)

## Notes

- Related: 103 (companion DevTools app, PR #635), 058 (telemetry).
- Evidence on TWE-001: `aspire logs sample-04-server` stack trace from `EndpointReference.get_EndpointAnnotation`;
  decompiled Aspire.Hosting under the cockpit scratchpad `aspire-decomp/`.
- Aspire CLI: `aspire describe`, `aspire logs <resource>` are enough to verify; `aspire mcp tools` reported no
  MCP-exposing resources for this AppHost.
