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

- [ ] `.githooks/pre-push.cs` refreshed via `ganda repo audit --fix --checks memsearch-scaffold`
- [ ] Audit clean (no `memsearch-scaffold` warning)
- [ ] Hook smoke test: task→home refused, raw sha→home allowed (stdin simulation only)
- [ ] Gates per this repo's `tw-pr` (a hook-only change needs no full build unless the skill's
      scope table says otherwise)
- [ ] Implementation review; host `open-pr`

## Notes

- One of a set of identical tasks filed in each repo that carries `.githooks/pre-push.cs`:
  amuru, architecture, bayline, ganda, kiini, mediator, nuru, state, taratibu.
- Do not start any app host. Run builds serially and call `dotnet build-server shutdown` before
  finishing.

## Results

*(fill when done)*

### How to validate

*(required before done)*

`ganda repo audit` shows no `memsearch-scaffold` warning, and the stdin smoke test output is
recorded.

## Session

- Created: 2026-10-01
