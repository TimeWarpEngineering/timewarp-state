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

- [x] Add `id-token: write` (with `contents: read`) permissions to the publish job
- [x] Add `nuget/login@v1` gated on the publish condition
- [x] Replace the stored-secret `--api-key` with the login step output
- [ ] Verify the publish path end-to-end on the next release
- [ ] AFTER verified: operator revokes the long-lived NuGet key and deletes the GitHub secret (org-wide revocation tracked in nuru 458-009)

## Notes

Created from the timewarp-nuru 458-009 rollout session (2026-08-08).

## Session

- 2026-08-08: Implemented workflow migration in `.github/workflows/ci-cd.yml` — OIDC trusted publishing via `nuget/login@v1`, break-glass dispatch inputs, `release: types: [published]`, removed secret refs. Verify + revoke still open until next real release.
- 2026-08-08: Phase 4b review (effort 1, general) — clean disposition under `review/`. Folderized task for kitchen.

## Results

### What shipped

Migrated the NuGet publish path in `.github/workflows/ci-cd.yml` from the long-lived `PUBLISH_TO_NUGET_ORG` secret to NuGet Trusted Publishing (OIDC):

- `release.types: [published]` (no longer fires on draft `created` releases)
- `workflow_dispatch` inputs `mode` / `confirm` break-glass gate (default `merge` does not publish)
- `release` job permissions: `contents: read` + `id-token: write`
- `nuget/login@v1` with `user: TimeWarp.Enterprises`, gated on publish condition
- All three package pushes use `${{ steps.nuget-login.outputs.NUGET_API_KEY }}`
- Removed unused top-level `NUGET_AUTH_TOKEN` and all workflow references to `PUBLISH_TO_NUGET_ORG`

Out of scope (unchanged): full 458 reusable-workflow conversion; `TimeWarpStateVersion` SSOT; automatic key/secret revocation.

### Review (Phase 4b)

- Effort 1, roster: general only
- Rounds: 1
- Counts: 0 open / 0 fixed / 0 wontfix (bug, suggestion, nit all zero)
- Disposition: **clean**
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`

### Operator follow-ups (checklist remains open)

1. Verify E2E on the next non-draft GitHub Release (OIDC login + three pushes succeed; packages on nuget.org).
2. Only after that success: revoke long-lived NuGet API key and delete GitHub secret `PUBLISH_TO_NUGET_ORG` (org-wide: nuru 458-009). Do not revoke before first OIDC success.

### How to validate

**Smoke (pre-merge / post-merge, no publish):**

1. Confirm workflow has no secret refs:
   ```bash
   rg 'PUBLISH_TO_NUGET_ORG|NUGET_AUTH_TOKEN' .github/workflows/ci-cd.yml
   # Expect: no matches
   ```
2. Confirm OIDC + published trigger:
   ```bash
   rg -n 'types: \[published\]|nuget/login@v1|id-token: write|NUGET_API_KEY' .github/workflows/ci-cd.yml
   ```
3. Optional after merge to default branch: **Actions → CI/CD Pipeline → Run workflow** with `mode=merge` (default). Expect: release job packages; login and push steps **skipped**; job green.
4. Optional negative: dispatch `mode=release` with empty/wrong `confirm` → fails at “Validate break-glass confirmation”; nothing published.

**Expect (next real release):**

1. Set `TimeWarpStateVersion` in `source/Directory.Build.props` to match the release tag (existing process).
2. Create and **publish** a GitHub Release (not leave draft) so the event is `published`.
3. `release` job: tag validation → packages → NuGet login (OIDC) succeeds → three pushes succeed (or `--skip-duplicate`).
4. Packages appear on nuget.org: TimeWarp.State, TimeWarp.State.Plus, TimeWarp.State.Policies.
5. Then revoke long-lived key + delete `PUBLISH_TO_NUGET_ORG` secret.

**Automated gate:** Path-filtered PR that touches `ci-cd.yml` runs the `ci` job only; `release` job must not run on PR. No unit test covers GHA OIDC.

**Depends on:** NuGet.org trusted publishing policy for owner TimeWarp.Enterprises already created (2026-08-08) matching this repo and `.github/workflows/ci-cd.yml`.

**Not in scope:** Live nuget.org publish from this implement session; secret deletion in-repo.

### Implementation plan (2026-08-08)

**Goal:** Make the `release` job in `.github/workflows/ci-cd.yml` publish via NuGet Trusted Publishing (OIDC) instead of `PUBLISH_TO_NUGET_ORG`, adapted from timewarp-nuru reference — not a full 458 reusable-workflow conversion.

**Non-goals:** Full nuru 458 conversion; `TimeWarpStateVersion` SSOT changes; automating key/secret revocation (operator after first verified release); touching `ci`/`docs` behavior beyond removing unused top-level NuGet env.

**File:** `.github/workflows/ci-cd.yml` only (plus this task).

1. **Triggers:** `release.types: [created]` → `[published]`. Add `workflow_dispatch` inputs `mode` (merge|release, default merge) and `confirm` (string, default empty) — nuru break-glass pattern.
2. **Env:** Remove unused top-level `NUGET_AUTH_TOKEN: ${{ secrets.PUBLISH_TO_NUGET_ORG }}` (never referenced; pushes use secret inline today).
3. **`release` job permissions:** `contents: read` + `id-token: write` (job-scoped only; leave `ci`/`docs` alone).
4. **Break-glass validate step:** fail if `workflow_dispatch && mode==release && confirm!='release'`.
5. **`nuget/login@v1`** after package, `id: nuget-login`, `user: TimeWarp.Enterprises`, gated on `should_publish`.
6. **Three push steps:** same `should_publish` gate; `--api-key ${{ steps.nuget-login.outputs.NUGET_API_KEY }}`. Zero remaining `PUBLISH_TO_NUGET_ORG` references.

**`should_publish`:**
```text
github.event_name == 'release'
|| (github.event_name == 'workflow_dispatch' && inputs.mode == 'release' && inputs.confirm == 'release')
```

| Event | Behavior |
|-------|----------|
| `release` published | package → OIDC login → push ×3 |
| draft release (`created` only) | workflow does not run |
| dispatch `mode=merge` | package only; login/push skipped |
| dispatch `mode=release` + `confirm=release` | break-glass publish |
| dispatch `mode=release` bad confirm | fail at validate |

**Validate pre-merge:** YAML review; no secret refs; PR runs `ci` only. Optional post-merge dry dispatch `mode=merge`. **E2E:** next non-draft release; only then operator revokes key + deletes secret (nuru 458-009).

**Leave open until real release:** checklist items “Verify…” and “AFTER verified…”.
