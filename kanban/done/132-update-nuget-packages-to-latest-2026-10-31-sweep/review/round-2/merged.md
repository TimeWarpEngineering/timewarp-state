# Round 2 — merged findings
**Date:** 2026-10-31
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 0 |

## Resolved prior

### M1 — Severity: suggestion — Status: fixed
- File: .editorconfig:295-333, msbuild/external-sources.globalconfig:14-28, documentation/release-notes/release12.0.0-beta.16.md:39
- Description: TWA0025 and TWA0035 were not turned off, and the list of new ids was incomplete.
- Source: general (round 1)
- Disposition notes: Fixed in a1876db4. Both ids are `none` in `[*.cs]` and in the globalconfig. The ten-id list is corrected in the config comment, the release notes, and Results.

### M2 — Severity: nit — Status: fixed
- File: .editorconfig (former `[*.razor]` block) vs msbuild/external-sources.globalconfig
- Description: The two config comments contradicted each other about what silences Razor TWA0036, so the `[*.razor]` block may have been dead.
- Source: general (round 1)
- Disposition notes: Fixed in a1876db4. The `[*.razor]` block is removed, and both comments now credit the globalconfig. The implementer's sample 00 probe is in Results. A round-2 build of the sample 00 projects shows no TWA diagnostics.

## Issues

None new.

## Duplicates / conflicts

- None; single reviewer.
