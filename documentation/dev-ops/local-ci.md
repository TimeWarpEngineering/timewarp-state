# Local CI fast path

`dev ci` runs the pull-request pipeline on your machine. When it passes, GitHub CI skips that pipeline on the
same tree, or on a tree that differs from it only under `kanban/`, and finishes in under a minute instead of five
to seven.

## Run it

```bash
git status          # must be clean: the note covers HEAD's tree
dev ci              # or: dotnet run --file tools/dev-cli/dev.cs -- ci
git push
```

`ganda task work` runs it for you before the push CI sees, from the `localCi` key in `.timewarp/dev.jsonc`.

## What `dev ci` does

1. Refuses a dirty worktree.
2. Runs assert-version-ssot, clean, build, test, e2e, and verify-samples (the same stages as `dev workflow`
   on a pull request). verify-samples packs into `artifacts/packages`.
3. Refuses to publish if a stage changed a tracked file or moved `HEAD`.
4. Pushes the `*.nupkg` files to `refs/ci-packages/<tree>`, a parentless commit whose tree is those files.
5. Writes an unsigned note on `refs/notes/ganda-ci` for `HEAD^{tree}` with `ganda repo ci-note write` and
   pushes it. The note lists the stages, the tools (`sdk`, `rid`, `playwright`, `chromium`), and each package's
   sha256.

| Exit | Meaning |
|------|---------|
| 0 | Stages passed; packages and note published |
| 1 | A stage failed, or the worktree was dirty. Nothing published |
| 2 | Stages passed, but the package ref or the note was not published (no `ganda`, auth, network). CI runs in full |

## What CI does

The `Local CI fast path` step (`scripts/ci-fast-path-gate.ps1`) runs on `pull_request` and `push` before
`Run CI Pipeline`. CI checks out `refs/pull/N/merge` on a pull request and the merge commit on `master`. Both
carry the kanban commits that `ganda task work` lands on `master` before the merge, so their tree is almost never
the tree `dev ci` attested. The gate looks for the attested tree through the commit graph. It tries these
candidates in order and skips a tree it has already tried:

1. `HEAD`;
2. `pr-head`, the pull request's head commit (`CI_FAST_PATH_PR_HEAD`, set on `pull_request` only);
3. `parent N`, each parent of `HEAD` when `HEAD` is a merge (on the `master` push the PR head is parent 2).

A candidate is the attested one when its tree has a note and that tree equals `HEAD^{tree}` once the top-level
`kanban` entry is removed from both (`scripts/ci-fast-path-tree.ps1`). A candidate with a note that differs
outside `kanban/` is rejected with the paths that differ, and the walk goes on. The hit line names the winner,
for example `CI fast path: hit (pr-head <sha>)`. With no winner the gate prints one miss line with every
candidate's reason.

It takes the fast path only when all of these hold for the attested tree:

- the note for the attested tree exists and records every stage as `pass`;
- the note's `sdk` equals `dotnet --version` after Setup .NET;
- the note's `rid` equals the runner's (`linux-x64`);
- `refs/ci-packages/<attested tree>` holds exactly the packages the note lists, and every sha256 matches.

Then it restores those packages into `artifacts/packages`, skips `Run CI Pipeline` and the `coverage` upload,
and the `Upload Artifacts` step publishes `Packages-<run_number>` as usual, so `dev release` can promote it.
Any miss prints the reason, stays green, and the full pipeline runs.

## Notes

- `dev ci` itself still needs an exact tree: it refuses a dirty worktree and writes the note for `HEAD^{tree}`.
  Only the CI lookup relaxes to "equal outside `kanban/`".
- Lookup is by tree, not commit. If `master` moved after `dev ci` ran and the move touched only `kanban/` (the
  land-kanban commits of a walk), the PR and master-push runs still hit. If `master` moved anywhere else, the merge
  tree differs outside `kanban/` and CI runs in full.
- `kanban/` holds task files only. No build, test, or pack step reads it, so ignoring it cannot change a package.
- The note is not signed. Anyone who can push the ref can also edit the workflow, so a signature would protect
  nothing (task 150 override, 2026-11-17).
- A fast-path run has no `coverage` artifact. Nothing in this repo reads it.
- The SourceLink commit inside fast-path packages is the commit `dev ci` packed. Its tree equals the tree CI
  checked out outside `kanban/`, so every source file SourceLink points at is the same.

why: task 150 (2026-11-18); task 162, kanban-only tree drift and the candidate walk (2026-11-18)
