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

- [ ] Review architecture and project layout
- [ ] Review public API and design
- [ ] Review code quality
- [ ] Review correctness and concurrency
- [ ] Review AOT and trimming
- [ ] Review the source-generated cloner and the TimeWarp.State.Blazor split
- [ ] Review tests and coverage gaps
- [ ] Review docs and samples
- [ ] Review CI and packaging
- [ ] Write overall assessment and severity-grouped findings to `kitchen/review-findings.md`

## Session

- Created: 3535798 (2026-10-10)

## Notes

- Requested by Steven on a voice call, 2026-10-10. Run through the full ganda walk with profile `implementer-claude-fable` (model `claude-fable-5-1`).
