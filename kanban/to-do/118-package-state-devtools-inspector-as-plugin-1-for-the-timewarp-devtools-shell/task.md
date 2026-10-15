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

- [ ] Inspector published into a NuGet package (dashboard pattern); RID/runtime strategy recorded
- [ ] Hosting runs the packaged executable; `DevToolsProjectLocator` + source `AddProject` removed
- [ ] Optional explicit dev-only project mode (if needed), never a folder walk
- [ ] Out-of-tree consumer test proves it works from packages alone
- [ ] Run-mode only / not in publish output
- [ ] Readme + Design regions; version aligned with Hosting; release
- [ ] Gates: build, tests, `ganda repo audit`

## Sequencing

**Do this first.** TimeWarpEngineering/timewarp-architecture task 335-001 (add DevTools to
Architecture's AppHost with one `AddTimeWarpStateDevTools()` call) depends on this shipping. (This
replaces the earlier plan to get the source-path setup working in Architecture first; that would
have required the env-var workaround.)

## Session

- Created: 1230420 (2026-10-12)
- 2026-10-15: rescoped by the timewarp-architecture cockpit per Steve. Packaged app, dashboard
  pattern; locator removed; shell/plugin work moved to "Later"; now precedes Architecture 335-001.

## References

- Task 103 (State DevTools companion app)
- TimeWarpEngineering/timewarp-architecture task 335 (DevTools shell) and 335-001 (AppHost integration)
- Aspire dashboard packaging: `Aspire.Dashboard.Sdk` (executable resource from a NuGet package)

## Learnings
- 2026-10-15 (boyscout, to pass the publish audit): TimeWarp.Nuru/Nuru.DevCli 3.0.0-beta.80 → 3.0.0 with TimeWarp.Amuru/Amuru.Tools → 2.0.0 (Nuru 3.0.0 requires Amuru ≥ 2.0.0; NU1605 otherwise); root .gitignore lines for the new kanban-porcelain / routine-journals audit checks. `dev build` 0 errors; 158 pre-existing warnings (e.g. BL0010 from the RC2 SDK analyzers), unrelated to the bump.
