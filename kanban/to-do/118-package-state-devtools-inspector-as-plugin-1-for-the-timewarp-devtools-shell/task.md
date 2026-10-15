# Ship the State DevTools inspector as a packaged app the Hosting package runs (no source, no folder walk)

## Description

`TimeWarp.State.DevTools.Hosting` (the Aspire extension package, 12.0.0-beta.13) adds the task 103
DevTools inspector to an AppHost with `AddTimeWarpStateDevTools()` / `WithTimeWarpStateDevTools()`.
Today it does that by calling Aspire `AddProject` on the inspector's **source** `.csproj`, which
`DevToolsProjectLocator` finds by walking parent folders for
`source/timewarp-state-devtools/timewarp-state-devtools.csproj` (or the
`TIMEWARP_STATE_DEVTOOLS_PROJECT` override).
- It only works **inside this repo**, which is why the State demo works and Architecture's AppHost
  (TimeWarpEngineering/timewarp-architecture) can't use it.
- The env-var override is a workaround, not a fix.

**Goal:** any AppHost references `TimeWarp.State.DevTools.Hosting`, calls
`AddTimeWarpStateDevTools()`, and gets the inspector. No source, no paths, no env vars, like any
other Aspire integration.

## Decision (Steve + timewarp-architecture cockpit, 2026-10-15): follow the Aspire dashboard pattern

Ship the inspector the way **Aspire ships its own dashboard**: `Aspire.Dashboard.Sdk` is a NuGet
package containing the published dashboard app (for example
`~/.nuget/packages/aspire.dashboard.sdk.linux-x64/13.6.1/tools/Aspire.Dashboard`), which the AppHost
starts as an **executable resource**.

Precedent, verified in Architecture's running AppHost: the dashboard resource's
`executable.path` points into that package. `Aspire.Hosting.EntityFrameworkCore` similarly runs
`dotnet-ef` as a `Tool` resource. Containers are the norm for third-party, non-.NET services (Redis,
Postgres…), **not** for a .NET dev-time companion app.

Alternatives considered:
- **A .NET tool run as an Aspire Tool resource** (`dotnet-ef` style). Simpler to pack, but it may lean
  on non-public or preview Aspire APIs in 13.6. If the dashboard pattern proves impractical, this is
  the fallback; record why.
- **A container image** (`AddContainer`). Rejected: an image build, registry and Docker as
  dependencies for a dev tool.

## Requirements

- **Package the published inspector:** a NuGet package (name suggestion `TimeWarp.State.DevTools.App`,
  or per-RID packages like `Aspire.Dashboard.Sdk.<rid>` if needed) contains the **published**
  inspector app.
  - Framework-dependent is fine if the AppHost machine has the runtime; record self-contained vs
    framework-dependent and the RID strategy.
  - Its version equals the Hosting package version, published in the same release.
- **The Hosting package runs the packaged executable:** `AddTimeWarpStateDevTools()` resolves the
  inspector from the restored package, for example through a package dependency plus an MSBuild
  property or generated path, as the dashboard SDK does, and adds it as an executable resource with
  the same endpoint, `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` wiring and wait behaviour as today.
- **Delete `DevToolsProjectLocator`** and the source `AddProject` path. In-repo development (the State
  demo AppHost) may keep an explicit, clearly named project-reference mode **only if** it's needed
  for inner-loop work on the inspector itself. That's a deliberate developer switch, never the
  default and never a folder walk. Record it.
- **Run mode only.** The inspector never appears in publish output. If the Hosting package can
  declare that, do it. Otherwise document that consumers must guard it.
- **Tests:**
  - a consumer AppHost **outside this repo's folder tree**, for example a temp-folder sample using
    the locally packed packages, starts the inspector from the package;
  - `WithTimeWarpStateDevTools` wiring;
  - endpoint naming unchanged;
  - no source path resolution remains.
- **Docs:** the Hosting readme and Design regions describe the package-based shape. The State demo
  AppHost uses the same public call.
- **Keep task 103's separate-store rule.**

## Later (not this task)

The DevTools **shell and plugin contract** (TimeWarpEngineering/timewarp-architecture task 335) builds
on this. Making the inspector a plugin implementing the shell's minimal contract, and moving any
generic pieces to the shell, is follow-up work once 335 defines the contract. File it then. The
contract must not depend on State.

## Checklist

