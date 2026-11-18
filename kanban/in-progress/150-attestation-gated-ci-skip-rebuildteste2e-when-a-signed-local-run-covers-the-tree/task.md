# Attestation-gated CI: skip rebuild/test/e2e when a signed local run covers the tree

## Description

CI takes 5.5 to 7 minutes per master/PR run and every agent walk waits on it at the merge gate. Steve's direction
(2026-11-07): run the pipeline locally, attest the result, and let CI skip work that a trusted signed run already
covered. Apply this to all builds, not just e2e.

Measured on master run for 51e8e997 (2026-11-07): total 5m34s. Inside `Run CI Pipeline`: restore + build + unit
test suites ~3m, e2e ~70s (publish SUT 25s, Playwright install 16s, tests 26s), verify-samples ~50s.
Re-checked 2026-11-16: the five most recent successful runs took 5m28s to 7m12s, so the saving still stands.

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
7. Coverage upload: since task 154 the workflow has a `coverage` step that uploads `artifacts/coverage` with
   `if-no-files-found: error`, and that file is written by the e2e test run. A fast path that skips e2e must skip that
   step too, or carry the coverage file some other way. Pick one.

## How to run this task

RFC first. The design questions above are the first deliverable and the ganda-side tasks do not exist yet, so a
plain implement walk cannot finish. Start with `ganda task work 150 --routine rfc`; implement on this same task
after the RFC is folded in.

## Requirements

- RFC with the decisions above and the per-run time saved, committed under `rfc/`.
- Implement the repo-side pieces here: `dev ci` (or equivalent) that runs the stages and calls attest with the stage
  payload; workflow gate step that reads the attestation and skips accordingly; artifact still produced.
- Ganda-side pieces go in timewarp-ganda tasks; list their ids under `## Notes` and block on them explicitly.
- Prove it: one PR where CI skips the pipeline on an attested tree and finishes in under 1 minute, and one where a
  non-attested push still runs everything. Record both run ids under `## Results`.
- Local `dev e2e` already runs without prompts (task 149, done 2026-11-07).
- Run `ganda repo audit --fix` in this worktree and commit whatever it changes (audit fixes are policy).

## Goal (implementer restatement)

Cut CI wall time (5.5 to 7 min per run) by letting CI trust a signed local pipeline run. A local `dev ci` runs
build, test, e2e and verify-samples, then attests HEAD's tree with a payload that names the passed stages and the
tool versions. The workflow checks that attestation before `Run CI Pipeline`: on a match it only packs the
`Packages-*` artifact (and skips or feeds the coverage upload); on a miss it runs everything as today.

Acceptance: an RFC under `rfc/` settles the seven design questions; the ganda-side tasks exist in timewarp-ganda and
are linked; `dev ci` attests with a stage payload; the workflow gate skips on match and runs fully on miss; the
artifact is still produced; tool-version drift refuses the fast path; two proving CI runs (fast < 1 min, slow
unchanged) are recorded under `## Results`; `ganda repo audit --fix` changes are committed.

Implement-walk finding (2026-11-17): no `rfc/` folder exists yet and `ganda repo attest` has no stage-payload
option (only `[--force] [--no-push] [--no-status]`, `status`, `keygen`, `key-show`). The task says it is RFC-first
and that a plain implement walk cannot finish, so this walk parks as Blocked until
`ganda task work 150 --routine rfc` writes the RFC and creates the timewarp-ganda tasks.

## Checklist

- [x] RFC written under `rfc/` with the seven decisions resolved
- [x] Ganda-side tasks created in timewarp-ganda and linked
- [ ] `dev ci` runs all stages locally and attests with stage payload
- [ ] Workflow gate reads attestation; skips pipeline on match; full run on miss
- [ ] `Packages-*` artifact still produced on the fast path
- [ ] Tool-version drift refuses the fast path
- [ ] Two proving runs recorded (fast path < 1 min, slow path unchanged)
- [x] `ganda task work` runs the local pipeline + attest before open-pr (or follow-up task linked)
- [ ] `ganda repo audit --fix` run and its changes committed
- [ ] `## Results` filled in

## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-07)
- Context: Steve: "CI takes forever and it slows up all of our agents"; wants local run + attestation to skip CI work across all builds (2026-11-07)
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 98240814-df7b-428f-ad99-3daf2845663f, max-turns 500, Blocked — 2026-11-17T12:26:55Z
- RFC oracle: grok session 01a1e986-6767-78a3-b95e-36f8a1bf1041 (2026-11-17). Ballot in `rfc/rfc.md`. Fold-in is the next compute on this id.

## Notes

- Task 149 (the former prerequisite) is done.
- Groomed 2026-11-16: dropped the finished 149 dependency, added the coverage-upload question, refreshed timing,
  and marked the task RFC-first.
- Timing source: `gh run view` on the master run for 51e8e997 (task 148 merge), 2026-11-07.
- RFC: `rfc/rfc.md`. Resolved 2026-11-17. Third option on the artifact: `refs/ci-packages/<treeSha>`, not a draft GitHub Release. Second key `tw-ci-1`. Tree lookup. Coverage step skipped on the fast path.
- timewarp-ganda 660: CI stage note and `tw-ci-1`. Fold-in of `dev ci` waits on that merge.
- timewarp-ganda 661: task-work `localCi` after the last kitchen commit and before the branch push CI sees. Stage failure fails the walk. A publish or auth failure warns and still opens the pull request.

## Blocked

Resolved 2026-11-18. The RFC is written under `rfc/` (ballot resolved, commit ec381102). timewarp-ganda tasks 660
(`ganda repo ci-note write`/`status`) and 661 (walk hook) are done and the installed ganda (1.0.0-beta.38) ships
`ganda repo ci-note`. The implement walk can proceed: wire `dev ci` to `ganda repo ci-note write`, add the workflow
gate using `ganda repo ci-note status`, and follow the decisions in `rfc/rfc.md`.
