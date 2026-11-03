# Review framework

## Budget (by-diff)

- Lines changed: 270
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Framework — task 134

**Date:** 2026-11-04
**Host task:** kanban/in-progress/134-fix-state-clone-planner-correctness-bugs-jsonignore-non-nameable-types-missing-clone-comparers-cancellation/
**Diff scope:** branch `task/134-fix-state-clone-planner-correctness-bugs-jsonignor` (HEAD b154d5f3) vs `master` merge base. Product files: `source/timewarp-state-source-generator/state-clone-planner.cs`, `state-clone-source-generator.cs`, `timewarp-state-source-generator.csproj`, `tests/timewarp-state-source-generator-tests/state-clone-correctness-tests.cs`, `state-clone-generator-test-driver.cs`, `global-usings.cs`. Audit-driven `.gitignore` / `Directory.Packages.props` changes are policy and out of review scope.
**Plan / brief:** task.md `## Goal (implementer restatement)` and `## Requirements`: JsonIgnore Condition, nameability (file-local, other-assembly internal, type arguments) for clone and ICloneable trim roots, `IsPubliclyVisible` type arguments, Share-classified `[GenerateClone]` struct, comparer for collection subclasses, CancellationToken threading, reference-assembly record EqualityContract.
**Effort:** 3
**Reviewer roster:** general
**Session IDs:** review oracle claude-opus-5-5 (Claude Code, headless `ganda task work`), 2026-11-04

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