- [x] Inspector published into a NuGet package (dashboard pattern); RID/runtime strategy recorded
- [x] Hosting runs the packaged executable; `DevToolsProjectLocator` + source `AddProject` removed
- [x] Optional explicit dev-only project mode (if needed), never a folder walk
- [x] Out-of-tree consumer test proves it works from packages alone
- [x] Run-mode only / not in publish output
- [x] Readme + Design regions; version aligned with Hosting; release
- [x] Gates: build, tests, `ganda repo audit`

## Sequencing

**Do this first.** TimeWarpEngineering/timewarp-architecture task 335-001 (add DevTools to
Architecture's AppHost with one `AddTimeWarpStateDevTools()` call) depends on this shipping. (This
replaces the earlier plan to get the source-path setup working in Architecture first; that would
have required the env-var workaround.)

## Session

- Created: 1230420 (2026-10-12)
- 2026-10-15: rescoped by the timewarp-architecture cockpit per Steve. Packaged app, dashboard
  pattern; locator removed; shell/plugin work moved to "Later"; now precedes Architecture 335-001.
- 2026-10-15: packaged `TimeWarp.State.DevTools.App` and pointed Hosting at the published dll.
  Version pin is 12.0.0-beta.14. NuGet release is not cut here.
- 2026-10-16: `dotnet run --file tools/dev-cli/dev.cs -- pack` verified the set. App nuspec
  dependencies are none. Hosting depends on Aspire.Hosting and TimeWarp.State.DevTools.App.
  `./bin/dev verify-samples` printed "Samples verified successfully!". `./bin/dev test` had
  already printed "Tests completed successfully!". `ganda repo audit` exited 0 with one
  pre-existing kebab-path advisory (Blazor `*.lib.module.*` names and backlog 110).
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-15T17:44:53Z
- Review oracle round 1 (2026-10-16): general reviewer, claude-opus-5-5, session
  3bc6725a-883d-4f3b-b451-3c8caedd96e0, effort 3 (by-diff, 976 lines). 5 open findings (1 bug,
  3 suggestions, 1 nit) in `review/round-1/merged.md`; returned Blocked for a fix pass.
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-15T17:59:33Z
- 2026-10-16: fix pass for review round 1. Publish mode does not wire the inspector. The launch command resolves the AppHost .NET host. The inspector is published after its build. The package `tools/` folder omits development settings, `tsconfig.json`, `web.config`, and XML docs. `dotnet test tests/timewarp-state-devtools-tests/timewarp-state-devtools-tests.csproj` passed 66. `ganda repo audit` exited 0 with the pre-existing kebab-path advisory.
- Review oracle round 2 (2026-10-16): general reviewer, claude-opus-5-5, session
  532d65d0-a89f-4bd9-949d-fc9f1773beb0, effort 2 (by-diff, 275 lines, fix delta
  `9f4be5b8..04c63679`). M1–M5 fixed, no new findings. Disposition clean (`review/disposition.md`).

## References

- Task 103 (State DevTools companion app)
- TimeWarpEngineering/timewarp-architecture task 335 (DevTools shell) and 335-001 (AppHost integration)
- Aspire dashboard packaging: `Aspire.Dashboard.Sdk` (executable resource from a NuGet package)

## Learnings
- 2026-10-15 (boyscout, to pass the publish audit): TimeWarp.Nuru/Nuru.DevCli 3.0.0-beta.80 → 3.0.0 with TimeWarp.Amuru/Amuru.Tools → 2.0.0 (Nuru 3.0.0 requires Amuru ≥ 2.0.0; NU1605 otherwise); root .gitignore lines for the new kanban-porcelain / routine-journals audit checks. `dev build` 0 errors; 158 pre-existing warnings (e.g. BL0010 from the RC2 SDK analyzers), unrelated to the bump.
- NuGet restore drops a `ProjectReference` whose `ReferenceOutputAssembly` is false, so Hosting must reference the App project normally or the packed nuspec will not depend on `TimeWarp.State.DevTools.App`.
- `IResourceBuilder<T>` is invariant. `WaitFor` takes `IResourceBuilder<IResource>`. Rewrap with `CreateResourceBuilder` before waiting.
- Stamp `timewarpstatedevtoolspath` in a target `BeforeTargets="GetAssemblyAttributes"`. An evaluation-time `AssemblyAttribute` in the imported targets file was not emitted.
- Pack the published files as `TfmSpecificPackageFile`. A `None` item added only before `GenerateNuspec` is too late, and `%(RecursiveDir)` on `None` batches against the targets file already in that item list.
- `./bin/dev` is a gitignored AOT build. CI runs `dotnet run --file tools/dev-cli/dev.cs`. A local `./bin/dev pack` keeps the allow-list from the last `self-install` until that binary is rebuilt.

