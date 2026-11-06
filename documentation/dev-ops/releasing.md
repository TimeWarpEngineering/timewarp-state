---
uid: TimeWarpState:Releasing.md
title: Releasing
---

# Releasing

A release promotes the `Packages-*` artifact that CI already built and tested. Nothing is rebuilt at release time.

## Cut a release

1. Merge the version bump PR. `<Version>` in `source/Directory.Build.props` is the only place the version is typed.
2. Wait for the green `workflow.yml` push run on the merge commit.
3. From a clean master checkout that is in sync with `origin/master`, run `dev release --dry-run`, then `dev release`.

`dev release` creates the annotated tag `v{Version}`, pushes it, and creates the GitHub Release with generated notes. The `release: published` event runs `dev workflow` in release mode. That run checks out the tag, finds the CI run for the tagged commit (locate-run), downloads its `Packages-*` artifact, verifies the package set against the version, and pushes to NuGet.org.

## Kanban-only commits after the merge

ganda lands the board move as its own commit after a PR merges (`land kanban <id>`). That commit changes only `kanban/**`. The `workflow.yml` push trigger has a `paths:` filter that leaves out `kanban/**`, so the commit gets no CI run. Master HEAD is often one or more of these commits past the last green merge.

A commit that changes only `kanban/` cannot change `<Version>` or any packed file, so the last green commit's artifact is the HEAD artifact. The rule:

- Walk first-parent history back from HEAD, up to 50 commits.
- The first commit with a successful `workflow.yml` run (a push run is preferred, and pull_request runs never count) is the **release target**. Its artifact ships.
- Every commit between HEAD and the target must change only paths under `kanban/`. A commit that changes anything else ends the walk, and the release refuses. The code at HEAD is untested.
- Renames count both sides, so a move out of `source/` into `kanban/` is not kanban-only.

`dev release` applies this rule and puts the tag on the release target, not on HEAD. The tag then names the commit whose artifact was tested and shipped. The release run checks out that tag, so its locate-run finds the run directly. `dev workflow --mode release` (break-glass from master) applies the same walk in locate-run.

The dry run names the target when it is not HEAD:

```text
✓ HEAD is 6 kanban-only commit(s) past 640ea95 (CI run 42162651594)

Dry run — no changes made.
Version: 12.0.0-beta.17
Tag: v12.0.0-beta.17
Target commit: 640ea95 (640ea9513ebe5f871db3f7ebc485e734278727c9)
```

The release pipeline prints the same fact at locate-run:

```text
HEAD c8908d6 is 6 kanban-only commit(s) past 640ea95; promoting the artifact built at 640ea9513ebe5f871db3f7ebc485e734278727c9.
```

No `gh workflow run` dispatch is needed to create a run at a kanban-only HEAD.

Only `kanban/` is exempt. Other paths the push filter leaves out (`documentation/`, `analysis/`, and so on) still end the walk. If a release refuses because of one of those commits, dispatch CI on master (`gh workflow run workflow.yml --ref master`) and wait for the run before you release.

## When a release refuses

| Message | Fix |
| --- | --- |
| No successful CI run for HEAD, or for a commit it is only kanban-only commits past | Wait for CI on the merge commit, or rerun it (`gh run rerun <id>`). If the message names a commit outside `kanban/`, that commit needs a green run. |
| Tag already exists | The version was already tagged. Resume a partial publish with a break-glass dispatch from the tag's ref (`gh workflow run workflow.yml --ref v{Version} -f mode=release -f confirm=release`), or bump the version. |
| Downloaded package set does not match | The CI run predates the version bump. Rerun CI on the named commit. |

## Break-glass

`workflow_dispatch` with `mode=release` and `confirm=release` runs the release pipeline without a GitHub Release. Run it from the tag's ref when a tag exists: the tag-pin check refuses a run whose HEAD is not the tagged commit. `mode=probe` checks the NuGet trusted-publishing login only.
