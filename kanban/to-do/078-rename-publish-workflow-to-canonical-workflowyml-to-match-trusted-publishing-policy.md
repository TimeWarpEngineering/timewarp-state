# Rename publish workflow to canonical workflow.yml to match trusted publishing policy

## Description

Org ruling (timewarp-nuru 458-009, 2026-08-08): trusted publishing policies on
NuGet.org encode the convention — every repo's policy targets workflow file
`workflow.yml`, never legacy filenames. Repos conform to policy.

This repo's OIDC migration (previous task) landed on the legacy filename:
**ci-cd.yml**. The OIDC token's workflow claim therefore will NOT match the
repo's TP policy, and the next publish fails at `nuget/login`.

## Checklist

- [ ] The publishing workflow becomes `.github/workflows/workflow.yml` (rename or consolidate — exactly ONE canonical workflow publishes)
- [ ] No other workflow in this repo retains a NuGet push path
- [ ] Triggers/permissions/nuget-login content preserved from the migration
- [ ] Verify on next release: nuget/login succeeds against the workflow.yml policy

## Notes

Created from the timewarp-nuru 458-009 session after the TP-policy recreate was
standardized to workflow.yml for all 18 publishers. The full 458 conversion
(reusable-workflow caller) later replaces the CONTENT of workflow.yml; this
task only fixes the NAME so trusted publishing works meanwhile.
