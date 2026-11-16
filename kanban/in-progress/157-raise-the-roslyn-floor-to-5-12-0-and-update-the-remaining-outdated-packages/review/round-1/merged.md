# Round 1 — merged findings
**Date:** 2026-11-16
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: readme.md:44
- Description: The packed readme and `documentation/partials/installation.md:8` say ".NET 11 preview and RC SDKs are not supported." The task decision, the pin comment in `Directory.Packages.props`, and `documentation/release-notes/release12.0.0-beta.18.md` limit that exclusion to preview and RC SDKs that bundle an older Roslyn. Task 151 recorded that SDK 11.0.100-rc.3 bundles Roslyn 5.12 (toolset pin `5.12.0-1.26473.112`, AssemblyVersion 5.12.0.0), so the blanket sentence overstates the floor the rest of this change documents. The same sentence is inside `artifacts/packages/TimeWarp.State.12.0.0-beta.18.nupkg` `readme.md`.
- Suggestion: Use the decision's qualifier in `readme.md` and `documentation/partials/installation.md`: preview and RC SDKs that bundle an older Roslyn are not supported. Leave the minimum at .NET SDK 11.0.100 (GA).
- Source: general
- Disposition notes: fixed in the round-2 fix pass (2026-11-16). `readme.md:44` and `documentation/partials/installation.md:8` now say ".NET 11 preview and RC SDKs that bundle an older Roslyn are not supported." The rebuilt `artifacts/packages/TimeWarp.State.12.0.0-beta.18.nupkg` readme carries the same sentence. The minimum stays .NET SDK 11.0.100 (GA).

## Duplicates / conflicts

- One reviewer. Nothing to collapse.
