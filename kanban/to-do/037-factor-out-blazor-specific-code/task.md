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

TimeWarp.State 13.0.0-beta.1 has no reference to Microsoft.AspNetCore.Components or Microsoft.JSInterop. Components, JavaScript interop, Redux DevTools, render subscriptions, and wwwroot live in TimeWarp.State.Blazor. Moved types keep their namespaces. Blazor hosts add a PackageReference to TimeWarp.State.Blazor and call `AddTimeWarpStateBlazor()` after `AddTimeWarpState()`. Console hosts call `AddTimeWarpState()` only. TimeWarp.State.Plus keeps its package id and references TimeWarp.State.Blazor. Static web assets stay under `_content/TimeWarp.State/`, including `/_content/TimeWarp.State/js/timewarp.state.lib.module.js`.

With TimeWarp.State.Blazor referenced, pipeline order is ReduxDevTools 100, StateInitialization 200, StateTransaction 300, RenderSubscriptions 400. The split does not change those handlers.

Nupkg size: TimeWarp.State 12.0.0-beta.10 was 130,688 bytes and depended on Microsoft.AspNetCore.Components.Web. TimeWarp.State 13.0.0-beta.1 is 84,873 bytes. Its dependencies are JetBrains.Annotations, Microsoft.CodeAnalysis.CSharp, and TimeWarp.Mediator.Contracts. The package contains no wwwroot. TimeWarp.State.Blazor 13.0.0-beta.1 is 61,760 bytes and carries Components.Web plus the static assets.

`dev build` exits 0. `dev test` exits 0: analyzer 36 passed, source generator 15 passed, state 80 passed and 1 skipped (includes `CoreAssembly_Should_.NotReferenceBlazorOrJavaScriptInterop` and `BlazorPackage_ReferencesComponents`), plus 32 passed and 1 skipped, telemetry 13 passed, client integration 65 passed and 1 skipped, test-app architecture 7 passed and 1 skipped. `dev pack` writes the five packable packages at 13.0.0-beta.1. `dev verify-samples` exits 0. The console sample prints `Count=5` and exits 0. `ganda repo audit` exits 0 with one pre-existing advisory (kebab path `tests/test-app/test-app-client/wwwroot/Test.App.Client.lib.module.js`). Pre-existing build warnings remain (TW0007, RS0030, BL0010, BL0016, NU1510, ASPDEPR011, IL2026, IL2111).

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
- `./bin/dev pack && ./bin/dev verify-samples`
- `dotnet run --project samples/07-console/sample-07-console/sample-07-console.csproj -c Release`
- `ganda repo audit`

Expect:

- `dev build` exits 0.
- `dev test` exits 0 with the counts in Results.
- `dev verify-samples` exits 0, including `samples/07-console`.
- The console process prints `Count=5` and exits 0.
- `ganda repo audit` exits 0.

## Session

- Implementation: grok task-work implementer (2026-10-10)
- Review: claude-opus-5-5 review oracle (2026-10-10), general reviewer subagent a954005f421aa90d7