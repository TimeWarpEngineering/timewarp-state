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

Cut CI wall time (5.5 to 7 min per run) by letting CI trust a local pipeline run on the same tree. Under the
no-signing override below: `dev ci` runs assert-version-ssot, clean, build, test, e2e and verify-samples, pushes the
packed nupkgs to `refs/ci-packages/<tree>`, and writes the plain JSON note on `refs/notes/ganda-ci` with
`ganda repo ci-note write` (stages, tools `sdk`/`rid`/`playwright`/`chromium`, package sha256s). A workflow step
before `Run CI Pipeline` reads that note for `HEAD^{tree}`; on a hit (all stages pass, SDK and RID match, every
package hash matches) it restores the packages and skips the pipeline and the coverage upload, and `Upload Artifacts`
still publishes `Packages-*`. On any miss it stays green and the full pipeline runs as today.

Acceptance: `dev ci` exists and publishes; the gate skips on a hit and runs fully on a miss; the artifact is still
produced; SDK/RID drift refuses the fast path; `localCi` in `.timewarp/dev.jsonc` lets `ganda task work` run it;
`ganda repo audit --fix` changes committed and the audit green; proving runs recorded under `## Results`.

Implement-walk finding (2026-11-17): no `rfc/` folder exists yet and `ganda repo attest` has no stage-payload
option (only `[--force] [--no-push] [--no-status]`, `status`, `keygen`, `key-show`). The task says it is RFC-first
and that a plain implement walk cannot finish, so this walk parks as Blocked until
`ganda task work 150 --routine rfc` writes the RFC and creates the timewarp-ganda tasks.

## Override: no signing (Steven 2026-11-17)

Steven overrode the ballot's signing decisions ("Is all this shit worth it? We're trying to make CI faster", then
"rewrite it, make it the simple one"). Everyone who can push can also edit the workflow, so a signature protects
nothing here. This overrides every RFC resolution that names a key, a signature, `tw-ci-1`, or
`ganda repo attest` for stage claims:

- No signing and no second key. The note is a plain, unsigned JSON git note on `refs/notes/ganda-ci`, keyed by tree.
- timewarp-ganda 660 was rewritten to that: a ganda command (suggested `ganda repo ci-note write|status`) that writes
  `{tree, stages, tools, packages, ts}` and pushes it, and a `status` that exits 0 only when the `--require`d stages
  all passed and prints the tools. Use the command names 660 ships.
- `dev ci` runs the stages, then calls 660's `write`. The workflow gate calls 660's `status` with the required
  stages and compares SDK and RID from its tools output. Everything else in the RFC (artifact, tree lookup, coverage
  skipped on the fast path) stands.
- 661 (task-work runs `localCi` before the push) is unchanged.

## Checklist

- [x] RFC written under `rfc/` with the seven decisions resolved
- [x] Ganda-side tasks created in timewarp-ganda and linked
- [x] `dev ci` runs all stages locally and writes the unsigned stage note (ganda 660)
- [x] Workflow gate reads the stage note (same contract as ganda 660 `status`; see Results for why not the binary); skips pipeline on match; full run on miss
- [x] `Packages-*` artifact still produced on the fast path
- [x] Tool-version drift refuses the fast path
- [ ] Two proving runs recorded (fast path < 1 min, slow path unchanged) — local proof recorded; GitHub run ids need the PR (later host node)
- [x] `ganda task work` runs the local pipeline + attest before open-pr (or follow-up task linked)
- [x] `ganda repo audit --fix` run and its changes committed
- [x] `## Results` filled in

## Session

- Created: ffba6066-c22c-42a7-95c8-de75506a6687 (2026-11-07)
- Context: Steve: "CI takes forever and it slows up all of our agents"; wants local run + attestation to skip CI work across all builds (2026-11-07)
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 98240814-df7b-428f-ad99-3daf2845663f, max-turns 500, Blocked — 2026-11-17T12:26:55Z
- RFC oracle: grok session 01a1e986-6767-78a3-b95e-36f8a1bf1041 (2026-11-17). Ballot in `rfc/rfc.md`. Fold-in is the next compute on this id.
- Implement oracle (fold-in): claude-opus-5-5, 2026-11-18. `dev ci`, pwsh gate, `localCi`, docs, tests; filed task 161 for the Amuru/Nuru pin migration the audit fix exposed.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-18T13:25:28Z

