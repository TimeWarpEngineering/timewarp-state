# RFC: Attestation-gated CI skip for TimeWarp.State

**Status:** resolved 2026-11-17. Pending fold-in on task 150. Not product authority. The resolutions section is what fold-in implements. Author leans above it are the ballot, including leans the tally rejected.
**Host task:** 150
**Date:** 2026-11-17

This file is debate material. It is not a rule, a Design region, or a workflow contract until the accepted lines are folded into product code on task 150. Reviewers vote on the seven decisions. Do not treat an author lean as shipped behavior.

## Why this exists

Master and pull-request CI for this repo spends about five to seven minutes in `Run CI Pipeline`. Agent walks wait on that job at the merge gate. Steve's direction (2026-11-07, task 150) is to run the pipeline locally, sign the result, and let GitHub Actions skip work a trusted signature already covers. The saving has to apply to build and test, not only e2e.

A plain implement walk cannot finish first: `ganda repo attest` signs an audit check-set only, and the stage payload, the trust story, and the task-work hook do not exist yet.

## Out of scope

- Editing the frozen v1 audit note (`refs/notes/ganda-audit`) or its canonical bytes.
- Signing with the files under `~/.timewarp/ganda/keys/` except through a ganda command.
- Rebuilding the release promotion path. Release still downloads a `Packages-*` artifact from a successful `workflow.yml` run for the commit.
- Coverage comments, a coverage badge, or any consumer of the `coverage` artifact. This repo only uploads that artifact.
- A pack-only third mode. The gate is binary: trusted fast path, or today's full pipeline.
- Implementing the ganda commands in this ballot. Those are timewarp-ganda tasks linked from task 150. Fold-in on this task starts after those tasks exist and, for the parts that call them, after they have merged.

## Time saved

Wall clock is job `createdAt` to `updatedAt` on `workflow.yml` runs. The 35s Dependabot run 43978753085 is a different workflow (`event: dynamic`) and is not a pipeline sample.

| Run | Wall | Event | URL |
|-----|------|-------|-----|
| 44076437420 | 5m13s (2026-11-17T11:42:34Z to 11:47:47Z) | push to master | https://github.com/TimeWarpEngineering/timewarp-state/actions/runs/44076437420 |
| 43924026362 | 7m02s (2026-11-16T17:59:01Z to 18:06:03Z) | push to master | https://github.com/TimeWarpEngineering/timewarp-state/actions/runs/43924026362 |
| 43814858660 | 5m28s (2026-11-16T06:09:36Z to 06:15:04Z) | push to master | https://github.com/TimeWarpEngineering/timewarp-state/actions/runs/43814858660 |
| 43808921505 | 5m35s (2026-11-16T05:14:26Z to 05:20:01Z) | push to master | https://github.com/TimeWarpEngineering/timewarp-state/actions/runs/43808921505 |

Step split on run 44076437420, job `CI`: checkout 2s, Setup .NET 1s, `Run CI Pipeline` 5m01s (11:42:41Z to 11:47:42Z), `coverage` upload 1s, `Upload Artifacts` 1s. Log lines inside that step: pipeline start 11:43:04Z, e2e cleanup marker 11:45:48Z, e2e success 11:46:55Z, `Pipeline SUCCEEDED` 11:47:41Z. That is about 2m44s of build+test, 67s of e2e, and 46s of verify-samples. The earlier kitchen figure for 51e8e997 (5m34s, with ~3m restore+build+unit tests, ~70s e2e, ~50s verify-samples) matches this shape.

Skipping the pipeline step removes about five minutes. The fast path still pays checkout, setup, a note fetch, a package download, and the existing upload (a few seconds on this run, plus download time). The under-one-minute proof is that path. A from-scratch compile does not fit: build+test alone was 2m44s, and this RFC has no isolated cold-pack timing that would put compile inside the leftover minute.

## Evidence

