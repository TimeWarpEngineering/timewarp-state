# Round 4 — merged findings
**Date:** 2026-09-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

Re-verified against the NU1102 follow-up (`75f7f4de`):

| ID | Severity | Status | Round-4 verdict |
|----|----------|--------|-----------------|
| M1 | suggestion | fixed | verified-fixed — YAML still has no `<Version>` extract; `AssertVersionSsot` still runs in DevCli for PR/merge and release. |

Independent merge-pass: `timewarp-state.slnx` has 21 projects (7 samples, 14 source+test). Generated `artifacts/timewarp-state.build.slnf` lists those 14 and omits `samples/`. `StartsWith("samples/")` after backslash-normalize matches slnx `Path` values. `RunPrAsync` is pack then verify-samples. `dev test` / `dev e2e` restore individual test/SUT csprojs (test-app uses ProjectReference to source). Workflow clean is `IRepoCleanService` (bin/obj), not `dotnet restore` of the full slnx.

## Issues

None new.

## Duplicates / conflicts

- None. Single general reviewer; prior M1 carried with updated status.
