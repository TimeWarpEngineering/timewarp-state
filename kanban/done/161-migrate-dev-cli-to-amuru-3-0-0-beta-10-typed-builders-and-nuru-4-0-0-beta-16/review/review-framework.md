# Review framework — task 161

**Date:** 2026-11-18
**Host task:** kanban/in-progress/161-migrate-dev-cli-to-amuru-3-0-0-beta-10-typed-builders-and-nuru-4-0-0-beta-16/
**Diff scope:** branch `task/161-migrate-dev-cli-to-amuru-3-0-0-beta-10-typed` vs `master` (product diff: `.editorconfig`, `Directory.Packages.props`, `scripts/*.cs`, `tools/dev-cli/endpoints/`, `tests/timewarp-state-tests`). 372 lines changed. Master is two kanban-only commits ahead; those commits do not touch product code.
**Plan / brief:** Move dev-only pins to TimeWarp.Amuru / Amuru.Tools 3.0.0-beta.10 and TimeWarp.Nuru / Nuru.DevCli 4.0.0-beta.16, replace analyzer-refused `Shell.Builder("dotnet"|"gh")` call sites with typed builders, keep exit codes and argv behavior, compile the test project against DevCli `release-guard.cs`, and delete the four `package-pin-currency.hold` entries.
**Effort:** 2
**Reviewer roster:** general
**Session IDs:** Grok `01a1ef10-248e-7140-8f3b-1bc0817c81a9` (2026-11-18). Implementer session was not reported (implement oracle note on `task.md`).

## Budget (by-diff)

- Lines changed: 372
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
