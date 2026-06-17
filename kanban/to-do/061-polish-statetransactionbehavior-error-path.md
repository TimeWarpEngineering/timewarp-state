# Polish StateTransactionBehavior error path

## Description

Code review 2026-06-11, finding 2 *(as revised after author feedback)* and finding 27 (`code-review-2026-06-11.md`).

The catch-rollback-notify design in `source/timewarp-state/features/pipeline/state-transaction-behavior.cs` is intentional and stays (roll back to known-good state, publish `ExceptionNotification`, don't crash the circuit). Three small defects remain in the error path:

1. **Error reporting can itself be cancelled (line 118):** `Publisher.Publish(exceptionNotification, cancellationToken)` reuses the request's token. If the handler failed *because* that token was cancelled, the Publish throws `OperationCanceledException` immediately — `ExceptionNotification` handlers never run and an exception propagates anyway, defeating the don't-crash design in exactly that case.
2. **`OperationCanceledException` is reported as an error:** cancellation isn't a failure; converting it to an `ExceptionNotification` may surface spurious error UI. Make a deliberate decision (likely: roll back state but skip or differentiate the notification).
3. **Copy-paste log defects:** line 36 logs `typeof(ReduxDevToolsBehavior<,>).GetSimpleName()` as this class's name (every "constructing {ClassName}" trace claims to be ReduxDevToolsBehavior); line 98's catch logs `"Error cloning State"` for what is actually a handler failure (cloning happens earlier, outside the try).

## Checklist

- [ ] Publish the exception notification with `CancellationToken.None`
- [ ] Decide and implement OCE handling (rollback yes; notification — decide)
- [ ] Fix constructor class-name log (line 36) and catch-block message text (line 98)
- [ ] Unit test: cancelled token still results in rollback + notification handlers running
