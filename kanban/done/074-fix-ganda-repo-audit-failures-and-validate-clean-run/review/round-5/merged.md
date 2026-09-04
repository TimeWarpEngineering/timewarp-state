# Round 5 — merged findings
**Date:** 2026-09-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

Re-verified against the Fixie follow-up (`52ba2de0`):

| ID | Severity | Status | Round-5 verdict |
|----|----------|--------|-----------------|
| M1 | suggestion | fixed | verified-fixed — YAML still has no `<Version>` extract; `AssertVersionSsot` still runs in DevCli for PR/merge (`RunPrAsync`) and release (`RunReleaseAsync`). |

Independent merge-pass: `workflow.yml` is still the thin bash trigger (`dotnet run --file tools/dev-cli/dev.cs -- workflow`; no `shell: pwsh`, no `scripts/*.cs` steps, no `packages.lock.json`). `RunPrAsync` Test step invokes `TestCommand.Handler`, which `dotnet tool restore`s from `Git.FindRoot()` (`.config/dotnet-tools.json`, `fixie.console` 3.4.0) with `WithNoValidation` and fails the process on non-zero before wrapping `scripts/test.cs`. Direct `scripts/test.cs` restores via `RunStep` after `ScriptContext.FromRelativePath("..")` and before the first `dotnet fixie`. Dual restore is the documented two-entry-point design. `RunStepAsync` observes `Environment.ExitCode` from Test. E2E has no Fixie. Whole-manifest restore matches leftover `scripts/build.cs` (`DotNet.Tool().Restore()`), which YAML no longer calls.

## Issues

None new.

## Duplicates / conflicts

- None. Single general reviewer; prior M1 carried with updated status.