## Notes

- Task 149 (the former prerequisite) is done.
- Groomed 2026-11-16: dropped the finished 149 dependency, added the coverage-upload question, refreshed timing,
  and marked the task RFC-first.
- Timing source: `gh run view` on the master run for 51e8e997 (task 148 merge), 2026-11-07.
- RFC: `rfc/rfc.md`. Resolved 2026-11-17. Third option on the artifact: `refs/ci-packages/<treeSha>`, not a draft GitHub Release. Second key `tw-ci-1`. Tree lookup. Coverage step skipped on the fast path.
- timewarp-ganda 660: unsigned CI stage note (rewritten 2026-11-17, no `tw-ci-1`). Fold-in of `dev ci` waits on that merge.
- timewarp-ganda 661: task-work `localCi` after the last kitchen commit and before the branch push CI sees. Stage failure fails the walk. A publish or auth failure warns and still opens the pull request.
- timewarp-state 161: migrate the dev CLI to Amuru 3.0.0-beta.10 typed builders and Nuru 4.0.0-beta.16; removes the
  four `package-pin-currency.hold` entries this task added.

## Results

### What shipped

- `tools/dev-cli/endpoints/ci-command.cs`: `dev ci`. Clean-tree check, PR stages, second clean-tree and HEAD check,
  push `refs/ci-packages/<tree>` (git plumbing; `*.nupkg` only), then `ganda repo ci-note write --from
  artifacts/ci/ci-note-input.json`. Exit 0 published, 1 stage failed or dirty, 2 publish failed (timewarp-ganda
  `local-ci.md` contract).
- `tools/dev-cli/endpoints/workflow/pr-pipeline.cs`: the stage list, now shared by `dev workflow` and `dev ci`.
- `tools/dev-cli/endpoints/workflow/ci-note-input.cs`: pure note-input builder (stage ids, tools, packages, RID,
  Chromium revision, sha256), linked into `timewarp-state-tests`.
- `scripts/ci-fast-path-gate.ps1` + workflow step `Local CI fast path` (pull_request and push only).
  `Run CI Pipeline` and `coverage` skip when `fast_path == 'true'`; `Upload Artifacts` unchanged.
