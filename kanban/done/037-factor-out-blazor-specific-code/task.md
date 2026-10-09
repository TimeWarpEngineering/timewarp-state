# Task 037: Factor Out Blazor-Specific Code for Console App Support

## Description

- Refactor TimeWarp.State to separate core state management functionality from Blazor-specific features
- Enable TimeWarp.State to be used in console applications and other non-Blazor contexts
- Create a new TimeWarp.State.Blazor package for all Blazor-specific functionality

## Requirements

- TimeWarp.State package should have no Blazor dependencies
- All existing functionality must be preserved through package composition
- Maintain backward compatibility for existing users
- Console applications should be able to use core state management features

## Checklist

### Design
- [x] Identify all Blazor-specific code in TimeWarp.State
- [x] Design package structure and dependencies
- [x] Plan migration path for existing users
- [x] Create architecture tests to ensure no Blazor dependencies in core

### Implementation
- [x] Create TimeWarp.State.Blazor project structure
- [x] Move Blazor components to TimeWarp.State.Blazor
  - [x] TimeWarpStateComponent and related components
  - [x] TimeWarpStateDevComponent (Redux DevTools)
  - [x] RenderModeDisplay component
  - [x] ReduxDevTools.razor
  - [x] TimeWarpJavaScriptInterop.razor
- [x] Move JavaScript interop functionality
  - [x] JsonRequestHandler
  - [x] ReduxDevToolsInterop
  - [x] All wwwroot assets (JS/TS files)
- [x] Move render subscription features
  - [x] RenderSubscriptionContext
  - [x] RenderSubscriptionsPostProcessor
- [x] Remove Microsoft.AspNetCore.Components.Web dependency from TimeWarp.State
- [x] Update service registration extensions
  - [x] Create separate registration for Blazor features
  - [x] Maintain core registration in TimeWarp.State
- [x] Consider renaming TimeWarp.State.Plus to TimeWarp.State.Blazor.Plus (package id stays TimeWarp.State.Plus)
- [x] Create console app sample to validate functionality
- [x] Update all existing samples to reference appropriate packages

### Documentation
- [x] Update package descriptions and README files
- [x] Document migration guide for existing users
- [x] Create usage examples for console applications
- [x] Update ai-context.md with new package structure

### Review
- [x] Record the migration for existing Blazor hosts (same namespaces; add TimeWarp.State.Blazor and AddTimeWarpStateBlazor)
- [x] Ensure clean separation of concerns
- [x] Test in both Blazor and console contexts
- [x] Performance impact assessment
- [x] Review package size reduction for console apps

## Notes

- 037 is this live to-do (created 2025-08-22). A cancelled “mediator migration test plan” kitchen reused 037; that file is archived **084** (cleanup **085**).
- TimeWarp.State will contain: Store, State, Actions, Handlers, Pipeline behaviors (minus Redux DevTools), Core attributes
- TimeWarp.State.Blazor will contain: All components, JS interop, Redux DevTools, Render subscriptions
- TimeWarp.State.Plus keeps its package id. Renaming the NuGet id would break existing PackageReferences. The project references TimeWarp.State.Blazor.
- This separation will enable broader adoption of TimeWarp.State in different application types

## Implementation Notes

- Start by creating the new project structure
- Move files incrementally, testing at each step
- Ensure analyzers and source generators work with new structure

## Results

TimeWarp.State 12.0.0-beta.10 has no reference to Microsoft.AspNetCore.Components or Microsoft.JSInterop. Components, JavaScript interop, Redux DevTools, render subscriptions, and wwwroot live in TimeWarp.State.Blazor. Moved types keep their namespaces. Blazor hosts add a PackageReference to TimeWarp.State.Blazor and call `AddTimeWarpStateBlazor()` after `AddTimeWarpState()`. Console hosts call `AddTimeWarpState()` only. TimeWarp.State.Plus keeps its package id and references TimeWarp.State.Blazor. Static web assets stay under `/_content/TimeWarp.State/`, including `/_content/TimeWarp.State/js/timewarp-state.js`. `StaticWebAssetBasePath` is `_content/TimeWarp.State` because the SDK uses that property as the whole path. Blazor loads the initializer `/_content/TimeWarp.State/js/TimeWarp.State.Blazor.lib.module.js` (`{PackageId}.lib.module.js`).