| Claim | Where |
|-------|--------|
| PR/merge pipeline is classify, assert-version-ssot, clean, build, test, e2e, verify-samples. verify-samples packs. Release promotes `Packages-*` and does not rebuild. | `tools/dev-cli/endpoints/workflow-command.cs` Design region; `tools/dev-cli/endpoints/workflow/ci-artifact-service.cs`; `documentation/dev-ops/releasing.md` |
| Workflow runs that pipeline in one step, then uploads `artifacts/coverage` (`if-no-files-found: error`) and `Packages-<run_number>` from `artifacts/packages/*.nupkg`. Both skip when `requires_build` is `false`. | `.github/workflows/workflow.yml` |
| Coverage files are produced by the e2e test run. Nothing in this repo downloads the `coverage` artifact. | `scripts/e2e.cs`, `scripts/e2e-tests.cs`, workflow above |
| `ganda repo attest` signs `tree` + `check_set` into `refs/notes/ganda-audit`, keyed by tree SHA. `check_set` is the SHA-256 of the ganda assembly version plus sorted audit check ids. The v1 canonical bytes are `v1`, `ed25519`, tree, check_set, timestamp, key id, with no trailer newline. Extra JSON fields are not signed. | timewarp-ganda `attestation-note.cs`, `attestation-payload.cs`, `audit-check-set.cs`, `git-notes-service.cs` |
| Default key id is `tw-audit-1`. Private material lives under the ganda home (`audit-ed25519.pem` and sidecars). | timewarp-ganda `attestation-key-store.cs` |
| Notes are fetched and pushed as `refs/notes/ganda-audit`. `actions/checkout` does not fetch notes. Ganda's own verifier fetches the notes ref, then reads `HEAD^{tree}`. | `git-notes-service.cs`; timewarp-ganda `tools/dev-cli/endpoints/workflow/attestation-note-verifier.cs` |
| Attest is allowed on master, main, and origin-tracking branches, including a task branch before its first push. | timewarp-ganda `attestation-branch-policy.cs` |
| There is no `dev ci` route. The installed `ganda repo attest` surface is the audit sign, `status`, `keygen`, and `key-show`. | `tools/dev-cli/endpoints/` (no ci command); `ganda repo attest --help` on 2026-11-17 |
| SDK pin is `11.0.100` with `rollForward: latestMinor`, so the tree does not fix the patch that actually ran. | `global.json` |
| Playwright pin is `Microsoft.Playwright.MSTest.v4` `1.64.1`. The browser revision is read from the built test output's `browsers.json`, not from a separate repo file. | `Directory.Packages.props`; `scripts/e2e-build.cs` |
| Release tags are `refs/tags/v{version}` only. | `tools/dev-cli/endpoints/workflow/release-git-gate.cs` |
| Task-work already has an `audit` node and an `open-pr` node. It does not run this repo's pipeline. | timewarp-ganda `task-work-compute.cs`, `task-transition-edge.cs` |
| Workflow permissions are `contents: read`, `id-token: write`, `actions: read`. | `.github/workflows/workflow.yml` |
| Local e2e already runs without prompts. | task 149, done |

## Facts, not votes

- v1 audit bytes are frozen. Putting stage names into `check_set` changes a hash that means "these audit checks ran on this ganda build", and a verifier recomputes that hash from the audit catalog. Stage ids would either break verification or be invisible inside the hash.
- A field that is not in the canonical bytes is unsigned. A sibling JSON property on the v1 note cannot carry trust.
- The private audit key is not a signing tool for this repo. State code calls a ganda command or it does not sign.
- The fast-path job must not `dotnet run` the dev CLI. That compile sits inside the five minutes the skip exists to avoid. Verification has to be `git` plus `openssl` (or a checked-in script that does not restore the product).
- The public key used by the gate cannot be "whatever this pull request says" if the same pull request is allowed to take the fast path. A pull request that changes the pin or the workflow runs the full pipeline.
- `git notes` are keyed by object id. Ganda passes the tree SHA. A later commit with the same tree, including a merge commit whose tree equals the attested pull-request head, sees the same note. A merge that produces a different tree does not.
- `dev ci` must not create a commit. A commit would change the tree the note just signed.

## Decisions

### Decision 1 — Where stage coverage lives

**Options**

- A. Extend `check_set` with `build`, `test`, `e2e`, `verify-samples`.
- B. A new git note, `refs/notes/ganda-ci`, written only by a new ganda command. The v1 audit note stays as it is. The note is pushed to this repo's origin the same way `refs/notes/ganda-audit` is. Schema lives in timewarp-ganda, not in a state-only format.
- C. A free-form note in this repo signed by a local openssl call against the org key.

**Trade-offs**

A collides with a frozen hash. C is the forgery the org key rule forbids, and it duplicates verifier code. B leaves every current audit verifier untouched. Cost: a ganda task, a second notes ref to fetch, and a push from `dev ci`.

**Author lean:** B.

The signed body is a new canonical document, domain-separated from the audit payload (its own leading version line, not the audit `v1` line). Suggested fields, all inside the signature: `claim` = `ci-stages`, `tree`, `stages` (map of stage id to `pass`; the command signs only when every required stage is `pass`), `tools` (`sdk`, `os`, `rid`, `playwright`, `chromium`), `packages` (list of `{name, sha256}` for the nupkgs just packed), `ts`, `key_id`. No signature means the stage is not covered. Failed runs do not get a note.

Required stage ids, matching `RunPrAsync`: `build`, `test`, `e2e`, `verify-samples`. `verify-samples` is the stage that packs, so a pass here is what makes the nupkgs exist.

### Decision 2 — Trust boundary

**Options**

- A. Second key id `tw-ci-1`, separate PEM and sidecars under the ganda home, used only by the new command. Payload still carries `claim: ci-stages`.
- B. Same `tw-audit-1` key, with the claim field and a different canonical domain, on the new note ref only.

**Trade-offs**

B is one less key to back up, and domain separation already stops an audit verifier from accepting the CI bytes. A matches the written rule that a signature from the audit key means the audit passed: the CI claim never uses that key. The public half of `tw-ci-1` is committed at `.timewarp/ci-attestation.pub.pem` so the workflow can verify with openssl and no ganda binary on the runner. The private half stays behind the ganda command.

A pull request whose diff touches `.github/workflows/workflow.yml` or `.timewarp/ci-attestation.pub.pem` does not take the fast path. The gate compares those paths to the base branch and falls through to the full pipeline. That stops a pull request from swapping the pin and skipping the work in one change. Master already runs the workflow that master merged.

**Author lean:** A.

### Decision 3 — What CI still produces

**Options**

- A. Fast path runs `dev pack` on the runner and uploads `Packages-*` as today.
- B. Local `dev ci` uploads the nupkgs it already packed. The fast path checks the signed sha256 list, downloads those bytes, and the existing upload step publishes `Packages-<run_number>`.
- C. Skip the artifact on pull requests and pack only on master.

