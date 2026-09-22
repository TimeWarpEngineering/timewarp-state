# Task 022: Create TimeWarpStateComponent Samples

## Description

Create comprehensive samples demonstrating the advanced rendering control features of TimeWarpStateComponent, showcasing its capabilities for fine-grained control over component re-rendering.

## Requirements

- Create sample applications that demonstrate:
  - RenderMode usage and configuration
  - RenderReasons tracking and optimization
  - Performance benefits of controlled re-rendering
  - Best practices for component state management
  - Integration with existing state management patterns

## Checklist

### Design
- [x] Plan sample scenarios that highlight key features
- [x] Design clear and educational component examples
- [x] Consider performance demonstration scenarios

### Implementation
- [x] Create basic sample demonstrating core features
- [x] Implement advanced rendering control examples
- [x] Add performance comparison demonstrations
- [x] Include comprehensive comments explaining the concepts
- [x] Add proper error handling and edge cases

### Documentation
- [x] Document each sample with clear explanations
- [x] Include usage guidelines and best practices
- [x] Add performance optimization tips
- [x] Create README for each sample
- [x] Update main documentation to reference new samples

### Review
- [x] Review code for clarity and educational value
- [x] Verify all samples work as intended
- [x] Test across different scenarios
- [x] Ensure documentation is clear and complete

## Notes

- Focus on real-world use cases that demonstrate practical benefits
- Include performance metrics where applicable
- Consider progressive complexity in examples
- Ensure samples are well-documented for learning purposes
- One standalone WASM sample (`samples/06-render-control`) covers modes clearly; no farm of apps.

## Implementation Notes

- Reference existing samples for consistent structure
- Follow TimeWarp.State coding conventions
- Include comments explaining key concepts and decisions

## Current API (do not invent)

- Match `samples/05-persistence` layout: numbered folder, WASM host, generated `AddGeneratedMediator`, kebab files.
- Features: `RenderMode`, `RenderReason` / `ShouldRender`, subscriptions, `HandleUnregisteredParameter` if useful. `CaptureRenderCaller` only if showing caller diagnostics (069, default off).
- Test-app already has should-render pages — this is a **standalone sample**, not a copy of the whole test-app.
- One sample is enough if it covers the modes clearly. Do not spawn a farm of apps.

## Session

- 2026-09-22: cockpit — after 029 merge. Dispatch implementer-grok.
- 2026-09-22: implementer-cursor (ganda task-work headless) — finished sample 06, docs cross-links, smoke validation.
- 2026-09-22: implementer-cursor (ganda task-work) — re-verified Release build of sample-06-wasm (0 warnings); product work complete; no further sample apps.
- 2026-09-22: implementer-cursor (ganda task-work review oracle) — `tw-implementation-review` effort 1; disposition clean.

## Results

Standalone Blazor WASM sample `samples/06-render-control` teaches `TimeWarpStateComponent` render decisions: host `RendererInfo` / `IsPreRendering` / `AssignedRenderMode`, `RenderReason` categories (Event, ParameterChanged, Subscription, Forced), primitive vs complex parameter checks, `HandleUnregisteredParameter`, `RegisterRenderTrigger` vs every-change subscription, and diagnostic `CaptureRenderCaller` (on in this host only).

### Review disposition

- **Outcome:** `clean`
- **Rounds:** 1 · **Effort:** 1 · **Roster:** general
- **Final counts:** bug/suggestion/nit all 0 open, 0 fixed, 0 wontfix
- **Artifacts:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`
- Re-verified `dotnet build samples/06-render-control/wasm/sample-06-wasm/sample-06-wasm.csproj -c Release` (0 warnings). No product fixes from review.

### Files changed

- `samples/06-render-control/` — overview, readme, WASM host (`sample-06-wasm`) with ActivityState, tracing/guarded bases, parameter and subscription cards, Home page
- `samples/overview.md` — entry for sample 06
- `documentation/topics/render-control.md` + `toc.yml` — topic page
- `documentation/features/features.md`, `readme.md`, `source/timewarp-state/components/timewarp-state-component.md` — cross-links
- `timewarp-state.slnx` — Samples/06-RenderControl project
- Kitchen folderized from flat `022-….md` → `022-…/task.md`
- `review/` — framework, round-1 general+merged, disposition clean

### Key decisions

- One WASM host (not Server/Auto farm); matches brief and sample 05 layout
- `CaptureRenderCaller = true` only so the caller column is educational; docs warn to leave it off in production
- `UseReduxDevTools()` without `<ReduxDevTools />` so CommitHandler DI validates without teaching the extension
- Performance lesson is live counters: Tick advances Beat (count-only card skips); IncrementCount paints both cards; store still clones on every action

### Test outcomes

- `dotnet restore` + `dotnet build` of `sample-06-wasm` — pass (0 warnings)
- Browser smoke at `http://localhost:5296` — pass:
  - Host: WebAssembly / interactive True / prerender False / InteractiveWebAssemblyRenderMode
  - Tick local: reference filter paints; open/guarded/text filter skip
  - Swap label: open → Event; guarded → ParameterChanged naming Label
  - New wrapper same text: reference paints; text filter skips
  - Change filter text: text filter → ParameterChanged naming Filter
  - Tick once: every-change +1; count-only stays; Beat diverges
  - Increment count: both cards paint; count-only Beat catches up
  - Schedule forced: page trace includes Forced via ComponentBase.StateHasChanged
  - Admit unknown: detail Unregistered parameter + expected Blazor property refusal

### How to validate

**Depends on:** pack (or existing) `TimeWarp.State` `12.0.0-beta.4` in `artifacts/packages`. If NuGet already cached an older build of that version, delete `~/.nuget/packages/timewarp.state/12.0.0-beta.4` before restore.

**Smoke**

```bash
dotnet build source/timewarp-state/timewarp-state.csproj -c Release
dotnet run --file tools/dev-cli/dev.cs -- pack   # or ensure TimeWarp.State.*.nupkg exists under artifacts/packages
dotnet restore samples/06-render-control/wasm/sample-06-wasm/sample-06-wasm.csproj
dotnet run --project samples/06-render-control/wasm/sample-06-wasm/sample-06-wasm.csproj --launch-profile http
```

Open `http://localhost:5296`, then:

1. Confirm Render mode strip: Name `WebAssembly`, IsInteractive `True`, IsPreRendering `False`, AssignedRenderMode `InteractiveWebAssemblyRenderMode`
2. **Tick local field** — reference-filter renders increase; open/guarded/text-filter do not
3. **Swap label** — open child reason `Event`; guarded child `ParameterChanged` / `Parameter 'Label' changed`
4. **New wrapper, same text** — reference filter paints; text filter does not
5. **Tick once** — every-change Beat and renders increase; count-only Beat and renders stay
6. **Increment count** — both subscription cards paint once
7. **Schedule forced render** — page trace contains a `Forced` line
8. **Admit unknown parameter** — admitted name `Unknown`; expected refusal message; detail mentions Unregistered parameter

**Expect**

- Build exit 0 for sample-06-wasm
- Page title `Render control sample`
- Steps 2–8 match the counter/reason behavior above
- No unhandled Blazor error UI after admit (refusal is caught on the guarded card)

**Automated gate**

```bash
dotnet build samples/06-render-control/wasm/sample-06-wasm/sample-06-wasm.csproj -c Release
```

(or `dotnet run --file tools/dev-cli/dev.cs -- verify-samples` after a full pack so every sample restores)

**Not in scope**

- Server/Auto hosts for this lesson
- PR open (later task-work nodes)
