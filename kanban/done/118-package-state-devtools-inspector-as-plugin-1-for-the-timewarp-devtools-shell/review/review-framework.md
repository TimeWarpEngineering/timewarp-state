# Review framework

## Budget (by-diff)

- Lines changed: 275
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Rounds

- Round 1: `99585466..f075d4a3`, effort 3 (976 lines), general. Review oracle session
  3bc6725a-883d-4f3b-b451-3c8caedd96e0. 5 open findings.
- Round 2: fix delta `9f4be5b8..04c63679`, effort 2 (275 lines), general (review oracle,
  claude-opus-5-5, session 532d65d0-a89f-4bd9-949d-fc9f1773beb0). M1–M5 fixed, no new findings.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
