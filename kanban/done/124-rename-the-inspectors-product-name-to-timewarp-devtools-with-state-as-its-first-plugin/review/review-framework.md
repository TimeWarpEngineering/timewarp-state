# Review framework

## Budget (by-diff)

- Lines changed: 261
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope — task 124

**Date:** 2026-10-17
**Host task:** kanban/to-do/124-rename-the-inspectors-product-name-to-timewarp-devtools-with-state-as-its-first-plugin/
**Diff scope:** commit `b05ababd` (product change) plus `8707b408` (kanban Results) on branch `task/124-rename-the-inspectors-product-name-to-timewarp-dev`. The local `master` ref is behind origin, so `master...HEAD` also shows tasks 122 and 123. Those were merged in #651 and #652 and are out of scope here.
**Plan / brief:** Rename the inspector product to TimeWarp DevTools with State as the first plugin label. Set the Aspire default resource name to `timewarp-devtools` and the dashboard text to `TimeWarp DevTools`. Add a breaking-change release note and bump the version.
**Effort:** 2 (by-diff budget); roster axes: general only
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle session not reported by host

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
