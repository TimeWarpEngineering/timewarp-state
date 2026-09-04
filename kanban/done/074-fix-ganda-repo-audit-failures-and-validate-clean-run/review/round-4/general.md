# Round 4 — general
**Date:** 2026-09-04
**Scope reviewed:** NU1102 follow-up (commits `6237d0b9` + `75f7f4de` vs round-3 `c17fc5d1`)

## Summary

`dev build` now writes a gitignored `artifacts/timewarp-state.build.slnf` that omits `samples/**` (14 source+test projects verified against `timewarp-state.slnx`), and `RunPrAsync` packs LocalNuGetFeed before `verify-samples`. That closes the CI NU1102 path: samples PackageReference `TimeWarp.State`/`Plus` at `TimeWarpStateVersion`, which is absent from nuget.org until pack. Surrounding call sites are safe — `dev test`/`dev e2e` restore individual test/SUT csprojs (ProjectReference to source, not sample PackageReference), workflow `Clean` via `RepoCleanService` only deletes bin/obj (no solution restore), and `dev build --clean` runs `dotnet clean` on the full slnx without restoring packages. M1 remains verified-fixed: YAML has no `<Version>` extract; `AssertVersionSsot` still runs for PR/merge and release. Handwritten slnf JSON matches PublishAot (no reflection `JsonSerializer`); absolute `solution.path` plus backslash project paths match the generated filter that already built locally. Overall risk is low; no open defects in this delta.

## Issues
