# Review framework — task 164

**Date:** 2026-11-19
**Host task:** kanban/in-progress/164-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-state/
**Diff scope:** branch `task/164-refresh-outdated-nu-get-packages-in-time-warp` vs `master` (three-dot). Product and kitchen commits through `43e09174`. 16 files, 162 lines. `review/` is untracked and is this review.
**Plan / brief:** Refresh the three outdated NuGet pins in `Directory.Packages.props` (Playwright.MSTest.v4 1.65.0, Aspire.Hosting 17.0.0, Aspire.Hosting.AppHost 17.0.0) and set `<Version>` to `12.0.0-beta.19`. The implementer also moved Aspire 17 call sites off obsolete `AddProject` and `Aspire.AppHost.Sdk`.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok `01a1f048-ab18-7b91-a7c5-42dbba6fef1e` (2026-11-19, round 1). Round 2: grok `01a1f053-b783-7360-b462-b58d6babf168` (2026-11-19). Implementer session was not reported.

## Round 2 scope

Delta since round 1 base `43e09174` through `04cf07c0` (fix commit). Product files: `tools/dev-cli/endpoints/nuspec-dependency-allow-list.cs`, `tests/timewarp-state-tests/packaging/nuspec-dependency-allow-list-tests.cs`. Re-verify M1. Do not re-review the earlier pin bump.

## Budget (by-diff)

- Lines changed: 155
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