## Results

`TimeWarp.State.DevTools.App` 12.0.0-beta.14 is one framework-dependent package (no runtime identifier, `UseAppHost=false`). Its `tools/` folder is the published inspector. `TimeWarp.State.DevTools.Hosting` depends on that package at the same version. The package targets stamp `timewarpstatedevtoolspath` onto the AppHost. `AddTimeWarpStateDevTools()` starts that dll with the AppHost's .NET host: `DOTNET_HOST_PATH` when it is set, otherwise the running `dotnet` muxer, otherwise `DOTNET_ROOT/dotnet`, otherwise `dotnet`. The resource is an `ExecutableResource` named `timewarp-state-devtools`, the endpoint name is still `http`, and `ExcludeFromManifest` keeps it out of publish output. In publish mode, `WithTimeWarpStateDevTools` returns the inspected resource unchanged: no `WaitFor` on the inspector and no `TIMEWARP_STATE_DEVTOOLS_ENDPOINT`. `DevToolsProjectLocator` and `TIMEWARP_STATE_DEVTOOLS_PROJECT` are gone. `AddTimeWarpStateDevToolsProject(projectPath)` is the explicit inner-loop switch. The sample AppHost uses the same public `AddTimeWarpStateDevTools()` call. The inspector still has its own store. The package build publishes the inspector after that project's build (`NoBuild` on the publish). Pack omits `appsettings.Development.json`, `tsconfig.json`, `web.config`, and `*.xml` from `tools/` and still ships the pdbs.

### How to validate

Smoke: `dotnet test tests/timewarp-state-devtools-tests/timewarp-state-devtools-tests.csproj`.

Expect: 66 passed. `AddTimeWarpStateDevTools_Should.LeaveTheInspectedAppUnwiredWhenPublishing` builds with `--operation publish` and finds no `WaitAnnotation` on the inspector and no `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` callback. `ResolveTheDotNetHostInOrder` covers host path, muxer, `DOTNET_ROOT/dotnet`, then `dotnet`. `PackagedDevToolsApp_Should.StartFromAnOutOfTreeAppHost` packs App and Hosting into a temp feed, builds an Aspire AppHost outside this repo that only package-references Hosting, and starts the packaged dll with the resource's command, the dll directory as the working directory, and only `ASPNETCORE_HTTP_PORTS`. The nupkg contains `tools/wwwroot/_framework/blazor.web.js` and a `tools/*.pdb`, and does not contain `appsettings.Development.json`, `tsconfig.json`, `web.config`, or `*.xml` under `tools/`. GET `/` returns a body containing `TimeWarp State DevTools`. GET `/_framework/blazor.web.js` returns 200. The stamped path is `tools/TimeWarp.State.DevTools.dll` under the restored `timewarp.state.devtools.app` package, not a csproj in this repo. `ganda repo audit` exits 0. The kebab-path advisory remains (Blazor `*.lib.module.*` names, backlog 110, and the NuGet `TimeWarp.State.DevTools.App.targets` filename).

### Review

- Disposition: **clean** (`review/disposition.md`). Nothing is open and nothing is wontfix.
- Rounds: 2. Roster: general. Effort: round 1 effort 3 (976 lines); round 2 effort 2 (275-line fix delta).
- Final counts (`review/round-2/merged.md`): bug 1 fixed; suggestion 3 fixed; nit 1 fixed; 0 open; 0 wontfix. Round 2 raised no new findings.
- Fix pass: publish mode leaves the inspected app unwired (M1); the out-of-tree test launches with the resolved command, `ASPNETCORE_HTTP_PORTS`, and checks `blazor.web.js` (M2); host resolution order (M3); publish after the inspector build (M4); package `tools/` omits development files and XML docs (M5).
- Round 2 verification: the test project built with 0 errors. The new hosting tests passed, including `ResolveTheDotNetHostInOrder` and `LeaveTheInspectedAppUnwiredWhenPublishing`. A Release pack of the App had no excluded files under `tools/`. The full `PackagedDevToolsApp_Should` run (cold nuget.org restore) outlasted the reviewer timeout, so the 66/66 result was not reproduced in review.
- Paths: `review/review-framework.md`, `review/round-1/`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`.

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5
- Round 2: Done — M1–M5 fixed, no new findings; disposition clean
