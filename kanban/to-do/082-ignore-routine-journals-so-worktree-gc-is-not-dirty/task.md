# Ignore routine journals so worktree gc is not dirty

## Description

`ganda task work` writes `task-work.journal.json` beside the kitchen. Unless
root `.gitignore` lists that basename, `git status --porcelain` shows `??`
and `ganda pr merge` / `worktree gc` **refuses** a dirty worktree.

This is a **consumer sweep**. Ganda **262** added audit check
`routine-journals-gitignore` and `--fix`, then left “sweep every org repo”
out of scope. That was wrong: we have hit this on merge at least six times
(Taratibu 252/253/254, mediator 004-001/004-002, architecture 207/208,
timewarp-software **033**). Each origin that never ran `--fix` is another
dirty-gc.

This origin (`timewarp-state`) is missing the ignore. Org SSOT: `ganda repo audit`
check `routine-journals-gitignore`. `--fix` appends the missing basename
lines. Tracked journals are **Failed / not fixable** — `git rm --cached`
is required (gitignore does not hide tracked files).

Do **not** commit journal contents.

## Requirements

Root `.gitignore` must contain these exact basename lines (comments/blanks ok):

```
task-work.journal.json
stacked-task-set.journal.json
planning.journal.json
rfc.journal.json
debate.journal.json
advisor.journal.json
```

Prefer `ganda repo audit --fix --checks routine-journals-gitignore` (this
CLI requires `--fix` when `--checks` is set) so the commented block matches
other origins:

```gitignore
# Task-work resume journal beside kitchens (local; not product)
task-work.journal.json
stacked-task-set.journal.json
planning.journal.json
rfc.journal.json
debate.journal.json
advisor.journal.json
```

Then:

- `git rm --cached` any `*.journal.json` that `git ls-files` still lists.
  Delete empty leftover dirs if they exist only because of the journal.
- Do **not** `git rm` product `task.md` files.
- `git ls-files '*.journal.json'` must be empty.
- Audit check `routine-journals-gitignore` PASSes.
- `git check-ignore -v` on a journal basename path hits the new line.

## Checklist

- [ ] Root `.gitignore` has the six routine-journal basenames
- [ ] `git ls-files '*.journal.json'` is empty
- [ ] Audit `routine-journals-gitignore` PASSes
- [ ] `git check-ignore -v` confirms ignore; porcelain does not list journals
- [ ] Do not implement on `master`

## Notes

- Predecessor: ganda `kanban/done/262-audit-gitignore-for-task-work-journal-so-worktree-gc-is-not-dirty/`
- Consumer precedent: architecture **208**, timewarp-software **034**
- Host hole (ganda kitchen, separate): unstage **any** `kanban/**/*.journal.json`
  on kitchen commits; consider a hook that runs `repo audit --fix`.
- 262 out-of-scope (“do not sweep every org repo”) is why this kitchen exists.

### How to validate

**Automated**
```bash
git check-ignore -v kanban/to-do/task-work.journal.json || true
# expect: .gitignore:…:task-work.journal.json (path may be untracked)

git ls-files '*.journal.json'
# expect: empty

ganda repo audit --fix --checks routine-journals-gitignore
# expect: routine-journals-gitignore PASS (fix is a no-op once present)
```

**Not in scope:** changing `WorktreeGcService` to treat untracked journals as
clean; host unstage-all (ganda).

## Session

- Created: grok `01a06304-cbf6-7d83-b5a2-4a99e9d09d40` (2026-09-03) cockpit timewarp-flow
- Trigger: `/tw-merge` software 033 — GC refused, then leftover journal
  committed; 262 left consumer sweep out of scope
