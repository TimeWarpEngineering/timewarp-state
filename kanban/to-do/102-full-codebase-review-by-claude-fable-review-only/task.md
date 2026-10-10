# Full codebase review by Claude Fable (review only)

## Description

Steven wants a full, whole-repo review of timewarp-state by Claude Fable (Claude Code on the `claude-fable-5-1` model, ganda profile `implementer-claude-fable`). This is a **review only** task: no code changes. The only deliverable is the findings file in this kitchen plus the normal kanban moves. Follow-up work is filed later by Steven's decision, not by this task.

Write findings into `kitchen/review-findings.md` (beside this task.md) as the review goes, so partial results survive an interrupted run.

### Scope

Review the whole repository, covering:

- Architecture: project layout, layering, dependencies between TimeWarp.State, TimeWarp.State.Plus, TimeWarp.State.Blazor, the analyzer and the source generator
- Public API and design: naming, consistency, extensibility, breaking-change risk
- Code quality: duplication, dead code, complexity, conventions from AGENTS.md
- Correctness and concurrency: state mutation, thread safety, async/await, subscription and render-trigger lifetimes, memory leaks
- AOT and trimming: remaining reflection, `IL2026`/`IL2111`-class warnings, trim annotations
- The source-generated cloner (task 097) and the TimeWarp.State.Blazor split (task 037, 12.0.0-beta.10): correctness, edge cases, gaps, migration story
- Tests and coverage gaps: unit, analyzer, source generator, integration, end-to-end
- Docs and samples: accuracy against current APIs, stale references, sample health
- CI and packaging: workflows, build scripts, NuGet metadata, versioning, package contents

### Output format (`kitchen/review-findings.md`)

1. **Overall assessment**: a few paragraphs on the state of the codebase.
2. **Findings grouped by severity**: `## Critical`, `## High`, `## Medium`, `## Low`, `## Nit`. Each finding has a short title, the file path(s) (with line numbers where useful), what is wrong and why it matters, and a concrete suggestion.
3. Optionally, a short list of strengths worth keeping.

## Requirements

- Do not modify any file outside this task's kitchen folder. No product code, test, doc, sample, build or CI changes.
- Do not file follow-up tasks or create other kanban items.
- Findings must cite real file paths in this repo.

## Checklist

- [x] Review architecture and project layout
- [x] Review public API and design
- [x] Review code quality
- [x] Review correctness and concurrency
- [x] Review AOT and trimming
- [x] Review the source-generated cloner and the TimeWarp.State.Blazor split
- [x] Review tests and coverage gaps
- [x] Review docs and samples
- [x] Review CI and packaging
- [x] Write overall assessment and severity-grouped findings to `kitchen/review-findings.md`

## Results

Review complete. Findings are in `review-findings.md` beside this file: overall assessment, 0 Critical, 4 High, 15 Medium, 11 Low, a Nit list, and strengths worth keeping. No file outside this kitchen was changed and no follow-up tasks were filed.

Headline findings:

- H1: `Directory.Build.props` references `Microsoft.CodeAnalysis.CSharp` without `PrivateAssets`, so the published `TimeWarp.State` and `TimeWarp.State.Blazor` packages depend on the Roslyn compiler at runtime (verified in the local NuGet cache nuspec files for 12.0.0-beta.10 and beta.11). `JetBrains.Annotations` leaks the same way.
- H2: `StateTransactionBehavior` rolls back unconditionally, so when two actions on one state overlap, a failure in one discards the other's committed changes.
- H3: the generated cloner has no share-by-reference opt-in; the documented `[IgnoreDataMember]` workaround for injected services produces a live clone whose service field is null.
- H4: `TimeWarpStateComponent` compares collection parameters by count, which suppresses renders in components that override a check hook.
- Medium: Redux DevTools time travel is unreachable from the shipped JavaScript yet keeps the core package's only trim-unsafe reflection; `IsAotCompatible` is missing on Blazor and Plus; cancellation and handler failures are swallowed by the transaction behavior; `Store.Reset` is partial; `IStore.GetSemaphore` and `StateInitializationTasks` leak implementation; `PushRouteInfo` uses JavaScript `eval`; `ActiveActionBehavior` completes tracking with a possibly cancelled token; `TimerState` publishes from timer threads via `async void`; two `PersistentStateMethod` values are no-ops; the "test" assembly-name sniff still guards public members; analyzer ids are inconsistent; docs and readmes have drifted.

Evidence gathered during the review: Release build of Plus, Telemetry, Policies and the test app server (0 errors, 0 IL warnings because only two packages run the trim analyzer; 62 TW0007, 42 nullable, 24 NU1510, 12 RS0030, 10 BL0016, 6 BL0010, 2 ASPDEPR011); `ganda repo audit` passes with one pre-existing kebab-path advisory.

### How to validate

Smoke:

- `cat kanban/to-do/102-full-codebase-review-by-claude-fable-review-only/review-findings.md`
- `git diff --stat origin/master..HEAD -- . ':!kanban'`
- `grep -E "<dependency " ~/.nuget/packages/timewarp.state/12.0.0-beta.11/timewarp.state.nuspec`
- `ganda repo audit`

Expect:

- The findings file has an overall assessment, `## Critical` through `## Nit` sections, and every finding cites a path under `source/`, `tests/`, `documentation/`, `samples/`, `tools/`, `.github/` or a repo-root file.
- The diff outside `kanban/` is empty: the task changed no product, test, doc, sample, build or CI file.
- The nuspec lists `Microsoft.CodeAnalysis.CSharp` and `JetBrains.Annotations` as dependencies, confirming H1.
- `ganda repo audit` exits 0 (one advisory on the five `*.lib.module.*` paths, pre-existing).

## Session

- Created: 3535798 (2026-10-10)
- Review: Claude Fable (claude-fable-5-1) via `ganda task work`, profile `implementer-claude-fable`, 2026-10-10. Wrote `review-findings.md`; no product changes.

## Notes

- Requested by Steven on a voice call, 2026-10-10. Run through the full ganda walk with profile `implementer-claude-fable` (model `claude-fable-5-1`).
