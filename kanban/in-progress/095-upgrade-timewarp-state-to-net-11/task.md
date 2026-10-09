# Upgrade timewarp-state to .NET 11

## Description

Upgrade TimeWarpEngineering/timewarp-state from .NET 10 to .NET 11 (`net11.0`).
Bring the SDK pin, default TFM, Microsoft/ASP.NET/Blazor package versions, CI toolchain,
devcontainer, local tools, samples/tests, and docs that still mention .NET 8/9/10 into
alignment with .NET 11. Keep the full `dev workflow` pipeline green. Do not land feature
commits on master — claim → task worktree → PR only.

As of 2026-10-02, .NET 11 is pre-GA (Preview 7 / RC.1 available; GA scheduled 2026-11-10
as STS). Implementation may target preview/RC packages first and re-pin to GA once
published; note which channel was used in Results.

## Requirements

- Bump `global.json` SDK to an installed .NET 11 SDK (e.g. `11.0.100` or current preview/RC band); decide `allowPrerelease` / `rollForward` for preview vs GA.
- Bump default `TargetFramework` in root `Directory.Build.props` from `net10.0` to `net11.0`. Leave analyzer/source-generator projects on `netstandard2.0`.
- Update CPM entries in `Directory.Packages.props` that gate the TFM: `Microsoft.AspNetCore.*`, `Microsoft.Extensions.*`, `System.Net.Http.Json` → 11.0.x (or matching preview).
- Refresh CI so runners install .NET 11 (`actions/setup-dotnet@v6` already reads `global.json`; confirm `ubuntu-latest` can resolve the SDK).
- Refresh DevContainer / local tool manifest as needed so `dotnet` tools run on SDK 11.
- Keep builds, unit/integration/E2E tests, pack, and sample verify green via `dotnet run --file tools/dev-cli/dev.cs -- workflow`.
- Update docs that pin older TFMs (`.ai/05-dotnet-conventions.md` still says net8.0; persistence note for `PersistentStateAttribute`; any README/CI docs).
- No product feature work on master; only kanban/** for this task's publish, then a separate implementation PR from the claim worktree.

## Checklist

### Prerequisites (update first — recommended order)

- [x] 1. Install / confirm .NET 11 SDK on agent machines; set `global.json` `sdk.version` to 11.0.x (preview/RC until GA). Set `allowPrerelease: true` while on preview; restore `false` after GA if desired. Keep or revisit `rollForward: latestMinor`.
- [x] 2. CI toolchain: verify `.github/workflows/workflow.yml` `actions/setup-dotnet@v6` + `global-json-file: global.json` installs .NET 11 on `ubuntu-latest`. Add an explicit `dotnet-version` only if image/catalog lag requires it. No custom Dockerfiles in-repo today.
- [x] 3. DevContainer: confirm `mcr.microsoft.com/devcontainers/universal:2` ships or can feature-install .NET 11; update image/features if not.
- [x] 4. Local tools: validate `.config/dotnet-tools.json` tools (fixie.console, dotnet-outdated-tool, docfx, etc.) restore/run under SDK 11; bump tool versions if restore fails.
- [x] 5. TFM bump: `Directory.Build.props` `TargetFramework` `net10.0` → `net11.0`. Do **not** change `source/timewarp-state-analyzer` or `source/timewarp-state-source-generator` (`netstandard2.0`).
- [x] 6. CPM Microsoft packages (TFM-gated): bump in `Directory.Packages.props` — `Microsoft.AspNetCore.Components.Web`, `.WebAssembly`, `.WebAssembly.DevServer`, `.WebAssembly.Server`, `.Mvc.Testing`, `.TestHost`; `Microsoft.Extensions.Logging.Abstractions` / `.Configuration`; `System.Net.Http.Json` — all currently `10.0.12` → `11.0.x`.
- [x] 7. Roslyn / analyzers floor: re-evaluate `Microsoft.CodeAnalysis.CSharp` (held at `4.14.0` for SDK 10 compatibility) and `Microsoft.CodeAnalysis.Analyzers` (`5.9.0`) against SDK 11; bump only as required so analyzer + source generator still load for consumers.
- [x] 8. Test SDK stack: `Microsoft.NET.Test.Sdk`, Playwright, Fixie adapters. Keep MSTest on 3.x until Playwright binds MSTest 4 (existing hold-back in props comments).
- [x] 9. Aspire / OpenTelemetry sample stack: `Aspire.Hosting.AppHost` (`13.5.4`) and OTel packages — bump to versions that support `net11.0` for sample-04.
- [x] 10. TimeWarp ecosystem packages (external gates): confirm `TimeWarp.Mediator*`, `TimeWarp.Nuru*`, `TimeWarp.Amuru*`, `TimeWarp.SourceGenerators`, `TimeWarp.Build.Tasks`, `TimeWarp.Fixie` publish builds compatible with net11 / SDK 11 before bumping; coordinate sibling upgrades if they block restore.
- [x] 10a. Eliminate the blocking clone path: remove TimeWarp.State's dependency on AnyClone (1.1.6) / TypeSupport (1.2.0) or otherwise make StateTransactionBehavior's clone non-blocking on browser WASM; add `<ItemGroup><SupportedPlatform Include="browser" /></ItemGroup>` to the OS-neutral library projects so CA1416 flags any other blocking calls; add a real-browser (WASM) test proving an action through StateTransactionBehavior succeeds on net11. Must be done before the TFM bump (item 5) is considered done.
- [x] 11. Regenerate any `packages.lock.json` / restore; fix TypeScript/MSBuild if `Microsoft.TypeScript.MSBuild` needs a companion `tsconfig` change.
- [x] 12. Docs & conventions: update `.ai/05-dotnet-conventions.md` (stale net8.0), persistence `PersistentStateAttribute` collision guidance for .NET 11, and any other pins. Library migration doc `documentation/migrations/migration10-11.md` is package 10→11 history (not TFM) — do not confuse with this TFM upgrade; add a short .NET 10→11 note elsewhere if needed.

### Implementation / verification

- [x] Apply TFM + package + toolchain edits in the claim worktree (not on master).
- [x] `dotnet run --file tools/dev-cli/dev.cs -- workflow` green (assert-version-ssot → clean → build → test → e2e → pack → verify-samples).
- [ ] `ganda repo audit` reviewed; no new failures introduced by the upgrade.
- [x] Conventional commit + PR; no master feature commits.

### Documentation

- [x] Update README / AI context if they state a minimum .NET version.
- [x] Record preview vs GA package versions used in Results.

## Notes

### Current baseline (master, inspected 2026-10-02 Asia/Bangkok)

| Area | Current | Target for this task |
|---|---|---|
| `global.json` | SDK `10.0.301`, `rollForward: latestMinor`, `allowPrerelease: false` | SDK `11.0.x`, revisit prerelease/rollForward |
| Default TFM | `Directory.Build.props` → `net10.0` | `net11.0` |
| Analyzer / source-gen TFM | `netstandard2.0` (intentional) | keep `netstandard2.0` |
| AspNetCore / Extensions / Http.Json | CPM `10.0.12` | `11.0.x` |
| CI | single job, `ubuntu-latest`, `setup-dotnet@v6` + `global-json-file` | same shape; must resolve SDK 11 |
| Dockerfiles | none in repo | n/a |
| DevContainer | `mcr.microsoft.com/devcontainers/universal:2` | ensure .NET 11 available |
| Product version SSOT | `TimeWarpStateVersion` / `<Version>` `12.0.0-beta.7` | independent of TFM bump unless release policy says otherwise |
| Multi-targeting | none for product libs (single default TFM) | stay single-TFM unless a decision is made to multi-target net10+net11 for consumers |

### Impact / risk findings

- **SDK channel**: .NET 11 GA is scheduled 2026-11-10; until then builds need preview/RC SDK and `allowPrerelease`. CI agents and `ubuntu-latest` catalog lag is the first practical blocker.
- **Single TFM strategy**: repo already centralizes TFM in `Directory.Build.props`; one-line change cascades to all samples/tests except netstandard2.0 analyzer/generator. No multi-target matrix today — prefer stay single-target `net11.0` unless library consumers need continued net10 support (would be a deliberate multi-target decision).
- **Blazor / ASP.NET Core 11**: follow Microsoft 10→11 migration guide (TFM + package bumps). Watch Blazor SSR antiforgery defaults, NavMenu CSP/module changes in templates, and QuickGrid URL pagination if samples copy template markup. Library code using `Microsoft.AspNetCore.Components.*` may need API review for breaking changes.
- **`PersistentStateAttribute` collision**: docs already require aliasing TimeWarp's attribute vs `Microsoft.AspNetCore.Components.PersistentStateAttribute` on .NET 10; re-verify on .NET 11 and update the persistence topic.
- **Roslyn floor**: analyzer package is consumed by downstream projects; raising `Microsoft.CodeAnalysis.CSharp` too aggressively raises minimum SDK for consumers — keep the documented floor strategy.
- **MSTest / Playwright hold-back**: do not blindly major-bump MSTest to 4.x; E2E discovery breaks until Playwright binds the renamed TestFramework assembly.
- **Aspire sample**: AppHost TFM/package compatibility is a separate gate from core library packages.
- **External TimeWarp packages**: restore may fail if Mediator/Nuru/Amuru/SourceGenerators lack net11-capable assets — those are ordered prerequisites (checklist item 10), not silent pin-backs.
- **Stale AI conventions**: `.ai/05-dotnet-conventions.md` still says target net8.0 — documentation debt, not a build gate, but must be fixed in this upgrade.
- **Prior art**: done task `030-update-to-dotnet-9` is the playbook shape; `035-migrate-powershell-scripts-to-dotnet10` already moved CI to C# `dev workflow` (no PowerShell workflow scripts to migrate for this bump).

#### **Blocking waits throw on single-threaded browser WASM (.NET 11) — found 2026-10-09**

- .NET 11 throws `PlatformNotSupportedException` on any potentially blocking `SemaphoreSlim.Wait()` on single-threaded browser WASM, even `Wait(0)` on an available semaphore. Introduced by dotnet/runtime PR #123329 (https://github.com/dotnet/runtime/pull/123329); issue #131859 (https://github.com/dotnet/runtime/issues/131859) was closed as by design (Pavel Savara; Jan Kotas preferred the simple rule over softening). .NET 10 WASM returned normally.
- Impact on TimeWarp.State 12.0.0-beta.8: `StateTransactionBehavior` -> AnyClone 1.1.6 -> TypeSupport 1.2.0 `ExtendedTypeCache.GetOrCreate` -> `SemaphoreSlim.Wait`, so every action through `StateTransactionBehavior` fails in the browser on net11 (seen in timewarp-architecture after its .NET 11 RC1 upgrade, e.g. Ctrl-K `ApplicationState.SetActiveModal`; confirmed with a Playwright test in real WASM mode). Server-side tests pass because servers are multi-threaded, so browser tests are required.
- Recommended fix per the runtime team: fix the library so it does not block on browser (async `WaitAsync` or no lock on single-threaded browser); precedent grpc-dotnet PR #2756 (https://github.com/grpc/grpc-dotnet/pull/2756). For OS-neutral libraries add `<SupportedPlatform Include="browser" />` to get CA1416 warnings such as `SemaphoreSlim.Wait(int) is unsupported on: browser`.
- Disabling `UseStateTransactionBehavior` is NOT an acceptable fix (hides the bug); the root-cause fix belongs in TimeWarp.State.
- Downstream: timewarp-architecture is already on net11 and TimeWarp.State 12.0.0-beta.8; its kanban task 289 is blocked on this fix.

### Out of scope for implementation on this publish

- This kanban publish is analysis + task creation only. Product code / TFM edits land later via claim → worktree → PR.

## Session

- Created: 506106 (2026-10-02 Asia/Bangkok)
- Analysis: State agent on TWE-001; master at `/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-state/master`
- .NET 11 status checked via Microsoft Learn / dotnet/core releases (preview/RC; GA 2026-11-10)

## Results

Implemented on branch `task/095-upgrade-timewarp-state-to-net-11` (PR to master; Steven merges).

**10a (blocking-wait bug, top priority) — fixed**
- Removed `AnyClone` 1.1.6 and `TypeSupport` 1.2.0 from TimeWarp.State (csproj + CPM). Both unmaintained; TypeSupport `ExtendedTypeCache.GetOrCreate` used `SemaphoreSlim.Wait`, which .NET 11 throws on single-threaded browser WASM.
- New non-blocking deep clone `TimeWarp.Features.Cloning` (`source/timewarp-state/features/cloning/`): per-type plans in a `ConcurrentDictionary`, one synchronous walk, per-call reference map (cycles/shared refs), no waits. Keeps AnyClone defaults: `[IgnoreDataMember]` / `[NonSerialized]` / `[JsonIgnore]` members skipped and left at constructor values (State Guid stays unique); parameterless ctor, else fewest-arg ctor with defaults, else uninitialized. Public opt-in API `CloneExtensions.Clone<T>()` / `Clone<T>(CloneErrorHandler)`; consumers that used AnyClone transitively swap `using AnyClone;` for `using TimeWarp.Features.Cloning;`.
- `StateTransactionBehavior` uses it (ICloneable still wins). `<SupportedPlatform Include="browser" />` added to timewarp-state-policies and timewarp-state-telemetry (timewarp-state and -plus already had it); no CA1416 hits.
- Real-browser proof: new E2E `CloneTestPageTests.CloneSuitePassesInServerAndWasm` runs the clone suite in Server then WebAssembly. Baseline on net11 with AnyClone (before the fix): E2E 7 failed / 3 passed / 3 skipped; `CounterTests` failed in the WASM pass (click did not increment: StateTransactionBehavior threw). After fix: 11 passed / 0 failed / 3 skipped (14 total).
- Unit tests: `tests/timewarp-state-tests/cloning/deep-cloner-tests.cs` (private fields, ignored members, nested collections, cycles, multi-dim arrays, structs, delegates/types shared, null).

**Toolchain / packages (channel: .NET 11 RC1)**
- SDK `11.0.100-rc.1.26425.128` (`global.json`, `allowPrerelease: true`, `rollForward: latestMinor`). Re-pin to 11.0.100 GA after 2026-11-10.
- TFM `net10.0` → `net11.0` (Directory.Build.props). Analyzer / source generator remain `netstandard2.0`.
- Microsoft.AspNetCore.* / Microsoft.Extensions.Logging.* / System.Net.Http.Json `10.0.12` → `11.0.0-rc.1.26425.128`.
- CI: unchanged shape; `setup-dotnet@v6` + `global-json-file` installs the exact RC SDK on ubuntu-latest.
- DevContainer: added `ghcr.io/devcontainers/features/dotnet:2` `version: 11.0-preview`.
- Local tools restore and run on SDK 11 unchanged.
- Roslyn floor kept (`Microsoft.CodeAnalysis.CSharp` 4.14.0, Analyzers 5.9.0): loads fine under SDK 11.
- Test stack unchanged (Test.Sdk 18.10.1, Playwright.MSTest 1.62.0, MSTest 3.11.1 hold-back kept, Fixie 4.2.0). Analyzer tests now pin net11 reference assemblies (`Microsoft.NETCore.App.Ref` 11.0.0-rc.1.26425.128) because Analyzer.Testing 1.1.4 has no `Net110`.
- Aspire 13.5.4 / OTel 1.19.x: sample-04 builds on net11 without bumps.
- TimeWarp ecosystem: `TimeWarp.Nuru` / `Nuru.DevCli` 3.0.0-beta.76 → beta.79 (hook audit). Nuru beta.79 moved commands to TimeWarp.Mediator, so `TimeWarp.Mediator.*` 14.0.0-beta.1 → beta.4 and dev-cli handlers now return `Task<Unit>` with `global using TimeWarp.Mediator`. Others (Amuru 1.1.1, SourceGenerators beta.11, Build.Tasks 1.0.0, Fixie 3.1.0, Terminal 1.0.2) restore and build on net11 unchanged.
- No packages.lock.json in repo; TypeScript MSBuild unchanged.
- Docs: `.ai/05-dotnet-conventions.md` (net11.0), `claude.md`, persistence topic (alias still needed on .NET 11), `.ai/other/references.md` (AnyClone entry replaced).

**Verification (TWE-001, Ubuntu-26.04 WSL, `dotnet --version` = 11.0.100-rc.1.26425.128)**
`dotnet run --file tools/dev-cli/dev.cs -- workflow` → `Pipeline SUCCEEDED` (assert-version-ssot → clean → build → test → e2e → pack → verify-samples). Build 0 errors. Tests: analyzer 33, source generator 15, state 73 (+1 skipped), plus 31 (+1), telemetry 13, client integration 56 (+1), architecture 7 (+1) = 228 passed, 4 skipped, 0 failed. E2E 11 passed, 3 skipped, 0 failed. Samples verified.

**Deferred / follow-ups**
- New .NET 11 warnings left as-is: BL0010 (`InvokeAsync<object>` → `InvokeVoidAsync` in json-request-handler / redux-dev-tools-interop), ASPDEPR011 (`UseWebAssemblyDebugging` in test-app-server and sample-00), NU1510 (prunable `Microsoft.Extensions.Logging.Abstractions` / `System.Net.Http.Json` refs).
- `ganda repo audit` not re-run separately beyond the pre-commit hook.
- Re-pin SDK/packages to GA after 2026-11-10; set `allowPrerelease` back to false if desired.
