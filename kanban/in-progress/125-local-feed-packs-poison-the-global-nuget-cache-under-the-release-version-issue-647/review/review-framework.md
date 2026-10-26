# Review framework

## Budget (by-diff)

- Lines changed: 870
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-26
**Host task:** kanban/in-progress/125-local-feed-packs-poison-the-global-nuget-cache-under-the-release-version-issue-647/
**Diff scope:** branch `task/125-local-feed-packs-poison-the-global-nuget-cache-und` (HEAD `7fde0ec1`) vs merge-base with `master` (`41b1df74`). Product commit `d0385ca2`; the rest is kanban.
**Plan / brief:** Isolated packages folder. `nuget.config` `globalPackagesFolder`, `RestorePackagesPath`, and `NUGET_PACKAGES` (set by the dev CLI and the scripts) all point at `artifacts/nuget-packages`. `dev pack` and `scripts/package.cs` refuse to pack when the shared cache's `.nupkg.metadata` `source` for the packed version is not http(s). Docs are in `scripts/overview.md`, `documentation/contributing/overview.md`, and `claude.md`.
**Effort:** 3 (by-diff budget). The roster axes are general only.
**Reviewer roster:** general (the review oracle, inline)
**Session IDs:** headless `ganda task work` review oracle (Claude Opus 5.5). The session id is not exposed to the oracle.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
