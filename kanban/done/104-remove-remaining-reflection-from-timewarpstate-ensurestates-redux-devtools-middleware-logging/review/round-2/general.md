# Round 2 — general (orchestrator re-verification)
**Date:** 2026-10-12
**Scope reviewed:** fix delta b9288f80..0f1411f8, plus prior M1–M7

## Summary

The fix commit touches five files: `StateRegistrationRegistry.Apply` runs each assembly's module constructor before reading the registrar snapshot, the pipeline generator returns early without `PipelineBehaviorRegistry`, `GetComponentOrder` gains remarks, and the release notes and `cloning.md` document the behavior changes and the generator requirement. The registrar snapshot is now read after `RunModuleConstructor`, so a delegate the initializer just registered is seen. No new defects found.

## Verification

- `dotnet build source/timewarp-state/timewarp-state.csproj -c Release --nologo`: Build succeeded, no IL2xxx/IL3xxx.
- `dotnet test tests/timewarp-state-tests`: 120 passed, 1 skipped.
- `dotnet test tests/timewarp-state-source-generator-tests`: 90 passed.
- `dotnet test tests/client-integration-tests`: 64 passed, 1 skipped.

## Prior IDs

| ID | Status |
|----|--------|
| M1 | fixed |
| M2 | fixed |
| M3 | fixed |
| M4 | fixed |
| M5 | wontfix (unchanged rationale) |
| M6 | wontfix (unchanged rationale) |
| M7 | fixed |

## Issues

None.
