# Round 2 — merged findings
**Date:** 2026-09-04
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

Re-verified against the uncommitted M1 fix delta (`workflow.yml` + `tools/dev-cli/endpoints/workflow-command.cs`):

| ID | Severity | Status | Round-2 verdict |
|----|----------|--------|-----------------|
| M1 | suggestion | fixed | verified-fixed — DevCli `AssertVersionSsot` (PR + release) and `workflow.yml` ci + `extract_version` gates; both literals `12.0.0-beta.3`; mismatch fails; probe skip of `extract_version` is expected |

## Issues

None new.

## Duplicates / conflicts

- None. Single general reviewer; prior M1 carried with updated status.
