# Review framework — task 149

**Date:** 2026-11-07
**Host task:** kanban/in-progress/149-make-dev-e2e-run-locally-without-sudo-skip-cached-playwright-install-ci-only-with-deps/
**Diff scope:** branch `task/149-make-dev-e2e-run-locally-without-sudo-skip-cached` vs merge-base `a85ce50c` (master). Product files: `scripts/e2e-build.cs`, `scripts/e2e-dev-certs.cs`, `scripts/e2e.cs`, `scripts/global-usings.cs`, `tools/dev-cli/endpoints/e2e-command.cs`. Kitchen `task.md` move is in the same range. 498 lines changed.
**Plan / brief:** Make `dev e2e` finish on a developer box with no password prompt, skip a Playwright install when the pinned Chromium revision is already cached, and keep `--with-deps` for CI.
**Effort:** 2 (by-diff, 498 lines)
**Reviewer roster:** general
**Session IDs:** session not reported (Cursor harness has no `--max-turns`; bound cap uncapped). Budget turn cap 120.

## Budget (by-diff)

- Lines changed: 498
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