**Trade-offs**

A needs no upload token and keeps the bytes on the runner. It does not meet the under-one-minute proof: the compile is inside the 2m44s build+test window, and this RFC will not pretend an unmeasured pack is faster than that. C drops the artifact the release job promotes and contradicts the kitchen. B uses the `gh` credential that already pushes the task branch (`contents: write` locally). The workflow token stays `contents: read`, which can download a release asset in this private repo. Release lookup is unchanged: it still wants a successful `workflow.yml` run whose artifact name starts with `Packages-`.

Shape of B: after verify-samples, `dev ci` creates a draft GitHub release tagged `ci-packages/<treeSha>` (not `v*`; release-git-gate only looks at `refs/tags/v{version}`) and uploads the nupkgs. The note's `packages` list is exactly those files and their sha256. The workflow downloads the draft, checks every hash, writes `artifacts/packages`, and lets the current upload step run. It does not delete the draft in this task, because deletion wants `contents: write` on the workflow token. Drafts are leftover, not product tags. A later cleanup can remove them.

If the draft is missing, `gh` is not authenticated, or any hash mismatches, the note is not a fast-path note: `dev ci` does not record `packages`, and CI runs the full pipeline. There is no pack-only middle.

**Author lean:** B.

### Decision 4 — Branch and merge policy

**Options**

- A. Look up the note by commit SHA. Re-attest after every merge.
- B. Look up the note by `HEAD^{tree}`, the way audit notes already work. The same tree on a task branch, a pull-request head, a fast-forward, or a merge commit is covered. A merge whose tree differs is a miss and runs the full pipeline.

**Trade-offs**

A throws away the saving on every merge commit, which is the master run agents wait for after the pull request. B is already how `git notes show <tree>` behaves. No pull-request-head database is required. CI must `git fetch origin refs/notes/ganda-ci:refs/notes/ganda-ci` because checkout does not fetch notes. `fetch-depth: 0` stays.

**Author lean:** B.

### Decision 5 — Tool-version drift

**Options**

- A. Trust the tree. Tool versions are implied by `global.json` and `Directory.Packages.props`.
- B. Put the tools that actually ran in the signed payload. CI refuses the fast path on mismatch. Local `dev ci` refuses to sign when the browser it ran is not the revision in the built `browsers.json`.

**Trade-offs**

A misses `rollForward: latestMinor` and a stale Playwright cache. B records `sdk` from `dotnet --version`, `os` (`linux` or not), `rid`, `playwright` (the package version), and `chromium` (the revision). CI, after Setup .NET, refuses unless `sdk` equals its own `dotnet --version`, `os` is `linux`, `rid` is `linux-x64`, and `playwright` equals the version in `Directory.Packages.props`. The chromium check happens locally before signing, so the fast path does not restore Playwright. Kernel and Ubuntu image drift are accepted residual risk. The dev CLI source is inside the tree, so it has no separate version field.

**Author lean:** B.

### Decision 6 — Agent walks

**Options**

- A. Document that humans run `dev ci` by hand. Task-work stays unchanged.
- B. A timewarp-ganda task adds a task-work node after `audit` and before `open-pr`. The node runs only when the repo's `.timewarp/dev.jsonc` contains a `localCi` command. It does not commit. A non-zero exit fails the walk. Repos without the key skip the node.
- C. Run the pipeline from the post-commit hook on every commit.

**Trade-offs**

A misses the kitchen requirement that walks take the fast path by default. C adds five minutes to every commit, including audit-fix commits, and races the tree. B runs once, on the tree the audit node left behind. This repo's fold-in sets `localCi` to `dotnet run --file tools/dev-cli/dev.cs -- ci`. The ganda hook does not mention TimeWarp.State.

**Author lean:** B.

### Decision 7 — Coverage upload

**Options**

- A. Skip the `coverage` step when the fast path runs.
- B. Carry `artifacts/coverage` through the same draft release and upload it.

**Trade-offs**

B preserves an artifact nothing in this repo reads, and it adds files to the signed package list for no release consumer. A sets a step output `fast_path=true` and adds that condition to the coverage step's `if`. The upload step for `Packages-*` still runs. A missed fast path leaves both steps as they are today. Attested runs have no `coverage` artifact. That is the recorded loss.

**Author lean:** A.

## Ganda tasks this ballot asks for

Published to timewarp-ganda `kanban/to-do/` on 2026-11-17. Not launched from this walk. The contract is the resolutions section, not the author leans.

1. **Task 660 — CI stage attestation.** `ganda repo attest ci` (name to fit the existing attest group) writes `refs/notes/ganda-ci` with key id `tw-ci-1` on its own PEM. Payload matches resolution 1, 2, and 5. Verify exits 0 only for a matching tree, a valid signature, `claim: ci-stages`, and every required stage `pass`. Tool fields are returned so the caller can compare SDK and RID. No product repo reads the private key.
2. **Task 661 — task-work local CI.** Resolution 6. Run `.timewarp/dev.jsonc` `localCi` when present, after the last kitchen commit and before the branch push that CI will see, with the stage-failure versus publish-failure split.

Fold-in on task 150 (same id, later compute) is blocked on task 1 for the attest call. It can land `dev ci`, the workflow gate, and the public key file once task 1's contract is merged. Task 2 is the walk hook and is not required to prove one manual `dev ci` plus one pull request.

