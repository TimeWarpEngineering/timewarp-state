# Round 2 — general
**Date:** 2026-10-10
**Scope reviewed:** Fix delta in `review-findings.md` for round-1 M1–M3. Product code was not changed.

## Summary

The three citation fixes land on the lines they name. No new defect in the delta. The rest of the findings file was not re-litigated; round 1 already checked the High and Medium claims against the tree.

## Issues

### Resolved prior

- M1 (bug): `store.redux-dev-tools.cs:43-71` is the seven `UnconditionalSuppressMessage` attributes (file length 119). `i-store.cs:27` is `object GetState(Type)`, `:29` is `GetSemaphore`, `:37` is `StateInitializationTasks` (file length 38). The old ranges `274-349` and `379,387` are gone.
- M2 (nit): the nit counts five `TODO` comments and points the fifth at `policies.action-policy.cs:15`, which is the file-layout note.
- M3 (nit): L1 cites `timewarp-state-options.cs:29`, which is `UseStateTransactionBehavior`.

No new issues.
