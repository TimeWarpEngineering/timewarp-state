# Round 1 — merged findings
**Date:** 2026-10-26
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: tools/dev-cli/endpoints/verify-samples-command.cs:99
- Description: The sample 00 auto restore resolves `TimeWarp.State*` at the same version from the existing `artifacts/nuget-packages` extraction, not from the nupkg just packed. In this worktree, the cached `.nupkg.metadata` (19:50, contentHash `r4tHpL1d…`) predates the current nupkg (19:53, SHA-512 `GNdge4KA…`). Locally, the one packaged-consumer gate can therefore pass against stale bits. CI is not affected because it uses a fresh runner.
- Suggestion: Before the restore, purge `artifacts/nuget-packages/<id>/<version>` for each packable id, then force the restore. Or run that restore and build with `NUGET_PACKAGES` set to a per-run folder. The env var overrides the config's `globalPackagesFolder`.
- Source: general
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: tests/timewarp-state-packaging-tests/nupkg-content-tests.cs:68
- Description: The Plus case passes when either `document-title.js` or `download-file.js` is present and paired, so dropping one of them does not fail the test.
- Suggestion: Assert each script's `staticwebassets/js/` entry and its BasePath pair on its own.
- Source: general
- Disposition notes:

### M3 — Severity: nit — Status: open
- File: tests/timewarp-state-packaging-tests/nupkg-content-tests.cs:134
- Description: `HasAssetPair` parses every `.json` in the nupkg, including the JSONC `contentFiles/.../tsconfig.json`. A comment there would throw instead of giving a layout failure.
- Suggestion: Parse only the static web asset manifests, or allow comments and trailing commas.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None (single reviewer).