## Reviewer opinions

Ballots are copied from the sidecars. The author did not edit them.

### evidence-reviewer — 2026-11-17
**Overall:** Agree on a new note, a second key, tree lookup, signed tool versions, and skipping coverage; reject the draft-release artifact path and the pre-`open-pr` walk slot.
**Decision 1 (stage coverage):** B — `attestation-payload.cs` signs only `v1`, `ed25519`, tree, `check_set`, timestamp, and key id, with no trailing newline. `audit-check-set.cs` hashes the ganda assembly version plus sorted audit check ids, and `attest-surface-resolver.cs` recomputes that from the catalog, so stuffing stage ids into `check_set` either breaks `ganda repo attest` or is unsigned extra JSON. C would sign with the org audit key. The signed set must also include `assert-version-ssot`: `RunPrAsync` in `tools/dev-cli/endpoints/workflow-command.cs` fails the pipeline on it, and a fast path that skips the whole step would skip that gate. `build`, `test`, `e2e`, and `verify-samples` do match the later steps, and `verify-samples-command.cs` is what packs.
**Decision 2 (trust boundary):** A — `attestation-key-store.cs` defaults to key id `tw-audit-1` and `audit-ed25519.pem` (plus pub/hex/keyid sidecars) under the ganda home. Nuru's `attestation-verifier.cs` `KnownKeys` map contains only that id, and a signature from it means the audit passed. Domain separation is not a license to reuse the key. Any checked-in verifier script has to join `.github/workflows/workflow.yml` and the pem in the "diff falls through to the full pipeline" set; those two paths are not enough if the openssl steps live elsewhere.
**Decision 3 (artifact):** D, git ref not a GitHub Release — push the nupkgs to something `contents: read` can fetch (for example `refs/ci-packages/<treeSha>`), check the signed sha256 list, write `artifacts/packages`, and let the existing upload publish `Packages-<run_number>`. A missing ref or a hash mismatch falls through to today's pipeline; it must not fail the job. B is wrong: `gh repo view` shows this repo is public, not private, and GitHub lists draft releases only for push access while get-by-tag returns a published release. This workflow's token is `contents: read` (`.github/workflows/workflow.yml`). Publishing instead of drafting fires `on: release: types: [published]` in that same file, and the `release` event runs `dev workflow` as a NuGet promotion. `release-git-gate.cs` ignoring non-`v*` tags does not stop that trigger. A is a runner compile; run 44076437420 spent 11:42:41Z–11:47:42Z in `Run CI Pipeline`, and the task's proof is under one minute. C does not drop the promoted artifact: `tools/dev-cli/endpoints/workflow/artifact-source-walk.cs` calls `CiRunPromotion.OrderCandidateRuns`, which excludes `pull_request` runs (timewarp-nuru `source/timewarp-nuru-devcli/content/any/services/ci-run-promotion.cs`), so only the master push matters, and C still leaves that push compiling.
**Decision 4 (branch policy):** B — `git-notes-service.cs` stores and shows the note on the tree SHA, and `attestation-note-verifier.cs` fetches `refs/notes/ganda-audit` then reads `HEAD^{tree}`. Checkout does not fetch notes; `fetch-depth: 0` is already set. Default `pull_request` checkout is the merge ref (`workflow.yml` sets no `ref`; the promotion comment records that), so `HEAD^{tree}` matches the attested head only when the base added nothing. That miss is what B already requires. Commit-SHA lookup would also miss a same-tree merge.
**Decision 5 (staleness):** B — `global.json` is SDK `11.0.100` with `rollForward: latestMinor`, so the tree does not fix the patch. `Directory.Packages.props` pins `Microsoft.Playwright.MSTest.v4` at `1.64.1`. `scripts/e2e-build.cs` reads the Chromium revision from the built test output's `.playwright/package/browsers.json`, not from a repo file. Run 44076437420 logged `Playwright Chromium 1248` and installed it on the runner. Compare `sdk` to the runner's `dotnet --version` after Setup .NET; keep the browser check local so the fast path does not restore Playwright. `ubuntu-latest` matches the `linux` / `linux-x64` refuse rule.
**Decision 6 (agent walks):** localCi node, but after the land-pr commit and before its push — not the written B slot. The node rules (`.timewarp/dev.jsonc` `localCi`, no commit, non-zero fails the walk, absent key skips) are right, and task-work does have `audit` and `open-pr` and does not run this repo's pipeline. `open-pr` already pushes (`task-work-open-pr-node.cs`). `LandPrAsync` in timewarp-ganda `task-work-compute.cs` then commits a dirty kitchen (`task-work-kitchen-land.cs`) and pushes again. A note on the post-audit tree misses when that commit changes files. `kanban-done` is after human-merge, so it is not the slot. Push `refs/notes/ganda-ci` before the branch push CI will see.
**Decision 7 (coverage upload):** A — `scripts/e2e-tests.cs` collects XPlat coverage into `artifacts/coverage`, and the workflow uploads that path with `if-no-files-found: error`. Nothing in this repo downloads the `coverage` artifact. Release promotion wants `Packages-*` only. Keep the upload step in the file and add the fast-path condition; a miss must still upload. Attested runs having no coverage artifact is an acceptable recorded loss.
**Anything the author missed:** The repo is public. A draft is not readable with `contents: read`, and a published release retriggers NuGet promotion. `assert-version-ssot` is a real pipeline failure and is absent from the signed stage ids. `land-pr` can change the tree after `open-pr`'s push. The "C drops the release artifact" sentence is false for the reason in Decision 3. Nuru's `AttestationVerifier` does not recompute `check_set` from the catalog; it verifies the signature over the note's own field. The recompute that makes option A unsafe is `attest-surface-resolver.cs`. There is no `dev ci` route under `tools/dev-cli/endpoints/` (routes are build, test, e2e, pack, verify-samples, workflow, run). `ganda repo attest` is still only the audit sign, `status`, `keygen`, and `key-show`.

