# Round 1 — merged findings
**Date:** 2026-11-06
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: nit — Status: wontfix
- File: source/timewarp-state-source-generator/facts/reducer-facts.cs:121
- Description: Rule 5 checks only the Action's declared accessibility. A `public` Action in a `private` or `protected`
  `*ActionSet` passes the rule. The build then fails in Mediator.g.cs with CS0122, not with a TWS0014 that names the
  reason.
- Suggestion: Also fail when the action set or a containing type is private, protected or private protected, and add
  an analyzer test.
- Source: general
- Disposition notes: wontfix, decided by the review oracle (orchestrator) on 2026-11-06. This is a nit for an unusual
  shape. A hand-written handler for the same shape fails the same way (CS0122), so `[Reducer]` adds no regression. The
  build still fails loudly, and nothing goes wrong silently. The fix fits the next TWS0014 touch, or the async-reducer
  follow-up already listed under Results. It does not need a fix pass on this task.

## Duplicates / conflicts

- None (single reviewer).
