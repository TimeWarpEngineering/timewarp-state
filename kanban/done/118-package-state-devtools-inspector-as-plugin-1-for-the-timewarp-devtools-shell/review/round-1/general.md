# Round 1 — general
**Date:** 2026-10-16
**Scope reviewed:** `99585466..f075d4a3` (all 27 files; focus on Hosting extensions, App csproj/targets, tests, docs)

## Summary

The change packs the published inspector as `TimeWarp.State.DevTools.App` (framework-dependent, no RID),
stamps its dll path onto the AppHost from `build/` + `buildTransitive/` targets, and has
`AddTimeWarpStateDevTools()` add an `ExecutableResource` running `dotnet <dll>`. The locator and env-var
override are gone; `AddTimeWarpStateDevToolsProject(path)` is the explicit inner-loop switch. The shape
matches the Aspire dashboard pattern, docs/readmes/release notes are consistent, no stale
`DevToolsProjectLocator` / `TIMEWARP_STATE_DEVTOOLS_PROJECT` references remain outside the retired-name
test, and the staged publish output contains `wwwroot/_framework/blazor.web.js` and the static-asset
endpoints manifest. Remaining risk: the run-mode-only contract leaks through the inspected resource,
the launch command assumes `dotnet` on PATH, and the out-of-tree test does not exercise the launch
shape the resource actually uses.

Verification note: a local `dotnet test tests/timewarp-state-devtools-tests/...` run hit the 590 s
reviewer timeout before reporting (the out-of-tree test restores Aspire into a fresh
`globalPackagesFolder` from nuget.org). The implementer's green `./bin/dev test` claim was not
independently reproduced. The killed run left `/tmp/tw-devtools-consumer-bEYUkj` behind (its `finally`
never ran); the reviewer may not `rm`.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:403
- Description: Requirement "Run mode only. The inspector never appears in publish output." The
  inspector resource gets `ExcludeFromManifest()`, but `WithTimeWarpStateDevTools` still adds
  `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` (an `EndpointReference` to the inspector) and a `WaitFor` on it
  to the **inspected** resource in every execution mode. In publish mode the inspected app's manifest
  / compose / deployment output then carries `{timewarp-state-devtools.bindings.http.url}` and a
  dependency on a resource that is not emitted. Aspire 13.6.1 has no check for references to excluded
  resources (no such string in Aspire.Hosting.dll), so this is a dangling reference rather than an
  error at publish time. `AddTimeWarpStateDevTools` also throws in publish mode if the stamped dll is
  absent, even though nothing will run it. Not executed against a publisher (PLAUSIBLE).
- Suggestion: In `WithTimeWarpStateDevTools`, return `builder` unchanged when
  `builder.ApplicationBuilder.ExecutionContext.IsPublishMode`. Consider skipping the file-exists check
  in publish mode too. Add a test that builds with `DistributedApplicationOperation.Publish` and asserts
  the inspected resource has no `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` callback and no `WaitAnnotation` on
  the inspector. Update the Design region / readme line about publish output.
- Status: open

### Issue 2 — Severity: suggestion
- File: tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs:128
- Description: The requirement is "a consumer AppHost outside this repo's folder tree … starts the
  inspector from the package". The test builds the consumer AppHost and reads its stamp, but then
  starts the dll itself with `ASPNETCORE_URLS` and `ASPNETCORE_ENVIRONMENT=Production`. The resource
  `AddTimeWarpStateDevTools` builds is never run, so the real launch shape (command `dotnet`, working
  directory `tools/`, port via `ASPNETCORE_HTTP_PORTS`, no `ASPNETCORE_ENVIRONMENT`) is unproven, and
  only `/` is requested — not `/_framework/blazor.web.js`, which is the static-asset path the
  inspector's Design region calls out as the fragile part outside Development.
- Suggestion: At minimum launch with `ASPNETCORE_HTTP_PORTS=<port>` (no `ASPNETCORE_URLS`, no
  `ASPNETCORE_ENVIRONMENT`) and also GET `/_framework/blazor.web.js` expecting 200. Better: drive the
  consumer through `AddTimeWarpStateDevTools` (e.g. take `Command`/`Args`/`WorkingDirectory` and the
  endpoint env from the built resource, or run the consumer AppHost and wait for the resource) so the
  test proves what the AppHost actually starts.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs:274
- Description: `AppLaunchCommand = "dotnet"` is resolved from the AppHost's PATH. The Design region
  says it "is already on PATH because the AppHost is a dotnet process", which is not generally true: an
  AppHost started through its apphost executable or from an IDE with a `DOTNET_ROOT` install (e.g.
  `~/.dotnet` preview SDK for net11) need not have that `dotnet` on PATH, and when several installs
  exist the PATH `dotnet` may lack the net11 `Microsoft.AspNetCore.App` framework the inspector needs.
  The inspector then fails to start (or starts on a different runtime) with no hint from Hosting.
- Suggestion: Resolve the host the way the SDK does: `DOTNET_HOST_PATH` when set, else the running
  muxer (`Environment.ProcessPath` when its file name is `dotnet`/`dotnet.exe`), else
  `DOTNET_ROOT/dotnet`, else `"dotnet"`. Keep the constant as the fallback, fix the Design region and
  readme wording, and cover the resolution order in `AddTimeWarpStateDevTools_Should`.
- Status: open

### Issue 4 — Severity: suggestion
- File: source/timewarp-state-devtools-app/timewarp-state-devtools-app.csproj:185
- Description: `PublishDevToolsAppOnBuild` runs `Publish` on the inspector on every build with global
  properties (`SelfContained=false;UseAppHost=false;PublishDir=…`) that differ from the solution's own
  build of `timewarp-state-devtools.csproj`. MSBuild treats that as a second project instance that
  builds into the same `bin/`/`obj/` folders. In a parallel solution or test build both instances can
  compile/copy the same intermediate files at once (intermittent file-in-use / MSB3026 / CS2012), and
  the default build's apphost output and the publish instance's view of it disagree. Not reproduced
  here (PLAUSIBLE).
- Suggestion: Let the existing `ProjectReference` build the inspector with default properties, then
  publish with `NoBuild=true` (and the same `Configuration`) so the publish instance only copies
  outputs, or give the publish instance its own intermediate/output paths. Apply the same to the
  pack-time publish in `AddDevToolsToolsToPackage`.
- Status: open

### Issue 5 — Severity: nit
- File: source/timewarp-state-devtools-app/timewarp-state-devtools-app.csproj:207
- Description: `tools/` packs the whole publish folder, including `appsettings.Development.json`,
  `tsconfig.json`, `web.config`, `*.pdb` and `*.xml` doc files. None are needed to run the inspector
  from the package (it runs as Production under Aspire).
- Suggestion: Exclude `tsconfig.json`, `web.config`, `appsettings.Development.json` and `*.xml` from
  `_DevToolsPublishedFile` (keep pdbs if symbolized stack traces are wanted), or mark them
  `CopyToPublishDirectory="Never"` in the inspector project.
- Status: open