### second-reviewer — 2026-11-17
**Overall:** The shape holds (new note, second key, tree lookup, signed tools, skip coverage). Two leans do not: a draft release is not readable by this job's token, and the walk node is one push too early.
**Decision 1 (stage coverage):** B — `check_set` is the SHA-256 of `ganda-audit-check-set`, schema, assembly version, and the sorted audit ids (`audit-check-set.cs`). The v1 signature is those six lines and no trailing newline (`attestation-payload.cs`); a sibling JSON field is unsigned. A either breaks that frozen hash or hides the stages inside it. C signs with the org audit key outside a ganda command. B leaves every current audit verifier alone.
**Decision 2 (trust boundary):** A — A signature from `tw-audit-1` means the audit passed. A different canonical domain stops a verifier mix-up; it does not make that key available for a CI claim. `tw-ci-1` stays a separate PEM. The public half is committed so the runner can use `openssl` and no ganda binary. A pull request that touches the workflow or that pin runs the full pipeline.
**Decision 3 (artifact):** B's hashes, not a draft release — Keep the signed `{name, sha256}` list and the existing `Packages-<run_number>` upload. Do not create a GitHub Release. The releases API shows drafts only to callers with push access, and this job's token is `contents: read` (`workflow.yml`). A published release fires `on: release: types: [published]`, which runs the promote path. `dev ci` should push the nupkgs on a non-tag ref the read token can fetch (for example `refs/ci-packages/<treeSha>`), the job should check every hash, write `artifacts/packages`, and let the current upload step run. A missing ref or a bad hash discards those bytes and falls through to the full pipeline. A still spends the compile this skip exists to avoid. C drops the artifact `CiArtifactService` promotes.
**Decision 4 (branch policy):** B — Audit notes are already stored on the tree object (`git notes --ref=ganda-audit show <tree>`). Commit lookup misses a same-tree merge, which is the master push whose `Packages-*` artifact release will accept. `CiRunPromotion` drops `pull_request` runs because `actions/checkout` on that event uses `refs/pull/N/merge`, a different commit and sometimes a different tree. Lookup by `HEAD^{tree}` covers the merge ref when the tree really matches, and a different tree stays a full run. `fetch-depth: 0` stays.
**Decision 5 (staleness):** B — `global.json` is SDK `11.0.100` with `rollForward: latestMinor`, so the tree does not name the patch that ran. Playwright is `Microsoft.Playwright.MSTest.v4` `1.64.1`, and the browser revision is read from the built `browsers.json` (`e2e-build.cs`). A trusts pins the tree does not fix. B records `dotnet --version`, `linux`, `linux-x64`, and the Playwright package version, and the runner refuses a mismatch. The chromium check stays local, before signing, so the fast path does not restore Playwright.
**Decision 6 (agent walks):** B, but after `land-pr` — The `localCi` key, no commit, and a non-zero exit failing the walk are right, and repos without the key skip the node. A never makes walks fast. C adds the pipeline to every commit, including audit-fix commits. The bound order is `audit`, `open-pr`, `land-pr`, `human-merge` (`task-work-compute.tests.cs`). `land-pr` commits a dirty kitchen and pushes (`LandPrAsync`). That commit changes the tree. `PullRequestBuildClassifier` diffs the whole pull request against the base (`base...head`), so a kitchen tip on a source pull request still builds. A note taken before `open-pr` misses the commit the merge gate waits on. Run `localCi` after that push. This repo's value stays `dotnet run --file tools/dev-cli/dev.cs -- ci`.
**Decision 7 (coverage upload):** A — The `coverage` step uploads `artifacts/coverage` with `if-no-files-found: error`, and those files are written by the e2e run (`e2e.cs`). Nothing in this repo downloads them. B signs bytes no release consumer reads. A adds `fast_path=true` to that step only. The `Packages-*` upload still runs. This condition is load-bearing: a skipped pipeline leaves `requires_build` unset, which is not `'false'`, so without it the coverage step fails the job after the skip.
**Anything the author missed:** A failed notes fetch, including a missing `refs/notes/ganda-ci` on the first runs, has to be a miss and a full pipeline, not a red job. Match the audit verifier and fetch with a leading `+`. The fast path is only `pull_request` and `push`. `release`, `probe`, and `workflow_dispatch` keep today's step; release still promotes the master push artifact, so that push has to re-upload `Packages-*` when the tree matches. The pull request that edits `workflow.yml` or the pin cannot be the under-one-minute proof, because its own gate refuses the fast path; prove on a later pull request. `TimeWarp.Build.Tasks` embeds `git rev-parse HEAD` as assembly `CommitHash`, and `source/Directory.Build.props` sets `PublishRepositoryUrl` and `IncludeSymbols` with `snupkg`. The shipped nupkgs will name the `dev ci` commit, not the `v*` commit. Say that is accepted. The signed list should be the `*.nupkg` set the upload glob publishes; snupkgs are produced today and not uploaded. The same-repo pull request runs the workflow from its own head, so the path guard is not a boundary against a head that deletes the guard. Any checked-in verify script has to be in that path list too, or the check should be inline in `workflow.yml`. `tw-ci-1` must not follow `GANDA_AUDIT_KEY_PATH` or the audit PEM; today's `keygen --force` overwrites the single audit key and keeps its key id. Re-running `dev ci` for one tree has to replace the package ref or refuse to sign hashes the ref does not hold. A notes push failure is a non-zero `dev ci`.

