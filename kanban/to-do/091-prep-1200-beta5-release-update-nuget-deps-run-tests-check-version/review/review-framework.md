# Review framework — task 091

**Date:** 2026-09-23
**Host task:** kanban/to-do/091-prep-1200-beta5-release-update-nuget-deps-run-tests-check-version/
**Diff scope:** branch `task/091-prep-1200-beta5-release-update-nuget-deps-run-test` vs `master`
  (commits `fe119600`, `2995e1f2`, `23a68854`): `.gitignore`, `Directory.Packages.props`,
  `source/timewarp-state/tsconfig.json`, generated `source/timewarp-state/wwwroot/js/*.js(.map)`.
**Plan / brief:** Prep 12.0.0-beta.5 release — bump every NuGet dependency to latest,
  keep the pipeline green, close the `.memsearch/` gitignore gap, confirm `check-version`.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** claude-sonnet-5 (this session, review oracle)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
