# Round 1 — merged findings
**Date:** 2026-10-28
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: open
- File: documentation/release-notes/ (no entry); source/Directory.Build.props:9
- Description: The consumer-breaking Mediator 15 move has no release note and no version bump. Repo convention (`b05ababd`, `91e42103`, `b9288f80`) adds both for breaking changes. The breaks are: handlers and behaviors move to `ValueTask`; the package ids change; hosts must import `TimeWarp.Mediator.Generated`; `LogTimeWarpStateMiddleware` takes a new required parameter; `PipelineBehaviorRegistry.Register` and the registry generator are removed; `GetComponentOrder` changes meaning.
- Suggestion: Add a **Breaking** release-notes entry that lists these items and links the migration page, and follow the repo's version-bump and `toc.yml` convention.
- Source: general
- Disposition notes:

### M2 — Severity: nit — Status: open
- File: tests/timewarp-state-tests/javascript-interop/json-request-handler-dispatch-tests.cs:190 (also json-request-handler-lifecycle-tests.cs:67, store/store-get-or-add-tests.cs:177)
- Description: Removing the `CreateStream` members left double blank lines in three fakes.
- Suggestion: Delete the extra blank lines.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None (single reviewer).