### adversarial-reviewer — 2026-11-17
**Overall:** Do not stamp decision 3 or the "any non-zero fails the walk" half of decision 6. The second key is the trust-root rule, not what stops cross-protocol acceptance.
**Decision 1 (stage coverage):** B — `AuditCheckSet` hashes `ganda-audit-check-set`, schema, assembly version, and the sorted audit ids, and `TryVerify` rebuilds that hash from the catalog. Stage ids inside `check_set` either break verification or pretend to be audit checks. `AttestationPayload.BuildCanonicalBytes` signs only `v1`, `ed25519`, tree, check_set, timestamp, and key id, so a sibling JSON field is unsigned. A local openssl call against `tw-audit-1` is the forgery the key rule forbids. A new `refs/notes/ganda-ci` is the only option that leaves the frozen note alone.
**Decision 2 (trust boundary):** A — Domain separation already stops cross-protocol confusion. `TryVerify` rejects `v != 1` and checks the signature only against rebuilt v1 canonical bytes (`attestation-note-signer.cs`, `attestation-payload.cs`). A CI body with its own leading line, on another notes ref, will not verify as an audit pass. A second key is still required, because the org rule attaches the meaning to the key: a signature from `tw-audit-1` means the audit passed, and signing any other payload with it forges that sentence. B is safe against today's verifier and still the wrong use of the trust root. A is weaker than it looks in the implementation. `AttestationKeyStore` is one slot (`audit-ed25519.pem`); `Generate(force: true)` overwrites that PEM and only preserves a key id already in the sidecar. "keygen learns `tw-ci-1`" must be a different path, or the second key clobbers the audit key. The committed `.timewarp/ci-attestation.pub.pem` plus a path check is not a boundary against a bad pull request: `pull_request` runs the workflow from the merge commit, so the same diff can delete the check. That is the same trust CI has today. Do not describe the path check as why the pin is safe.
**Decision 3 (artifact):** `refs/ci-packages/<treeSha>`, not B — Run 44076437420 confirms the budget: wall 11:42:34Z–11:47:47Z, checkout about 2s, Setup .NET about 1s, `Run CI Pipeline` 11:42:41Z–11:47:42Z (5m01s), coverage and `Packages-*` upload about 1s each. Skipping that step can finish in under a minute. A runner compile cannot. Draft releases do not fit the token that has to download them. `workflow.yml` sets `contents: read`. The releases API lists drafts only for push access, and get-by-tag returns a published release. `contents: read` is not push. The author's claim that this token can download the draft asset is true for a published release, not for a draft. Publishing the draft so the job can see it creates the tag and fires `on: release: types: [published]`. `TagAssertion` would then abort because `GITHUB_REF_NAME` is not `v{version}`, so NuGet would not publish, but a release job would still start and fail. `ReleaseGitGate` only resolves `refs/tags/v{version}`, so a `ci-packages/` tag is not promoted; that part is true and is not enough. Whether `gh release create --draft` creates the git tag immediately is unsettled. The unreadability does not depend on it. The workflow also cannot delete the draft, so one release object remains per attested tree. A ref `refs/ci-packages/<treeSha>` is pushed with the same credential that pushes the branch. No tag, no release object, no `contents: write` on the workflow. `contents: read` can fetch that one ref. `actions/checkout` with `fetch-depth: 0` fetches branch history, not this namespace, so the measured checkout stays small. The cost is server-side blobs and larger mirror clones. This worktree has no `TimeWarp.State*.nupkg`, so the byte size is unmeasured; nine projects are packable. A blob over GitHub's 100MB limit would reject the push, and that would be a reason to move the bytes out of git after measuring, not a reason to keep an unreadable draft. Re-upload through the existing `Packages-*` step stays. Release promotion reads that Actions artifact and skips `pull_request` runs, so the push to master is the upload that matters.
**Decision 4 (branch policy):** B — Notes are already shown by tree SHA (`git-notes-service.cs`). A commit-SHA lookup misses every merge whose tree did not change, which is the master run the walk waits on when the pull request was current. Do not "fix" the lookup to `pull_request.head.sha`. For `pull_request`, checkout is `refs/pull/N/merge`. `CiRunPromotion` excludes those runs because that tree is not the head commit's tree. `HEAD^{tree}` on the runner is the right object. When master has moved, the merge tree differs, the note misses, and the full pipeline is the correct result.
**Decision 5 (staleness):** B, but compare `rid` to the runner — Refusing a non-linux signature is right: the only job is `ubuntu-latest`, and a Windows or macOS e2e pass does not cover it. A hardcoded `linux-x64` is too tight and too loose. Too tight: this box is `x86_64` with SDK `11.0.100`, so it matches today, but an arm runner or an arm dev box can never hit the fast path even when the signed RID is the runner's RID. Too loose: `linux-x64` does not pin glibc or the Ubuntu image, and the chromium revision is checked only before signing. The fast path does not restore Playwright. `os == linux` is redundant once the RID must be `linux-x64`. Compare the signed SDK to `dotnet --version` after Setup .NET (`global.json` is `11.0.100` with `rollForward: latestMinor`, so the tree does not name the patch). Compare the signed RID to the runner's RID. Keep the local browsers.json check. Do not also pin the image label; an image bump would invalidate every note, and the RFC is right to leave that as residual risk.
**Decision 6 (agent walks):** Split B — Fail the walk when `build`, `test`, `e2e`, or `verify-samples` fails. Warn and still open the pull request when the failure is `gh` auth, the notes push, or the package upload. The node sits after `audit` and before `open-pr`, and a throw there ends the walk the way the audit node already does. Decision 3 says an unauthenticated `gh` writes no package list and CI runs the full pipeline. That fallback never runs if `localCi` exits non-zero for the blip: no pull request, no CI, and a green full run was still the correct result. One exit code for test failure and upload failure makes the optimization a merge gate. Stage failure should still stop the walk. The five minutes would only confirm a red tree.
**Decision 7 (coverage upload):** A — The `coverage` step uses `if-no-files-found: error`, and its `if` treats a missing `requires_build` as "build happened" (`!= 'false'`). A skipped pipeline step does not set that output, `success()` stays true, and the job fails unless the step also requires the fast path to be false. The same trap sits on `Upload Artifacts`. Nothing in this repo downloads the `coverage` artifact. `ci-code-coverage` is a static check that the upload step exists; an `if:` does not remove it. `ci-code-coverage.min-line` is unset, so the floor that reads the latest `coverage` artifact is off. Carrying the files through the draft inherits decision 3's unreadable store. Skipping attested runs is the recorded loss. If a floor is set later, fast-path master runs will not feed it, and the reader will stick to an older run.
**Anything the author missed:** The fast-path flag has to be set by the gate step, and a failed download has to clear it before the pipeline `if` is evaluated. A miss that leaves the pipeline skipped and `artifacts/packages` empty fails the upload on `if-no-files-found: error`. The gate itself cannot be `dotnet run` of `dev workflow`; that compile is inside the 5m01s. `pull_request` HEAD is the synthetic merge, so a note for the branch tip misses whenever the branch is behind master. Decision 3's "full CI will run" and decision 6's "non-zero fails the walk" contradict each other on the agent path. `keygen` for `tw-ci-1` is not a flag on the current one-file store.

