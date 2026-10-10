---
uid: TimeWarpState:StateTransactions.md
title: State transactions
---

# State transactions

`StateTransactionBehavior` (pipeline order 300) clones the enclosing state before the handler runs and installs that clone as the live state. A successful handler leaves the clone in the store. A failed handler rolls the clone back only when it is still the live instance. A concurrent action that already replaced it is left in place, and the behavior logs that the rollback was skipped.

## Cancellation

`OperationCanceledException` is not a failure. The behavior does not publish `ExceptionNotification` for it. After the rollback decision, the exception is always rethrown. Callers that chain actions stop instead of treating the cancelled send as success.

## Handler exceptions

Any other exception is logged, rolled back under the same rule, and published as `ExceptionNotification`. The publish uses `CancellationToken.None`, so a cancelled request token cannot skip the notification.

By default `TimeWarpStateOptions.RethrowHandlerExceptions` is false. `Send` then returns the default response and does not fault. Set the option to true when the host wants `Send` to fault after the notification:

```csharp
builder.Services.AddTimeWarpState(options =>
{
  options.RethrowHandlerExceptions = true;
});
```

The rethrow happens after rollback and after `ExceptionNotification`. Subscribers still see the failure, and the caller sees the original exception.