- `.timewarp/dev.jsonc`: `localCi = dotnet run --file tools/dev-cli/dev.cs -- ci` (timewarp-ganda 661 hook).
- `documentation/dev-ops/local-ci.md` (+ toc, dev-ops.md link).
- `tests/timewarp-state-tests/release-pipeline/ci-note-input-tests.cs` (6 tests, one pins the gate's stage list).

### Deviations from the override text

- The gate does not run `ganda repo ci-note status`: timewarp-ganda is a private repo with no release binary for
  1.0.0-beta.38 (latest GitHub release v1.0.0-beta.23, no assets), so the runner cannot install it. The gate reads
  the same note with git + pwsh and applies the same rule (every required stage `pass`), plus SDK/RID and package
  hashes. Locally `ganda repo ci-note status` and the gate agreed on tree 40f17485 (both pass).
- The gate is pwsh, not bash: the `workflow-pwsh-shell` audit check refuses bash steps.
- No "PR touches workflow.yml → full run" rule: that fall-through existed only to pin the signing key (resolution 2),
  which the override removed.

### Boy-scout (audit)

- `ganda repo audit --fix` bumped TimeWarp.SourceGenerators 1.0.0-beta.16 → 1.0.0-beta.17 (kept) and Amuru /
  Amuru.Tools 3.0.0-beta.5 → beta.10, Nuru / Nuru.DevCli 4.0.0-beta.15 → beta.16. The last four broke the build:
  17 AMURU0001/0002 errors in `tools/dev-cli/endpoints/` and CS0246 `GhReleaseCreateBuilder` in DevCli beta.16
  `release-guard.cs` linked into `timewarp-state-tests`. Reverted those four and added `package-pin-currency.hold`
  entries naming task 161 under `[ganda.audit]` in `.editorconfig`.

### Gates

- `./bin/dev build` (after `dotnet run --file tools/dev-cli/dev.cs -- self-install`): Build succeeded (first run
  failed TW0007 on test-file usings; fixed by nesting the test namespace and a global using; rerun green).
- `tests/timewarp-state-tests/release-pipeline/ci-note-input-tests.cs`: not a Jaribu runfile (Fixie project); ran
  `dotnet fixie timewarp-state-tests --tests "*CiNoteInputTests*"`: 6 passed. CI runs the full suite.
- `ganda repo audit --fix`: passes all checks after the fix; rerun `ganda repo audit` after the pin holds: passes all
  audit checks.

### Local proof (2026-11-18)

- `dotnet run --file tools/dev-cli/dev.cs -- ci` on commit 357d2187 (tree 40f1748559f9df10815acfe7ba217e4c2b92aa76):
  exit 0 in 332 s. Pipeline SUCCEEDED; tools chromium=1248, playwright=1.64.1, rid=linux-x64, sdk=11.0.100; pushed 9
  packages to `refs/ci-packages/40f17485…` (origin 5ee4c733) and `refs/notes/ganda-ci` (origin 8f2b7e44).
- Hit: `pwsh -File scripts/ci-fast-path-gate.ps1` on that tree: `CI fast path: hit … 9 package(s) restored`,
  `fast_path=true`, 2.7 s. All 9 restored nupkgs byte-identical (`cmp`) to the local packs; `.snupkg` excluded.
- Miss: same script in a throwaway clone at 2c8722ac (tree bd7b2aa4): `miss (no CI note for tree bd7b2aa4…)`, exit 0,
  `artifacts/packages` left absent so the pipeline packs.
- GitHub proving runs: not yet. The pull request is a later host node. With 661, `ganda task work` runs `localCi`
  before the open-pr / land-pr push, so that PR's CI should take the fast path when its merge tree equals the head
  tree. Record the fast run id and a full (miss) run id here at the merge gate.

### Risk for review

- Tree lookup (resolution 4) hits only when `refs/pull/N/merge` (and the master merge commit) has the same tree as
  the head `dev ci` ran on, i.e. the branch already contains master. Each walk lands `land kanban <id>` commits on
  master, so master often moves after `dev ci`; then both PR and master runs miss and run in full (correct, just no
  saving). If the proving runs show that, a follow-up could accept a head-tree note when the merge differs from the
  head only under `kanban/`.

### How to validate

Smoke:

```bash
git status --short                                   # empty
dotnet run --file tools/dev-cli/dev.cs -- ci; echo $? # 0; prints "CI note written and pushed for tree <tree>"
ganda repo ci-note status --require assert-version-ssot,build,test,e2e,verify-samples
rm -rf artifacts/packages && pwsh -File scripts/ci-fast-path-gate.ps1
```

Expect: `dev ci` exits 0 and pushes `refs/ci-packages/<tree>` and `refs/notes/ganda-ci`; `status` exits 0 with all
five stages pass; the gate prints `CI fast path: hit` and `fast_path=true` and restores the `*.nupkg` files. Any
commit that changes the tree, then the gate again: `CI fast path: miss (no CI note for tree …)`, exit 0. On GitHub, a
hit run shows `Local CI fast path` hit, `Run CI Pipeline` and `coverage` skipped, `Upload Artifacts` green, job under
one minute; a miss run shows the full pipeline as before.

## Blocked

2026-11-17: This task needs its RFC first. Run `ganda task work 150 --routine rfc` to write rfc/ with the seven decisions and create the linked timewarp-ganda tasks (stage coverage in the attestation, running the local pipeline before open-pr). Then run the implement walk again.

Cleared 2026-11-17 by the RFC walk. `rfc/rfc.md` is resolved. timewarp-ganda tasks 660 and 661 are in `kanban/to-do/`. Fold-in on this id still waits on 660 before `dev ci` can call the attest command. 661 is the walk hook and is not required for one manual proof.

Resolved 2026-11-18. timewarp-ganda 660 and 661 are done and the installed ganda (1.0.0-beta.38) ships
`ganda repo ci-note write|status`. The implement walk can proceed under the no-signing override above: wire `dev ci`
to `ganda repo ci-note write`, add the workflow gate on `ganda repo ci-note status`, keep the rest of `rfc/rfc.md`.
