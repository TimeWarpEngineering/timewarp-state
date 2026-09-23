# Prep 12.0.0-beta.5 release: update NuGet deps, run tests, check-version

## Description

Prepare timewarp-state for its next NuGet release. `source/Directory.Build.props`
carries `12.0.0-beta.5`; the newest tag is `v12.0.0-beta.3` (2026-09-17) and
neither beta.4 nor beta.5 has been published for any of the four packages
(TimeWarp.State, .Plus, .Policies, .Telemetry). Master is 121 commits ahead of
that tag, including the Telemetry OTel pipeline, render-suppression and
Store race fixes, the Subscriptions lock for Blazor Server, and the TW0002
analyzer. Before cutting, bring every NuGet dependency to latest, prove the
full pipeline is green, and confirm `dev check-version` accepts the version.
Open the PR; the cockpit cuts the release with `dev release` after merge.

## Checklist

- [ ] Run `ganda nuget outdated` in the claim worktree; record the report in Notes
- [ ] Run `ganda nuget outdated --update --force` so every package is on latest; review the diff for major bumps that need code changes
- [ ] `dotnet run --file tools/dev-cli/dev.cs -- workflow` (assert-version-ssot → clean → build → test → e2e → pack → verify-samples) green locally; record test totals (failed == 0, total > 0)
- [ ] `dotnet run --file tools/dev-cli/dev.cs -- check-version` passes for all four packages at 12.0.0-beta.5; if it refuses, bump `<Version>` as it directs and re-run
- [ ] `.gitignore` covers `.memsearch/` at the top level (the master checkout has an untracked `.memsearch/` that dirties `dev release`'s clean-tree guard); keep the existing `.memsearch/memory/` promote exception
- [ ] `ganda repo audit` clean
- [ ] `dotnet run --file tools/dev-cli/dev.cs -- release --dry-run` from the task branch fails only the on-master guard
- [ ] Commit with conventional message; PR open with the outdated report and test totals in the body

## Session

- Created: 2026-09-23 (cockpit dispatch)

## Notes

- Do not tag or run `dev release` on this task. The PR is the deliverable; the cockpit releases after merge.
- All four packages ship together at the single props version (org convention, nuru task 458). Never add a `<Version>` to a csproj.
- If a dependency bump breaks build, test, or e2e, fix forward here and note it; do not pin back without saying why.
- Cross-check: amuru task 115 (2026-09-23) is the same prep shape and is a good reference for the PR body.
