# Attestation-gated CI: skip rebuild/test/e2e when a signed local run covers the tree

## Description

CI takes ~5.5 minutes per master/PR run and every agent walk waits on it at the merge gate. Steve's direction
(2026-11-07): run the pipeline locally, attest the result, and let CI skip work that a trusted signed run already
covered. Apply this to all builds, not just e2e.

Measured on master run for 51e8e997 (2026-11-07): total 5m34s. Inside `Run CI Pipeline`: restore + build + unit
test suites ~3m, e2e ~70s (publish SUT 25s, Playwright install 16s, tests 26s), verify-samples ~50s.

`ganda repo attest` already signs `tree` + `check_set` into a git note (key `tw-audit-1`) and `ganda repo attest status`
verifies it against the current tree. Today the check_set only covers the audit checks. The release pipeline already
promotes the CI artifact without rebuilding, so "trust a signed earlier run" is an existing principle in this repo.

## Goal

A developer or agent runs `dev ci` (or `dev build && dev test && dev e2e`) locally in the task worktree. On success it
attests the tree with a check_set that records which pipeline stages passed. The GitHub workflow, before the
`Run CI Pipeline` step, checks for a valid attestation covering HEAD's tree and the required stage set. If present,
it skips to packing/uploading the artifact (or skips entirely when nothing downstream needs the artifact).
If absent or stale, it runs the full pipeline as today.

## Design questions to settle (write a short RFC under this task's `rfc/` folder before implementing)

1. Where does stage coverage live: extend ganda's check_set to include stage ids (`build`, `test`, `e2e`,
   `verify-samples`) with pass/fail, or a separate `tw-ci` note namespace in this repo? Ganda change needs a
   timewarp-ganda task; link it.
2. Trust boundary: attestation key is the org trust root ("the ganda audit ran and passed"). Extending its meaning to
   "tests passed" must be explicit in the note payload, never implied. Decide whether a second key or a payload field
   does this.
3. What CI still must do even when attested: produce the `Packages-*` artifact the release promotes. Cheapest path is
   `dotnet pack` only (no test), or have the local run upload the artifact too (needs a token story). Pick one.
4. Branch policy: honor attestations on task branches and PRs; master merge commits are new trees, so the gate must
   accept "tree equals a PR head tree that was attested" or CI re-runs on every merge and the win is lost. Merge
   commits with identical tree to the PR head (fast-forward-equivalent) can match by tree hash; others cannot.
5. Staleness: tool-version drift (SDK, Playwright, Nuru DevCli) between local and CI. Include tool versions in the
   payload and let CI refuse mismatched ones.
6. Agent walks: `ganda task work` should run `dev ci` + attest before `open-pr` so the PR's CI is the fast path by
   default. Coordinate with the task-work host steps (likely a timewarp-ganda task).

## Requirements

- RFC with the decisions above and the per-run time saved, committed under `rfc/`.
- Implement the repo-side pieces here: `dev ci` (or equivalent) that runs the stages and calls attest with the stage
  payload; workflow gate step that reads the attestation and skips accordingly; artifact still produced.
- Ganda-side pieces go in timewarp-ganda tasks; list their ids under `## Notes` and block on them explicitly.
- Prove it: one PR where CI skips the pipeline on an attested tree and finishes in under 1 minute, and one where a
  non-attested push still runs everything. Record both run ids under `## Results`.
- Depends on task 149 (local `dev e2e` must run without prompts).
- Run `ganda repo audit --fix` in this worktree and commit whatever it changes (audit fixes are policy).

## Checklist

- [ ] RFC written under `rfc/` with the six decisions resolved
- [ ] Ganda-side tasks created in timewarp-ganda and linked
- [ ] `dev ci` runs all stages locally and attests with stage payload
- [ ] Workflow gate reads attestation; skips pipeline on match; full run on miss
- [ ] `Packages-*` artifact still produced on the fast path
- [ ] Tool-version drift refuses the fast path
- [ ] Two proving runs recorded (fast path < 1 min, slow path unchanged)
- [ ] `ganda task work` runs the local pipeline + attest before open-pr (or follow-up task linked)
- [ ] `ganda repo audit --fix` run and its changes committed
- [ ] `## Results` filled in

## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-07)
- Context: Steve: "CI takes forever and it slows up all of our agents"; wants local run + attestation to skip CI work across all builds (2026-11-07)

## Notes

- Prerequisite: task 149.
- Timing source: `gh run view` on the master run for 51e8e997 (task 148 merge), 2026-11-07.
