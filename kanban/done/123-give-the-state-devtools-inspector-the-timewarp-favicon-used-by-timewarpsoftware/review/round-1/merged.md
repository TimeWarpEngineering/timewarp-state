# Round 1 — merged findings
**Date:** 2026-10-17
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: nit — Status: wontfix
- File: source/timewarp-state-devtools/timewarp-state-devtools.csproj:40
- Description: `PointDevToolsFaviconAtBrandFile` matches any `StaticWebAsset` whose Identity ends with `assets/logo.png`, and that includes referenced-project assets. A future RCL `wwwroot/assets/logo.png` would have its ContentRoot rewritten.
- Suggestion: Compare `%(FullPath)` to `$(_BrandFaviconDirectory)logo.png` exactly, or also require `'%(SourceType)' == 'Discovered'`.
- Source: general
- Disposition notes: wontfix (decided by the review oracle, 2026-10-17). It is latent only. The current build manifests (Debug and Release) contain exactly one matching asset, the brand file. The inspector's references (`timewarp-state`, `timewarp-state-blazor`, `timewarp-state-devtools-export`) ship no `assets/logo.png`. The tests pass. Spending a fix-pass round on a hypothetical nit does not pay for itself. If the condition is ever touched again, tighten it to the exact path.

## Duplicates / conflicts

- None (single reviewer).