## Claim check before the tally

Re-read on 2026-11-17, after the ballots:

- The repo is public (`gh repo view`: `visibility: PUBLIC`). Decision 3's sentence that `contents: read` can download a draft release asset is false. Drafts need push access. A published release would fire `on: release: types: [published]` in `workflow.yml`.
- `CiRunPromotion.OrderCandidateRuns` excludes `pull_request` events. The sentence "option C drops the artifact release promotes" is too strong. Option C would still have to pack on the master push, which is the run release accepts. C remains rejected because that push would compile.
- Nuru `AttestationVerifier.Evaluate` verifies the signature over the note's own `check_set`. It does not recompute the hash from the audit catalog. The recompute is ganda `AttestSurfaceResolver` plus `AttestationService`, which compares the note to the catalog hash. The adversarial line that `TryVerify` rebuilds the hash from the catalog is not what `AttestationNoteSigner.TryVerify` does. Option A is still rejected: ganda's own attest would break, and Nuru would treat a stage hash signed by `tw-audit-1` as a valid audit note.
- `LandPrAsync` lands the kitchen and then pushes, between `open-pr` and the human-merge gate (`task-work-compute.cs`). A kitchen commit changes the tree. That part of the ballots is true.
- `assert-version-ssot` is a failing gate inside `RunPrAsync`, before the four named steps. Skipping the whole step skips it.

## Tally

| # | Topic | Evidence | Second | Adversarial | Status |
|---|-------|----------|--------|-------------|--------|
| 1 | stage coverage | B | B | B | 3–0 B. Stages also include `assert-version-ssot` (evidence; adopted below). |
| 2 | trust boundary | A | A | A | 3–0 A. Second key is a new PEM, not `keygen --force` on the audit key. |
| 3 | artifact | git ref | git ref | git ref | Third option. Draft release rejected. `refs/ci-packages/<treeSha>`. |
| 4 | branch policy | B | B | B | 3–0 B. Lookup is `HEAD^{tree}`. |
| 5 | staleness | B, fixed `linux-x64` | B, fixed `linux-x64` | B, compare RID to the runner | B stands. RID detail resolved below: compare to the runner, require a `linux` RID. |
| 6 | agent walks | after land-pr commit, before its push; any non-zero fails | after land-pr; any non-zero fails | before open-pr; split the exit | Slot and exit resolved below. |
| 7 | coverage upload | A | A | A | 3–0 A. Coverage skips on the fast path. Upload of `Packages-*` still runs. |

## Resolutions

Recorded 2026-11-17 by the RFC oracle after the tally. Fold-in on task 150 implements this section. It does not implement the rejected author lean on decision 3.

