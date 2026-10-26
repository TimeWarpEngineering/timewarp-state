# Round 2 — merged findings
**Date:** 2026-10-26
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: tools/dev-cli/endpoints/verify-samples-command.cs:102
- Description: The sample 00 auto restore reused a stale same-version extract in `artifacts/nuget-packages`.
- Source: general (round 1)
- Disposition notes: `6fa0f6b8`. `PurgePackedExtractsAsync` deletes each packable id's version folder after pack, and the restore runs with `--force`. Verified: the isolated `.nupkg.metadata` `contentHash` equals the SHA-512 of the packed `TimeWarp.State` nupkg.

### M2 — Severity: suggestion — Status: fixed
- File: tests/timewarp-state-packaging-tests/nupkg-content-tests.cs:145
- Description: The Plus case passed when only one of its two scripts was present.
- Source: general (round 1)
- Disposition notes: `6fa0f6b8`. Each script is asserted on its own, and `FailPlusWhenDownloadFileIsMissing` covers the regression.

### M3 — Severity: nit — Status: fixed
- File: tests/timewarp-state-packaging-tests/nupkg-content-tests.cs:226
- Description: `HasAssetPair` parsed every `.json`, including the JSONC tsconfig.
- Source: general (round 1)
- Disposition notes: `6fa0f6b8`. Only manifest JSON is parsed. The real nupkgs keep `build/<id>.PackageAssets.json` in scope. Two new tests cover JSONC.

## New findings

None.

## Duplicates / conflicts

- None (single reviewer).
