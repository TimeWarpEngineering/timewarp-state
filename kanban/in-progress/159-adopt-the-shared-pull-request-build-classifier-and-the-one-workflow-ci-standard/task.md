# Adopt the shared pull request build classifier and the one-workflow CI standard

## Description

Bring timewarp-state to the org CI standard (ganda 476 / 479 / 565 / 587, audited by ganda 655): `workflow.yml` has
no `paths` / `paths-ignore` filter on `pull_request`, so every pull request reports `CI`, and `dev workflow`
calls the shared classifier `PullRequestBuildClassifier` from `TimeWarp.Nuru.DevCli` 4.0.0-beta.15
(timewarp-nuru 595) to skip build and test on a pull request that touches no build path (kanban, docs, ...).

Tracked by timewarp-ganda 656 (architecture reposet rollout).

Survey 2026-11-16 for this repo: has a `pull_request` paths filter; `dev workflow` does not classify. Work: remove the filter, add the classifier call.

## Goal (implementer restatement)

Every pull request in timewarp-state must report a `CI` check, so `workflow.yml` drops its `pull_request`
paths filter. Build cost on doc/kanban-only pull requests is avoided in C# instead: `dev workflow` asks the
shared `PullRequestBuildClassifier` (TimeWarp.Nuru.DevCli 4.0.0-beta.15) first and, on a skip, prints its
message and exits 0 (and the workflow's coverage/package upload steps skip with it). Push keeps its filter
(with a why comment) so a kanban-only merge does not rebuild and upload a Packages artifact. Release is unchanged.
Files outside the classifier's protected roots that are packed (root `readme.md`, `assets/logo.png`) are
declared as `buildPaths` in `.timewarp/dev.jsonc`; no local copy of the rule. Done when `ganda repo audit`
passes both CI-standard checks (`workflow-file`, `dev-workflow-classifier`), build and tests are green, and
the pull request shows a `CI` run.

## Requirements

1. Run `ganda repo audit --fix` first and commit what it changes.
2. Bump the `TimeWarp.Nuru.DevCli` pin to `4.0.0-beta.15` (and `TimeWarp.Nuru` to the same version if pinned).
3. In the `workflow` command's pull-request path, call `PullRequestBuildClassifier.DecideAsync` first. On skip,
   print its message and exit 0. Push and release paths are unchanged.
4. Remove `paths` / `paths-ignore` from the `pull_request` trigger of `.github/workflows/workflow.yml`. Keep any
   `push` filter and add a comment saying why (a kanban-only merge must not rebuild and upload packages).
5. If this repo builds from roots outside the classifier defaults (`source`, `tests`, `tools`, `msbuild`), declare
   them through the classifier's config (`PullRequestBuildConfig`), not by copying or editing the rule.
6. No local copy of the classification rule remains in `tools/dev-cli/`.
7. `ganda repo audit` passes, including both CI-standard checks from ganda 655.

## Checklist

- [x] `ganda repo audit --fix` run and committed
- [x] DevCli pin at 4.0.0-beta.15
- [x] `dev workflow` calls the shared classifier on pull requests; skip exits 0 with its message
- [x] No `paths` filter on `pull_request`; `push` filter kept with a why comment
- [x] Extra build roots (if any) declared through config
- [x] `ganda repo audit` clean
- [ ] This task's pull request shows a `CI` run (record the URL in Results)

## Notes

State builds from roots outside the defaults (for example `samples/**`, `scripts/**`); check the current `pull_request` filter and declare those through `PullRequestBuildConfig`.
After merge, the ganda 656 tracker proves a kanban-only pull request here reports `CI` success and merges with
plain `ganda pr merge` (no `--allow-no-ci`).

## Results

- `ganda repo audit --fix` (run first): changed nothing; the only failures were the two CI-standard checks
  (`workflow-file`, `dev-workflow-classifier`), fixed by this task.
- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` 4.0.0-beta.14 → 4.0.0-beta.15.
- `tools/dev-cli/endpoints/workflow-command.cs`: `RunPrAsync` first calls
  `PullRequestBuildClassifier.DecideAsync(repoRoot, ct)` and prints the message; on a skip it writes
  `requires_build=false` to `GITHUB_OUTPUT` and returns success (exit 0). Release path unchanged.
- `.github/workflows/workflow.yml`: `pull_request` has no `paths` filter (comment says why). `push` keeps its
  filter with a why comment, plus `readme.md` and `assets/logo.png` (both packed). The `Run CI Pipeline` step
  has `id: pipeline`; the `coverage` and `Upload Artifacts` steps skip when `requires_build == 'false'`, so a
  skipped pull request does not fail on `if-no-files-found: error`.
- `.timewarp/dev.jsonc` (new): `pullRequestBuild.buildPaths` = `readme.md` (packed into TimeWarp.State) and
  `assets/logo.png` (package icon). `samples/` and `scripts/` already build under the default rule (they are not
  declared non-build), so they need no entry. No local copy of the rule in `tools/dev-cli/`.

### Gates

| Gate | Result |
|------|--------|
| `./bin/dev build` | PASS (0 warnings, 0 errors) |
| `dotnet run --file tools/dev-cli/dev.cs -- self-install` (dev CLI changed) | PASS |
| `./bin/dev test` | PASS after clearing a stale `~/.dotnet/toolResolverCache` (first run: "Run dotnet tool restore" for fixie, a known environment issue). analyzer 90, source generator 174, state 153, plus 40, telemetry 21, devtools 124, isolation 4, client integration 67, architecture 7, all passed |
| `ganda repo audit` | PASS ("Repository passes all audit checks"; `workflow-file` and `dev-workflow-classifier` PASS) |

### How to validate

Smoke (kanban-only pull request skips):

```bash
T=$(mktemp -d); B=$(git merge-base HEAD origin/master)
printf '{"pull_request":{"base":{"sha":"%s"},"head":{"sha":"%s"}}}' "$B" <kanban-only-commit> > $T/event.json
GITHUB_EVENT_NAME=pull_request GITHUB_EVENT_PATH=$T/event.json GITHUB_OUTPUT=$T/out ./bin/dev workflow; echo $?; cat $T/out
```

Expect: `Pull request changes only declared non-build paths. Skipping build and test. ...`, exit 0, and
`$T/out` holds `requires_build=false`. Ran this against the kanban-only commit `caae4ff4`: got exactly that.
With a head that changes `tools/` (this branch's HEAD) the classifier should print `Build path changed: ... Full build.`
and run the pipeline (not run locally, since it cleans the worktree; the pull request's CI run proves it). On GitHub: this task's pull request shows a `CI` run (it changes `tools/`, so it builds).

## Session

- Created: 64677b22-d705-4b25-b537-4b53732bcb92 (2026-11-16)
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 61873cfa-2718-4ad5-9cde-5fa50ae23985, max-turns 500, Done — 2026-11-16T17:43:01Z
