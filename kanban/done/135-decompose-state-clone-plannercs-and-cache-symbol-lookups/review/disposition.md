# Disposition — task 135

**Date:** 2026-11-04
**Outcome:** clean
**Rounds:** 2
**Final open count:** 0

## Summary

Round 1 raised six findings: M1 and M2 (bug), M3–M5 (suggestion), and M6 (nit). The fix pass `8eadac32` fixed all six on this id, and round 2 re-verified each one against the post-fix tree. The new checks:

- Threading, tasks, `ICloneable`, `ReferenceAssemblyAttribute`, the shared BCL names, and the `ValueTuple` fallback match by name and namespace structure.
- A reference-pack generator test covers `CancellationTokenSource` and `SemaphoreSlim`.
- `Report` computes reachability lazily.
- `Emit` is five writers.
- The e2e runfile is a pure move into `#:include` partials, with every comment kept.

Round 2 raised no new findings. Final counts: bug 0 open / 2 fixed, suggestion 0 / 3, nit 0 / 1, wontfix 0.

## Exception log (if accepted-exceptions)

None.

## Escalations

None.
