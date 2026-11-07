# Review framework — task 151

**Date:** 2026-11-07
**Host task:** kanban/in-progress/151-update-microsoftcodeanalysiscsharp-and-workspaces-from-4140-to-590-raise-analyzer-roslyn-floor/
**Diff scope:** branch `task/151-update-microsoftcodeanalysiscsharp-and-workspaces` vs merge-base `b406f3a8` (master). Product commit `619b3ef3`. 12 files, +202/−84, including the kanban task move and Results.
**Plan / brief:** Bump `Microsoft.CodeAnalysis.CSharp` and `.Workspaces` from 4.14.0 to 5.9.0, keep them equal and `PrivateAssets="all"`, record the SDK floor from the SDK's bundled Roslyn, and state that floor in the docs and the next release notes. The goal restatement narrows the 10.0.403 `verify-samples` run; see `## Learnings`.
**Effort:** 2
**Reviewer roster:** general
**Session IDs:** review oracle, ganda task work, print mode, conversation `93a49854-ae59-405c-a6bf-85a3ef31f95b` (2026-11-07). Implementer session `b856b077-fb6f-4581-9299-06a8a8601025`.

## Budget (by-diff)

- Lines changed: 286
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
