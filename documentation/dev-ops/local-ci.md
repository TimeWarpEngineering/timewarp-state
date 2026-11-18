# Local CI fast path

`dev ci` runs the pull-request pipeline on your machine. When it passes, GitHub CI skips that pipeline on the
same tree and finishes in under a minute instead of five to seven.

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
`Run CI Pipeline`. It takes the fast path only when all of these hold:

- the note for `HEAD^{tree}` exists and records every stage as `pass`;
- the note's `sdk` equals `dotnet --version` after Setup .NET;
- the note's `rid` equals the runner's (`linux-x64`);
- `refs/ci-packages/<tree>` holds exactly the packages the note lists, and every sha256 matches.

Then it restores those packages into `artifacts/packages`, skips `Run CI Pipeline` and the `coverage` upload,
and the `Upload Artifacts` step publishes `Packages-<run_number>` as usual, so `dev release` can promote it.
Any miss prints the reason, stays green, and the full pipeline runs.

## Notes

- Lookup is by tree, not commit. A merge commit whose tree equals the tree `dev ci` ran on hits, so a pull
  request that is up to date with `master` is fast on the pull request and on the master push. If `master` moved
  after `dev ci` ran, the merge tree differs and CI runs in full.
- The note is not signed. Anyone who can push the ref can also edit the workflow, so a signature would protect
  nothing (task 150 override, 2026-11-17).
- A fast-path run has no `coverage` artifact. Nothing in this repo reads it.
- The SourceLink commit inside fast-path packages is the commit `dev ci` packed. It has the same tree as the
  commit CI checked out.

why: task 150 (2026-11-18)
