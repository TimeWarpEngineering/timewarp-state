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

Root `.gitignore` must contain this glob (comments/blanks ok):

```
*.journal.json
```

One line covers every routine journal (`task-work`, stacked-task-set, planning,
rfc, debate, advisor, and the next one). Ganda **268** updates the audit check
to PASS on this glob; do not add the six 262 exact names.

Prefer `ganda repo audit --fix --checks routine-journals-gitignore` (this
CLI requires `--fix` when `--checks` is set) so the commented block matches
other origins:

```gitignore
# Routine journals beside kitchens (local; not product)
*.journal.json
```

Then:

- `git rm --cached` any `*.journal.json` that `git ls-files` still lists.
  Delete empty leftover dirs if they exist only because of the journal.
- Do **not** `git rm` product `task.md` files.
- `git ls-files '*.journal.json'` must be empty.
- Audit check `routine-journals-gitignore` PASSes.
- `git check-ignore -v` on a journal basename path hits the new line.

## Checklist

- [x] Root `.gitignore` has `*.journal.json`
- [x] `git ls-files '*.journal.json'` is empty
- [x] Audit `routine-journals-gitignore` PASSes
- [x] `git check-ignore -v` confirms ignore; porcelain does not list journals
- [x] Do not implement on `master`

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
# expect: .gitignore:…:*.journal.json (path may be untracked)

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
- Pattern: `*.journal.json` (cockpit, 2026-09-03) — one glob, not six names
- Implementer: grok `01a06c8e-8df9-7ea2-b268-1d098edc4c0d` (2026-09-04) claimed worktree `task-082-ignore-routine-journals-so-worktree-gc-is-not-dirt`

## Results

Consumer sweep on this origin: confirm routine journals are ignored so `worktree gc` is not dirty. **No product diff on this branch** — `origin/master` already carries the org glob.

**What was implemented**

- Verified root `.gitignore` already has the commented block from `c6247c5d` (`chore: ignore routine journals and memsearch local files`, 2026-09-03), ancestor of `origin/master`:
  ```gitignore
  # Routine journals beside kitchens (local; not product)
  *.journal.json
  ```
- Ran `ganda repo audit --fix --checks routine-journals-gitignore`: check **PASS**; `--fix` was a no-op (`.gitignore already ignores routine journals`).
- `git ls-files '*.journal.json'` is empty — no `git rm --cached` required. Did **not** commit journal contents. Did **not** `git rm` product `task.md`.
- Kitchen moved to `kanban/in-progress/` on `task/082-ignore-routine-journals-so-worktree-gc-is-not-dirt`.

**Files changed**

- `.gitignore` — **unchanged** on this branch (already on master at lines 326–327)
- `kanban/in-progress/082-ignore-routine-journals-so-worktree-gc-is-not-dirty/task.md` — checklist, session, Results

**Key decisions / deviations**

- One glob (`*.journal.json`), not the six 262 exact names (already the landed form).
- Brief said this origin was missing the ignore; that was stale after `c6247c5d` landed on master the same day the kitchen was written. Remaining product work is zero.
- Did **not** add `.memsearch/memory/` (already present; separate audit). Did **not** chase unrelated full-audit FAILs (`bin-dev`, `dev-cli-capabilities`, `kebab-path-names`, `memsearch-scaffold`).

**Test outcomes**

- `git ls-files '*.journal.json'` — empty
- `git check-ignore -v kanban/to-do/task-work.journal.json` — `.gitignore:327:*.journal.json`
- `git check-ignore -v kanban/to-do/082-ignore-routine-journals-so-worktree-gc-is-not-dirty/task-work.journal.json` — same glob (kitchen path; file is ignored)
- `git status --porcelain` — no `*.journal.json` lines
- `ganda repo audit --fix --checks routine-journals-gitignore` — `routine-journals-gitignore` **PASS**
- Full `ganda repo audit` still has unrelated fails (`bin-dev`, `dev-cli-capabilities`, `kebab-path-names`, `memsearch-scaffold`); not this task
- Branch is `task/082-ignore-routine-journals-so-worktree-gc-is-not-dirt`, not `master`

### How to validate

**Smoke**

```bash
git check-ignore -v kanban/to-do/task-work.journal.json
git ls-files '*.journal.json'
git status --porcelain | grep -E 'journal\.json' || true
ganda repo audit --fix --checks routine-journals-gitignore
git rev-parse --abbrev-ref HEAD
grep -n '\*\.journal\.json' .gitignore
```

**Expect**

- `git check-ignore -v` prints `.gitignore:327:*.journal.json` and `kanban/to-do/task-work.journal.json` (line number may shift; pattern must be `*.journal.json`)
- `git ls-files '*.journal.json'` is empty
- porcelain grep for `journal.json` prints nothing
- audit table line `routine-journals-gitignore` is **PASS** (other unrelated checks may still fail; `--fix` is a no-op)
- branch is `task/082-ignore-routine-journals-so-worktree-gc-is-not-dirt`, not `master`
- `.gitignore` contains `# Routine journals beside kitchens (local; not product)` then `*.journal.json`

**Automated gate**

```bash
git check-ignore -v kanban/to-do/task-work.journal.json
# expect: .gitignore:327:*.journal.json	kanban/to-do/task-work.journal.json

git ls-files '*.journal.json'
# expect: empty

ganda repo audit --fix --checks routine-journals-gitignore
# expect: routine-journals-gitignore PASS (fix is a no-op once present)
```

**Not in scope:** changing `WorktreeGcService` to treat untracked journals as clean; host unstage-all (ganda); committing journals; restoring `bin/dev`; memsearch scaffold.
