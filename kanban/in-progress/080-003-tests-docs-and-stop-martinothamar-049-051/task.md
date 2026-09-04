# Tests docs and stop martinothamar 049-051

## Description

Parent: **080**. Prove the 14-beta path in test-app / integration / e2e as this repo actually runs them. Docs say TimeWarp.Mediator 14-beta, not martinothamar and not 13.0.0 reflection.

**049, 050, 051** (run tests / docs / release as martinothamar destination) are **not** the goal. Retarget or archive them so nobody ships a martinothamar State.

## Depends on

- 080-002

## Requirements

- Existing test suites green on generated + scoped senders (or a listed, justified skip)
- README / claude.md / package tags no longer advertise martinothamar as current
- 049–051: archive or rewrite to “soak 14-beta / later State release” — do not execute them as written
- Record gaps found in TimeWarp.Mediator (file issues or mediator follow-ups). This soak is why we are on **beta**.

## Checklist

- [x] Tests
- [x] Docs
- [x] 049–051 disposition
- [x] Feedback list for mediator (if any)

## Out of scope

- TimeWarp.State NuGet release as “done”
- TimeWarp.Mediator 14.0.0 stable

## Session

- Created: 154892 (2026-09-01)
- 2026-09-04: implementer (Grok 4.6, ganda task work) soaked 14-beta in Fixie + Playwright, retargeted docs, archived 049–051, filed mediator issues 63–65. `ganda kanban move 080-003 in-progress` refused (080-002 not on origin-home); kitchen moved with `git mv` on the task branch.

## Results

Proved the TimeWarp.Mediator **14.0.0-beta.1** generated + scoped-sender path in the suites this repo actually runs. Docs advertise that stack. 049–051 are archived so nobody ships a martinothamar State.

### Tests

Fixie (same counts as 080-002):

| Suite | Result |
|-------|--------|
| `timewarp-state-analyzer-tests` | 10 passed |
| `timewarp-state-tests` | 16 passed, 1 skipped |
| `timewarp-state-plus-tests` | 11 passed, 1 skipped |
| `client-integration-tests` | 42 passed, 1 skipped |
| `test-app-architecture-tests` | 7 passed, 1 skipped |

Playwright (`UseHttp=true dotnet run --file ./scripts/e2e.cs`): **10 passed, 3 skipped, 0 failed**. CI `continue-on-error` on `Run E2E tests` is removed.

Soak-blocking E2E work (pre-existing on origin/dev; not a 14-beta handler-registration miss):

- Static pages: `RenderModeDisplay` always emits `[data-qa=configured-render-mode]`, `"None"` when `AssignedRenderMode` is null.
- InteractiveAuto: once interactive, Blazor’s `AssignedRenderMode.GetType().Name` is `InteractiveServerRenderMode` / `InteractiveWebAssemblyRenderMode`. Tests map `RendererInfo.Name` accordingly. `RendererInfo.Name` for WASM is `"WebAssembly"`.
- `RouteState` implements `ICloneable` so StateTransactionBehavior does not AnyClone `Stack<RouteInfo>` (that threw `CloneException` / `ILCacheKey` and crashed the circuit, so clicks never incremented). `CancellationTokenSource` on `State<T>` is `[IgnoreDataMember]`.
- Untracked `tests/test-app/test-app-client/generated/**` (080-001 M6).

**Justified skips (3):**

- `ExampleTest.HasTitle` / `GetStartedLink` — Playwright sample against playwright.dev, not the SUT.
- `PersistenceTest.TestPersistence` — `[PersistentState]` reload round-trip is **065** (serializer/key). Counter 3→8→13, EventStream Start/Completed, ThrowException rollback, CacheableWeather fetch, ChangeRoute/ResetStore/GoBack all pass on generated `ClientPipeline`.

### Docs

