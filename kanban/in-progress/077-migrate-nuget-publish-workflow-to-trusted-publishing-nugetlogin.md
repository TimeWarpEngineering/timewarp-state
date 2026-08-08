# Migrate NuGet publish workflow to trusted publishing (nuget/login)

## Description

The trusted publishing policy for this repo already exists on NuGet.org
(owner TimeWarp.Enterprises, created 2026-08-08) but is INERT until the
publish workflow exchanges an OIDC token for a temp key instead of using a
stored secret. Org program context: timewarp-nuru kanban 458-009.

Current state (2026-08-07 org audit): secret `PUBLISH_TO_NUGET_ORG` in ci-cd.yml; trigger is `release: types [created]` which FIRES ON DRAFT RELEASES — change to `published` while in here; dispatch publishes unconditionally (gate it); version SSOT sits behind the `TimeWarpStateVersion` indirection (full SSOT alignment belongs to the 458 conversion, not this task).

Reference implementation: timewarp-nuru `.github/workflows/workflow.yml` —
`nuget/login@v1` step (user: TimeWarp.Enterprises) gated on the release
condition, `id-token: write` job permission, push via
`--api-key ${{ steps.nuget-login.outputs.NUGET_API_KEY }}`.

NOTE: if this repo's full convention conversion (reusable-workflow caller,
timewarp-nuru 458 rollout) is imminent, do the conversion instead — it
includes this migration for free.

## Checklist

- [ ] Add `id-token: write` (with `contents: read`) permissions to the publish job
- [ ] Add `nuget/login@v1` gated on the publish condition
- [ ] Replace the stored-secret `--api-key` with the login step output
- [ ] Verify the publish path end-to-end on the next release
- [ ] AFTER verified: operator revokes the long-lived NuGet key and deletes the GitHub secret (org-wide revocation tracked in nuru 458-009)

## Notes

Created from the timewarp-nuru 458-009 rollout session (2026-08-08).
