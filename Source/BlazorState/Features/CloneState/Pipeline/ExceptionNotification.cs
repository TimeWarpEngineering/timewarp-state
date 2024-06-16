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