- `readme.md`, `documentation/overview.md`, `documentation/partials/summary.md`, `claude.md`: TimeWarp.Mediator 14-beta, `AddGeneratedMediator<ClientPipeline>()`, named pipelines; not MediatR / not `AddMediator()`.
- Sample 00 / 02 tutorials: `TimeWarp.Mediator.Generators`, `[assembly: MediatorAssembly]` / `MediatorScope`, `StateActionHandler<T>`.
- PackageTags: dropped inherited `MediatR` from `source/Directory.Build.props`. State/Plus already tagged `TimeWarp.Mediator`.
- Badges: `dotnet-10.0`.

### 049–051

Rewritten as “do not execute as martinothamar destination” and **archived** (`kanban/archived/049|050|051-*.md`). Tests/docs soak is this id; a later State NuGet release is a new task after 080.

### Mediator feedback (filed)

- https://github.com/TimeWarpEngineering/timewarp-mediator/issues/63 — `TimeWarpMediatorNamespace` does not rename `GeneratedMediatorServiceCollectionExtensions` (CS0121; `AddServerPipelineMediator` workaround remains).
- https://github.com/TimeWarpEngineering/timewarp-mediator/issues/64 — no `GenerateTypesAsInternal`; host actions must be public (CS0051).
- https://github.com/TimeWarpEngineering/timewarp-mediator/issues/65 — CS1591 flood from generated public types under `GenerateDocumentationFile`.

### How to validate

**Smoke**

```bash
mkdir -p artifacts/packages
rm -rf ~/.nuget/packages/timewarp.state/12.0.0-beta.3 ~/.nuget/packages/timewarp.state.plus/12.0.0-beta.3
dotnet tool restore
dotnet run --file ./scripts/test.cs
UseHttp=true dotnet run --file ./scripts/e2e.cs
```

Docs / tags / archive:

```bash
rg -n 'martinothamar' --glob '!kanban/**' --glob '!**/generated/**' readme.md claude.md documentation source samples
rg -n 'AddGeneratedMediator<ClientPipeline>' --glob '*.md' readme.md documentation samples claude.md
rg -n 'MediatR' source/Directory.Build.props source/timewarp-state/timewarp-state.csproj source/timewarp-state-plus/timewarp-state-plus.csproj
test -f kanban/archived/049-migrate-mediator-run-tests-and-fix-issues.md
test -f kanban/archived/050-migrate-mediator-update-documentation.md
test -f kanban/archived/051-migrate-mediator-bump-version-and-release.md
test ! -e kanban/to-do/049-migrate-mediator-run-tests-and-fix-issues.md
rg -n 'continue-on-error' .github/workflows/workflow.yml
```

**Expect**

- `scripts/test.cs` exits 0: analyzer 10 passed; state 16/1 skip; plus 11/1 skip; integration 42/1 skip; architecture 7/1 skip; 0 failed.
- `e2e.cs` prints `Passed!  - Failed: 0, Passed: 10, Skipped: 3, Total: 13` and exits 0. Skips are HasTitle, GetStartedLink, TestPersistence only.
- No `martinothamar` in product docs (`readme.md` / `claude.md` / `documentation/` / `source/` / `samples/`). `AddGeneratedMediator<ClientPipeline>` appears in overview/readme/sample 00 tutorials. PackageTags have `TimeWarp.Mediator` and not `MediatR`.
- 049–051 exist only under `kanban/archived/`. Workflow has no `continue-on-error` on `Run E2E tests`.
- Mediator issues 63, 64, 65 are open on TimeWarpEngineering/timewarp-mediator.

**Automated gate:** `dotnet run --file ./scripts/test.cs` and `UseHttp=true dotnet run --file ./scripts/e2e.cs`.

**Depends on:** Playwright Chromium already on the machine (script `install --with-deps` is best-effort; this host’s OS is not an official Playwright target). Local NuGet feed folder `artifacts/packages`.

**Not in scope:** TimeWarp.State NuGet release; TimeWarp.Mediator 14.0.0 stable; PersistentState reload (065); swapping `AddServerPipelineMediator` for namespaced `AddGeneratedMediator<ServerPipeline>()` (mediator #63).
