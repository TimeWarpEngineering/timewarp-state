# Fix and restructure scripts/e2e.cs (dead modes, Amuru exit codes, process lifetime)
## Description

`scripts/e2e.cs` (517 LOC) has dead code paths, exit-code checks that can never fire, and an undisposed SUT process. Fix the bugs and split the script into a few cohesive parts.

## Requirements

Bugs:
- `runMode` is the constant `"Auto"` (line 55). Manual, Development and Release branches are dead, and Dev/Release would block forever because they `await RunAsync()` on the server. Remove them or make the mode a real argument that starts the server in the background.
- Every `if (exitCode != 0) Environment.Exit(1)` is dead: Amuru beta.5 throws on non-zero exit. Either use `WithNoValidation()` and keep the checks, or drop the checks and let the exception carry the failure. `Environment.Exit` also skips the `using var context` disposal.
- The SUT `Process` (line ~346) is never disposed. `KillSut` checks `HasExited` then `Kill()`, which can throw inside `finally`; `WaitForExit()` has no timeout.
- `WaitForSut` does not dispose the `HttpResponseMessage`, has a bare `catch`, retries silently on non-success, and never checks whether the server process already died.
- The `UseHttp` string replace (~line 248) is a silent no-op if the JSON has no space after the colon.
- Exception at ~199 names the wrong project.

Conventions:
- Use TimeWarp.Amuru instead of `System.Diagnostics.Process` (lines ~300, 346, 470).
- Normalise indentation (file mixes 2 and 4 spaces).
- Remove `async` from methods with no `await` (CS1998).

Split:
- Build steps (~203-298)
- A `SutHost` class owning the process and log writers, `IAsyncDisposable` (~300-427, 470-485)
- Test runner (~429-468)
- Dev-certs step (~487-517)
- `Run` stays as orchestration.

Cleanups: hard-coded `./tests/test-app/output` though `outputPath` exists; log paths built twice; `UseHttp` env var set twice; `net11.0` hard-coded; `settings` loop has one element; `WithClean()` dev-certs result discarded.

## Checklist

- [ ] Dead modes removed or made real
- [ ] Exit-code handling consistent with Amuru throw semantics
- [ ] SUT process owned by a disposable host; no throw from finally
- [ ] WaitForSut disposes responses, logs failures, detects early exit
- [ ] Amuru replaces Process
- [ ] File split as above
- [ ] E2E run still passes (or fails for the same pre-existing reasons as origin/dev)

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. The E2E suite has a known failing baseline; compare before/after rather than requiring green.

## Session

- Created: claude code review session (2026-11-04)
