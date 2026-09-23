# Review framework — task 090

**Date:** 2026-09-22
**Host task:** kanban/to-do/090-stateinitialize-test-assembly-guard-is-case-sensitive-containstest-kebab-cased-test-assemblies-like-web-spa-integration-tests-fail-it/
**Diff scope:** branch `task/090-stateinitialize-test-assembly-guard-is-case-sensit` vs `origin/master` (product: `c0fe0d5d` fix: allow kebab-case test assemblies without AssemblyName override)
**Plan / brief:** `task.md` — `ThrowIfNotTestAssembly` used case-sensitive `Contains("Test")`; kebab-case test assemblies failed. Prefer `StateTestOptions.Enable()` with ordinal-ignore-case name sniff as beta.5 fallback; tests; release notes; version bump to 12.0.0-beta.5.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** cursor review oracle (ganda task-work, 2026-09-22)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
