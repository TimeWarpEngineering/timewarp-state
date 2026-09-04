# Round 1 — merged findings
**Date:** 2026-09-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

None.

## Duplicates / conflicts

- None. Single general reviewer; zero findings.

## Merge-pass notes

Independent re-check of the product delta (`f6df8332` + `e5624036` vs `2c68fa7c`, excluding `kanban/` and `generated/**`):

- No `martinothamar` under `readme.md` / `claude.md` / `documentation/` / `source/` / `samples/`.
- `AddGeneratedMediator<ClientPipeline>` is in overview, readme, summary, and sample 00/02 tutorials.
- PackageTags dropped inherited `MediatR` from `source/Directory.Build.props`; State/Plus already tagged `TimeWarp.Mediator`.
- 049–051 exist only under `kanban/archived/` and are marked do-not-execute.
- `.github/workflows/workflow.yml` has no `continue-on-error` on `Run E2E tests`.
- `RouteState` implements `ICloneable`; `StateTransactionBehavior` prefers that path over AnyClone. Clone allocates a new Guid (transaction invariant) and copies the route stack in original order. `clone.Sender.ShouldBe(RouteState.Sender)` matches the ICloneable constructor (behavior also reassigns Sender after clone).
- `[IgnoreDataMember]` on `State<T>.CancellationTokenSource` matches the existing Guid ignore for AnyClone.
- RenderModeDisplay always emits `[data-qa=configured-render-mode]` (`"None"` when unassigned). `TimeWarpStateDevComponent` passes `"None"` the same way. `RenderModes.Wasm` is `"WebAssembly"`; `ConfiguredRenderModes.ForCurrentRenderMode` maps InteractiveAuto after interactivity.
- Justified skips: `ExampleTest` (playwright.dev sample), `PersistenceTest.TestPersistence` (task 065).
- Mediator issues 63 / 64 / 65 exist on TimeWarpEngineering/timewarp-mediator (titles match Results).
