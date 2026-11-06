# Review framework

## Budget (by-diff)

- Lines changed: 800
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope — task 145

**Date:** 2026-11-06
**Host task:** kanban/in-progress/145-option-5-shorter-action-authoring-reducer-shape-on-3b/
**Diff scope:** branch `task/145-option-5-shorter-action-authoring-reducer-shape-on` (HEAD a7e76a7d) vs merge-base
with `master` 47607cd2. Product commit 856ff215; 14 files, +714 / -86 (kanban files excluded from review).
**Plan / brief:** task.md `## Goal (implementer restatement)`: make `[Reducer]` the recommended action shape in the
docs, prove TWS0001, TWS0004-0008 and TWS0010/0011 accept it with analyzer tests, migrate one sample 05 state, add a
release-note "Authoring" section. No generator, analyzer or attribute change; no version bump.
**Effort:** 3 (by-diff budget), roster axis general only
**Reviewer roster:** general
**Session IDs:** review oracle, ganda task work, claude-opus-5-5 (this session)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