1. **Stage coverage.** New note `refs/notes/ganda-ci`, new canonical document, domain-separated from the audit `v1` bytes. `claim` is `ci-stages`. Required stages, all `pass` or there is no note: `assert-version-ssot`, `build`, `test`, `e2e`, `verify-samples`. The extra stage is in because `RunPrAsync` fails the pipeline when version SSOT fails, and the fast path skips that whole step. `verify-samples` is the stage that packs.

2. **Trust.** Key id `tw-ci-1` with its own PEM, public hex, and key-id sidecar. It must not read or write `audit-ed25519.pem`, and it must not honor `GANDA_AUDIT_KEY_PATH`. `keygen --force` on the audit key stays the audit key. The public PEM is committed at `.timewarp/ci-attestation.pub.pem`. The workflow verifies with `openssl` inline in `workflow.yml` (no `dotnet run`, no ganda install on the runner). A pull request whose diff touches `workflow.yml` or that PEM does not take the fast path. That fall-through is so a pin edit pays for a full run. It is not a new security property: a pull request can already replace the workflow, which is the trust CI has today.

3. **Artifact.** `dev ci` pushes the packed `*.nupkg` files (the upload glob, not the `.snupkg` files) to `refs/ci-packages/<treeSha>`, a commit whose tree is those files. The note's `packages` list is their names and sha256. The fast path fetches that one ref, checks every hash, writes `artifacts/packages`, and the existing upload step publishes `Packages-<run_number>`. A missing ref, a hash miss, or a notes-fetch miss (fetch with a leading `+`, same as the audit verifier) is a full pipeline, and the gate step stays green. No GitHub Release and no tag. Re-running `dev ci` for a tree replaces that ref or refuses to sign hashes the ref does not hold. Blobs linger on the server after a later delete; that cost is accepted. If a nupkg exceeds GitHub's blob limit, fold-in measures it and stops rather than inventing a third store in the same change. `CommitHash` and SourceLink inside those nupkgs name the commit `dev ci` packed, which is the attested tree's commit and may differ from a later same-tree merge commit. That is accepted.

4. **Branch policy.** The note is on the tree SHA. CI uses `git rev-parse HEAD^{tree}`. A merge commit or `refs/pull/N/merge` whose tree equals the attested tree hits. A different tree misses and runs the full pipeline. Do not look up `pull_request.head.sha`. The fast path applies only to `pull_request` and `push`. Release, probe, and `workflow_dispatch` keep today's steps. The master push is the run whose `Packages-*` artifact release will accept, because promotion skips `pull_request` runs.

5. **Staleness.** The note records `sdk` (`dotnet --version`), `rid` (the RID of the machine that ran), `playwright` (package version), and `chromium` (revision). `dev ci` refuses to sign when the Chromium revision it ran is not the revision in the built `browsers.json`. CI refuses the fast path unless `sdk` equals its `dotnet --version` after Setup .NET, and `rid` equals the runner's RID and starts with `linux`. No hardcoded `linux-x64`. Ubuntu image and glibc drift stay residual. The dev CLI source is in the tree and has no extra field.

6. **Agent walks.** timewarp-ganda runs the repo's `localCi` command from `.timewarp/dev.jsonc` when the key is present. Repos without it skip. The command does not commit. It runs on the tree that the walk is about to push, after the last kitchen commit, and it finishes the notes push and the `refs/ci-packages/<tree>` push before that branch push. `LandPrAsync` can commit after `open-pr` has already pushed, so a hook that only runs before `open-pr` misses the head the merge gate waits on. When `land-pr` will commit, run `localCi` after that commit and before its push (the earlier `open-pr` CI may be a full run; the new head is the fast one). When `land-pr` will not commit, run `localCi` before the `open-pr` push, because that push is the head. A failing stage (`assert-version-ssot`, `build`, `test`, `e2e`, `verify-samples`) fails the walk. A failure to authenticate, to push the note, or to push the package ref warns and still opens the pull request: CI then takes the full pipeline, which is the miss path decision 3 already requires. This split is the dissent on decision 6. The two other ballots wanted every non-zero exit to fail the walk. That would turn a `gh` blip into a blocked pull request even though a full CI run is the correct result. Stage failure still stops the walk, because the five minutes would only confirm a red tree.

7. **Coverage.** The gate step sets `fast_path=true` only after the note verifies and every package hash matches a file now in `artifacts/packages`. Any miss leaves `fast_path` unset and runs `Run CI Pipeline`. The `coverage` step also requires `fast_path != 'true'`. The `Packages-*` upload runs when `fast_path == 'true'` or when today's `requires_build` condition holds. A skipped pipeline leaves `requires_build` unset, which is not `'false'`, so without the coverage condition the job fails the upload of a missing coverage directory after a successful skip. Attested runs have no `coverage` artifact. The static `ci-code-coverage` check only requires the step to exist.

### Fold-in order

- timewarp-ganda 660 (resolutions 1, 2, and the verify contract): `dev ci` waits until that task has merged.
- timewarp-ganda 661 (resolution 6): the walk hook. Not required to prove one manual `dev ci` and one pull request.
- This repo, after the attest command exists: `dev ci`, `localCi` in `.timewarp/dev.jsonc`, the public PEM, and the workflow gate. The proving runs (fast path under one minute, and a miss that runs the full pipeline) are recorded under task 150 `## Results`. The pull request that edits `workflow.yml` or the PEM cannot be the fast-path proof.
