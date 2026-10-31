# dev release refuses after ganda lands kanban: HEAD is a kanban-only commit with no CI run

## Description

Since ganda started landing the kanban move as its own commit after a PR merge (`land kanban <id>`,
seen first with task 132 on 2026-10-31), master HEAD right after a merge is a commit that touches only
`kanban/**`. The CI workflow's `push` trigger has a `paths:` filter that excludes `kanban/**`, so no
`workflow.yml` run exists for that commit. `dev release --dry-run` then refuses:

```
Error: no successful CI run of workflow.yml exists for e05df145… The Release event would fail at
locate-run — wait for CI on this commit or rerun it (gh run rerun <id>).
```

The merge commit one below (`d120f3a0`) has a green run and byte-identical product content. The
operator remedy was `gh workflow run workflow.yml --ref master` (dispatch, merge mode) to create a run
at HEAD, then `dev release`. That costs a full CI cycle for no new code and will happen after every
merge from now on.

Filed by Claude (cockpit) at Steven's request, 2026-10-31.

## Requirements

Pick one and record it in `## Notes` (the first is preferred; the second is the fallback if the
pipeline's own `locate-run` cannot be changed safely):

1. **Release guard and `locate-run` accept an ancestor run when the diff to HEAD is kanban-only.**
   In `dev release` (tools/dev-cli) and in the pipeline's locate-run step: walk back from HEAD while
   `git diff --name-only <commit>^ <commit>` is entirely under `kanban/**`; the first commit with a
   successful `workflow.yml` run is the artifact source. Record that commit in the dry-run output and
   in the Release notes body. The tag still goes on HEAD. Add a test with a fake run list.
2. **Trigger CI on the land commit.** Either ganda lands kanban before the merge commit (so the merge
   commit is HEAD; that is a timewarp-ganda change — file it there and link it here), or the workflow's
   push `paths:` filter gains `kanban/**` (wasteful: a full build per board move).

Either way:

- `documentation/developer/guides/releasing.md` explains the rule.
- `dev release --dry-run` prints which commit's artifact will be promoted when it differs from HEAD.
- Version is unaffected; no bump.

## Checklist

- [ ] Direction recorded
- [ ] Guard + locate-run (or trigger) change with tests
- [ ] releasing.md updated; dry-run prints the artifact commit
- [ ] Manual: merge a kanban-only landing on a throwaway branch or replay 2026-10-31 and confirm the dry run passes
- [ ] `ganda repo audit` clean; build, test green

## Session

- Created: cockpit (2026-10-31)

## Notes

- Related: 119 (previous release flow, before land-kanban existed), timewarp-ganda land-kanban change
  (version `1.0.0-beta.38+a9a702ef`).
- Evidence: `~/.grok/long-running-background-tasks/release-132.log` ("FAILED no master CI run for
  e05df145"); `gh run list --branch master` shows the last push run at `d120f3a0`.
