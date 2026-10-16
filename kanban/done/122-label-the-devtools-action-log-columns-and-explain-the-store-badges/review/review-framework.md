# Review framework

## Budget (by-diff)

- Lines changed: 231
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Round 1

- **Date:** 2026-10-16
- **Host task:** kanban/to-do/122-label-the-devtools-action-log-columns-and-explain-the-store-badges/
- **Diff scope:** branch `task/122-label-the-devtools-action-log-columns-and-explain` (5093b07f) vs `master` (c2c4e444)
- **Plan / brief:** task.md Requirements + 2026-10-16 amendment; `## Goal (implementer restatement)`
- **Effort:** 2 — roster: general
- **Session IDs:** reviewer claude (claude-opus-5-5), session id not exposed in print mode

## Round 2

- **Date:** 2026-10-16
- **Diff scope:** fix delta b2bccf43 vs round-1 base 7f941cfc (231 lines changed)
- **Brief:** re-check M1–M5 from `round-1/merged.md` and look for new defects in the fix delta
- **Effort:** 2 — roster: general
- **Session IDs:** reviewer claude (claude-opus-5-5); print mode does not expose the session id

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
