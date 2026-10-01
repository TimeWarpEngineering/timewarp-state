# Refresh githooks pre-push to the ganda baseline (task branch to home guard)

## Description

Refresh this repo's `.githooks/pre-push.cs` (and the `.githooks/pre-push` shim if the baseline
changed it) to the current ganda repo baseline.

Ganda task 323 (timewarp-ganda PR #197, merged 2026-10-01) added a guard to the baseline
pre-push hook. It refuses a push whose local ref is `refs/heads/task/*` and whose destination
is the home branch (`master` / `main`), and the message names the task branch. Raw-sha pushes,
such as the `kanban publish` merge commit, stay allowed. Until each repo picks up the new hook,
`ganda repo audit` warns `memsearch-scaffold: .githooks/pre-push.cs (outdated)`, and the repo
lacks the guard.

## Requirements

1. Apply the baseline with ganda itself, not by hand:
   `ganda repo audit --fix --checks memsearch-scaffold`. Confirm that `.githooks/pre-push.cs`
   now matches the baseline and the warning is gone.
2. Keep any repo-specific hook content the baseline intends to preserve. If `--fix` would drop a
   local customization, stop and record it in Notes rather than overwrite it.
3. Re-run `ganda repo audit` and fix anything else it reports (boyscout welcome), then commit.
4. Smoke-test the hook without touching home:
   - Pipe a fake pre-push line into the hook,
     `refs/heads/task/x <sha> refs/heads/master <sha>`, and confirm it is refused.
   - Pipe `<sha> <sha> refs/heads/master <sha>` and confirm it is allowed.

   Do not push to master to test.

## Checklist

- [x] `.githooks/pre-push.cs` refreshed via `ganda repo audit --fix --checks memsearch-scaffold`
- [x] Audit clean (no `memsearch-scaffold` warning)
- [x] Hook smoke test: task→home refused, raw sha→home allowed (stdin simulation only)
- [x] Gates per this repo's `tw-pr` (a hook-only change needs no full build unless the skill's
      scope table says otherwise)
- [x] Implementation review; host `open-pr`

## Notes

- One of a set of identical tasks filed in each repo that carries `.githooks/pre-push.cs`:
  amuru, architecture, bayline, ganda, kiini, mediator, nuru, state, taratibu.
- Do not start any app host. Run builds serially and call `dotnet build-server shutdown` before
  finishing.

## Results

- `ganda repo audit --fix --checks memsearch-scaffold` refreshed `.githooks/pre-push.cs`
  (+19 lines, insertions only: the task/* → home guard and its header comment). There were no
  local customizations, so nothing was dropped. The `.githooks/pre-push` shim did not change.
- `ganda repo audit` passes. The `memsearch-scaffold` warning is gone. Before the refresh, the
  `bin-dev` and `dev-cli-capabilities` errors also appeared, because this fresh worktree had no
  local `bin/dev`. That binary is gitignored, and I built it with
  `dotnet run --file tools/dev-cli/dev.cs -- self-install`. No repo change was needed for it.
- One non-blocking advisory warning remains: `kebab-path-names` flags
  `tests/test-app/test-app-client/wwwroot/Test.App.Client.lib.module.js`. The name is required
  by Blazor: a JS initializer must be named `{AssemblyName}.lib.module.js`, and the assembly is
  `Test.App.Client`. I left it alone because renaming it would break the E2E test app.
- Gates: the change only touches a hook runfile, so no solution build is needed. Running the hook
  in the smoke test below compiles it.

### How to validate

Smoke:

```bash
ganda repo audit
S=$(git rev-parse HEAD); Z=0000000000000000000000000000000000000000
echo "refs/heads/task/x $S refs/heads/master $Z" | .githooks/pre-push origin url; echo "exit=$?"
echo "$S $S refs/heads/master $Z" | .githooks/pre-push origin url; echo "exit=$?"
```

Expect:

- Audit: `Repository passes`, with no `memsearch-scaffold` entry (only the kebab advisory).
- Task → home: `Refusing push of task branch to home: task/x -> master.` plus the
  guidance lines, then `exit=1`.
- Raw sha → home, run from a task branch HEAD: no output, `exit=0`.

Recorded output (2026-10-01):

```
--- task->home
Refusing push of task branch to home: task/x -> master.
Task branches publish to origin/<task branch> and land on home via PR.
Fix tracking: ganda repo audit --fix --checks task-branch-upstream, then git push.
Escape hatch (intentional only): git push --no-verify
exit=1
--- rawsha->home
exit=0
```

### Review disposition

- Rounds: 1. Effort 1, roster: general.
- Final counts: 0 bug, 0 suggestion, 0 nit (0 open, 0 fixed, 0 wontfix).
- Disposition: **clean**.
- Artifacts: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Session

- Created: 2026-10-01
- 2026-10-01: implementer (claude) refreshed hook via audit --fix, smoke-tested via stdin, audit passes.
- 2026-10-01: review oracle (claude) effort 1 general review, 0 findings, disposition clean.
