# Disposition — task 125

**Date:** 2026-10-26
**Outcome:** accepted-exceptions
**Rounds:** 1
**Final open count:** 0

## Summary

Round 1 (effort 3, general reviewer) found no bugs and no suggestions, and one nit. Restores now land in the worktree's `artifacts/nuget-packages` through three layers: `nuget.config` `globalPackagesFolder`, `RestorePackagesPath`, and `NUGET_PACKAGES`. The shared-cache guard blocks `dev pack` and `scripts/package.cs` when a local-sourced entry exists, and it has unit tests. Re-verified on this run:

- `dotnet nuget locals global-packages -l` resolves to the isolated folder.
- The 6 guard tests pass.
- A build and restore wrote nothing new to `~/.nuget/packages`.

The one nit (M1, a per-worktree cold-cache cost the docs do not mention) is accepted as wontfix.

## Exception log (if accepted-exceptions)

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M1 | nit | Documentation-only. The cost is inherent to the isolation approach the task allows, and behaviour is correct. One sentence does not justify a fix pass plus another review round. | review oracle (orchestrator) |

## Escalations

- None.
