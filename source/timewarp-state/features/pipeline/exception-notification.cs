#region Purpose
// Notification carrying the name of a request whose handler threw and the exception. StateTransactionBehavior
// publishes it after restoring the original state.
#endregion

#region Design
// Immutable INotification (get-only properties set in the constructor). Apps subscribe with an
// INotificationHandler to report errors.
#endregion

namespace TimeWarp.Features.StateTransactions;

public class ExceptionNotification : INotification
{
  public ExceptionNotification(string requestName, Exception exception)
  {
    RequestName = requestName;
    Exception = exception;
  }
  public string RequestName { get; }

  public Exception Exception { get; }
}