With TimeWarp.State.Blazor referenced, pipeline order is ReduxDevTools 100, StateInitialization 200, StateTransaction 300, RenderSubscriptions 400. The split does not change those handlers.

Before the split, TimeWarp.State was 130,688 bytes and depended on Microsoft.AspNetCore.Components.Web. TimeWarp.State 12.0.0-beta.10 is 84,887 bytes. Its dependencies are JetBrains.Annotations 2026.2.0, Microsoft.CodeAnalysis.CSharp 4.14.0, and TimeWarp.Mediator.Contracts 14.0.0-beta.4. The package contains no wwwroot. TimeWarp.State.Blazor 12.0.0-beta.10 is 61,941 bytes and depends on TimeWarp.State, Microsoft.AspNetCore.Components.Web 11.0.0-rc.1.26425.128, and Microsoft.CodeAnalysis.CSharp. Release notes and the migration guide for this version also cover `TimeWarpCacheableState<TState>` deriving from `State<TState>`, `InvalidCloneException` taking a cause, removal of `FeatureFlagState` from TimeWarp.State.Plus, the NuGet pin updates (including TimeWarp.Amuru 2.0.0-beta.2), the Purpose/Design backfill, and TWA0004 via TimeWarp.Architecture.Analyzers.

`dev build` exits 0. `dev test` exits 0: analyzer 36 passed, source generator 15 passed, state 84 passed and 1 skipped (includes `CoreAssembly_Should_.NotReferenceBlazorOrJavaScriptInterop` and `BlazorPackage_ReferencesComponents`), plus 32 passed and 1 skipped, telemetry 13 passed, client integration 65 passed and 1 skipped, test-app architecture 7 passed and 1 skipped. `dev e2e` exits 0: Failed 0, Passed 11, Skipped 3, Total 14. `dev pack` writes the five packable packages at 12.0.0-beta.10. `dev verify-samples` exits 0 after the global NuGet cache copies of TimeWarp.State, TimeWarp.State.Plus, and TimeWarp.State.Telemetry 12.0.0-beta.10 were removed; NuGet does not replace a folder it already extracted for that version, and the cached TimeWarp.State still contained the Blazor types. The console sample prints `Count=5` and exits 0. `ganda repo audit` exits 0 with an advisory on two kebab paths: `tests/test-app/test-app-client/wwwroot/Test.App.Client.lib.module.js` and `source/timewarp-state-blazor/wwwroot/typescript/TimeWarp.State.Blazor.lib.module.ts` (Blazor's initializer filter is `{PackageId}.lib.module.js`). Pre-existing build warnings remain (TW0007, RS0030, BL0010, BL0016, NU1510, ASPDEPR011, IL2026, IL2111).

### Implementation review

Review effort was 3 with a general reviewer, over 2 rounds. Final counts: bugs 0. Suggestions: 1 fixed, 1 wontfix. Nits: 2 fixed. Open: 0. Disposition: **accepted-exceptions**.

- Fixed:
  - M1: `UseReduxDevTools` and `AddJavaScriptDispatch` now register `JsonRequestHandler` themselves.
  - M3: the sample overviews now show `AddTimeWarpStateBlazor()`.
  - M4: `tests/timewarp-state-tests/architecture/add-timewarp-state-blazor-tests.cs` covers registration, idempotency and call order. state tests: 84 passed, 1 skipped.
- Wontfix: M2. There is no custom fail-fast when a Blazor host omits `AddTimeWarpStateBlazor()`. DI's error names `RenderSubscriptionContext`, and the call is documented.
- Artifacts: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

Smoke:

- `./bin/dev build`
- `./bin/dev test`
- `./bin/dev e2e`
- `./bin/dev pack && ./bin/dev verify-samples`
- `dotnet run --project samples/07-console/sample-07-console/sample-07-console.csproj -c Release`
- `ganda repo audit`

Expect:

- `dev build` exits 0.
- `dev test` exits 0 with the counts in Results.
- `dev e2e` exits 0: Failed 0, Passed 11, Skipped 3, Total 14. ThrowExceptionPage reaches render mode `Server`.
- `dev verify-samples` exits 0, including `samples/07-console`. If the machine already extracted TimeWarp.State 12.0.0-beta.10 into the global NuGet cache from before this split, delete that version folder first.
- The console process prints `Count=5` and exits 0.
- `ganda repo audit` exits 0.

## Session

- Implementation: grok task-work implementer (2026-10-10)
- Review: claude-opus-5-5 review oracle (2026-10-10), general reviewer subagent a954005f421aa90d7
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-09T17:33:00Z
- Resume: grok task-work implementer (2026-10-10). Version is 12.0.0-beta.10. Static assets publish at `/_content/TimeWarp.State/` and the initializer is `TimeWarp.State.Blazor.lib.module.js`. `dev e2e` Passed 11, Skipped 3, Failed 0.
## Notes

### 2026-10-10 — Steven's decision and CI failure on PR #625 (resume the walk)

- **Version:** Steven accepts breaking changes in the 12.0 betas. This split ships in **12.0.0-beta.10**, not
  13.0.0-beta.1. Set `<Version>` in `source/Directory.Build.props` and `TimeWarpStateVersion` in
  `msbuild/repository.props` back to `12.0.0-beta.10`, and replace every 13.0.0-beta.1 reference (docs, release
  notes, migration guide, PR text, package size notes).
- **CI failure (must be fixed, all e2e must pass):** run
  https://github.com/TimeWarpEngineering/timewarp-state/actions/runs/37967174955 —
  `test-app-end-to-end-tests` failed 9 of 14 (2 passed, 3 skipped). Example:
  `ThrowExceptionPageTests.ThrowExceptionTests.TestThrowException`: Playwright locator
  `[data-qa='current-render-mode']` expected `Server` but was `Static`. The test app no longer starts interactive
  server rendering after the Blazor split. Likely causes: the test app (server and/or client) is missing the
  `TimeWarp.State.Blazor` reference, the `AddTimeWarpStateBlazor()` call, or a render-mode / static web asset
  registration (e.g. `_content/TimeWarp.State.Blazor/...` scripts, `MapStaticAssets`, `AddInteractiveServerComponents`
  / `AddInteractiveServerRenderMode`). Reproduce locally with the e2e suite (`dev e2e` or the CI pipeline command)
  and fix until the full end-to-end run passes.
- **Release notes and migration guide for 12.0.0-beta.10** (`documentation/release-notes/release12.0.0-beta.10.md`,
  `documentation/migrations/migration12.0.0-beta.10.md`) must cover, together:
  - 037: the new `TimeWarp.State.Blazor` package, what moved into it (components, JS interop, Redux DevTools,
    render subscriptions, wwwroot; namespaces unchanged), and that Blazor hosts must reference it and call
    `AddTimeWarpStateBlazor()` after `AddTimeWarpState()`; console hosts use TimeWarp.State alone.
  - 100's breaking changes (already in the notes): `TimeWarpCacheableState<TState>` now derives from
    `State<TState>`, `InvalidCloneException` takes a cause, and the **removal of the `FeatureFlagState` placeholder
    from TimeWarp.State.Plus**.
  - 101: NuGet package pins moved to current latest, including **TimeWarp.Amuru 2.0.0-beta.2**.
  - 099: Purpose/Design region backfill across all C# files.
  - 622 (task 099-001): TWA0004 Purpose-region analyzer enabled via TimeWarp.Architecture.Analyzers.
