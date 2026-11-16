# Round 1 — general
**Date:** 2026-11-17
**Scope reviewed:** same as framework (product commit `e2715169` vs merge-base `0aaa1f92`)

## Summary

`workflow.yml` no longer filters `pull_request`, so every pull request runs the `CI` job. `RunPrAsync` calls `PullRequestBuildClassifier.DecideAsync` first, prints its message, and on a skip writes `requires_build=false` and returns success. The coverage and Packages uploads skip with that output. `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` are pinned to 4.0.0-beta.15. `.timewarp/dev.jsonc` adds `readme.md` and `assets/logo.png` as `buildPaths`. The push filter stays, with a why comment, and those two packed paths were added to it. Release still bypasses `RunPrAsync`. Risk is low. The recorded build, test, self-install, and audit results cover the change. One mismatch between the restatement and the task Notes is recorded under `## Learnings` and is not a finding.

## Verification

Read the diff and the surrounding workflow, and read `PullRequestBuildClassifier` plus `RepoConfigService.Parse` in the installed `TimeWarp.Nuru.DevCli` 4.0.0-beta.15 content (not executed). Did not re-run `./bin/dev build`, `./bin/dev test`, `ganda repo audit`, or the git hooks. No targeted command: nothing in the diff rose to a suspected bug.

- `pull_request` has no `paths` or `paths-ignore`. The only remaining `paths:` key is the `push` filter. The why comment is on that filter.
- `DecideAsync(repoRoot, ct)` is the one-argument call the package documents. It reads `GITHUB_EVENT_NAME`, the event file, and `.timewarp/dev.jsonc`. A non-pull-request event (push, `workflow_dispatch`, release, local) returns `RequiresBuild` true. `RunAsync` still sends only non-release modes through `RunPrAsync`, and release still uses `ReleasePipeline`.
- On skip, `WriteSkipOutputAsync` appends `requires_build=false` to `GITHUB_OUTPUT` when that variable is set, then `PipelineResult.Success` becomes exit 0 in `Handle`. The upload steps use `steps.pipeline.outputs.requires_build != 'false'` together with the existing release and probe exclusions, so an unset output still uploads and a skip does not hit `if-no-files-found: error`.
- Checkout already uses `fetch-depth: 0`, which the classifier needs for `git diff base...head`.
- The package rule skips only declared non-build paths (`kanban/`, `documentation/`, `assets/`, `notes/`, `.github/` except `.github/workflows/`, `.gitignore`, and `*.md` outside `source`, `tests`, `tools`, and `msbuild`). Anything else builds. `samples/` and `scripts/` are not on that skip list, so a non-markdown change there already requires a build. `RepoConfigService.Parse` uses case-insensitive names and skips JSONC comments, so `pullRequestBuild.buildPaths` binds.
- Packed inputs outside those protected roots are the root `readme.md` (`source/timewarp-state/timewarp-state.csproj`) and `assets/logo.png` (`source/Directory.Build.props`). Both are in `buildPaths`, which the classifier checks before the default `assets/` and markdown skips. Other package readmes live under `source/**`, which always builds. No `class PullRequestBuildClassifier` remains in this repo.
- `## Results` records `ganda repo audit --fix` (no edits), the pin bump, the skip output, both CI-standard checks passing, `./bin/dev build` 0/0, self-install, `./bin/dev test` with per-project pass counts, and a kanban-only smoke against `caae4ff4` that printed the skip message, exited 0, and wrote `requires_build=false`. That record covers the change. The checklist line for this pull request's `CI` URL waits on the host open-pr node.

## Issues

None.

## Observations (not findings)

- The restatement does not declare `samples/**` and `scripts/**`, which the task Notes give as roots outside `source`, `tests`, `tools`, and `msbuild`. The shipped classifier is a skip list, not an allowlist of those four roots, so those trees already build. See `## Learnings`.
- A markdown-only change under `samples/` or `scripts/` now skips. Those trees do not pack a readme. The shared rule is the contract the task asked for.
- `DecideAsync` also runs on a push. It prints that the event is not a pull request and then the pipeline builds. Release is unchanged.
- The push filter gained `readme.md` and `assets/logo.png` so a merge that only touches a packed file still uploads a Packages artifact. The kanban-only exclusion stays.
