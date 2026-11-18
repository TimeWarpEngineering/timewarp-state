# Review framework — task 150

**Date:** 2026-11-18
**Host task:** kanban/in-progress/150-attestation-gated-ci-skip-rebuildteste2e-when-a-signed-local-run-covers-the-tree/
**Diff scope:** branch `task/150-attestation-gated-ci-skip-rebuildteste2e-when-a` vs `master` (944 lines, 16 files). Product commits through `357d2187`; later commits are kanban only. Untracked kitchen files under `review/` are this review.
**Plan / brief:** `task.md` goal, the no-signing override (Steven 2026-11-17), and `rfc/rfc.md` resolutions, with signing replaced by the unsigned `refs/notes/ganda-ci` note.
**Effort:** 3
**Reviewer roster:** general
**Session IDs:** grok `01a1eed5-9579-7b12-8049-91779d8e067f` (review oracle). Implement session recorded on `task.md`.

## Budget (by-diff)

- Lines changed: 944
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
