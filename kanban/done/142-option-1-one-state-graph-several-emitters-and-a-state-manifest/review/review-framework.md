# Review framework

## Budget (by-diff)

- Lines changed: 593
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope (review oracle, round 1)

**Date:** 2026-11-06
**Host task:** kanban/in-progress/142-option-1-one-state-graph-several-emitters-and-a-state-manifest/
**Diff scope:** branch `task/142-option-1-one-state-graph-several-emitters-and-a-st` vs `master`
(merge base b92802bb, head 75b10bdd; 90 files, +4398 / -1210)
**Plan / brief:** task.md `## Goal (implementer restatement)` and `## Requirements`. One `TimeWarpStateGenerator`,
one equatable `StateGraph`, emitters with golden parity, the State manifest, DevTools ingest, and an incrementality test.
**Effort:** 3. Roster axis is general. One general reviewer (the review oracle) owns `round-1/general.md`. Two
read-only helper passes covered (a) the DevTools and runtime manifest slice and (b) the analyzer linked facts plus
test claims. Their findings were re-verified and folded into general.md.
**Session IDs:** review oracle claude-opus-5-5 (ganda task work review node); helpers general-purpose subagents of
that session.

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity is bug, suggestion or nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Address the diff and the call sites around it. Re-verify falsifiable claims against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.

## Scope (review oracle, round 2)

**Date:** 2026-11-06
**Diff scope:** the round-1 fix delta, `75b10bdd..138a9298` (21 product files, kanban excluded). M1 to M12 were
re-verified, and the delta was scanned for new defects.
**Effort:** 2 (the budget above). The roster is general: one general reviewer, the review oracle, with no helper
agents.
**Session IDs:** review oracle claude-opus-5-5 (ganda task work review-2 node).
