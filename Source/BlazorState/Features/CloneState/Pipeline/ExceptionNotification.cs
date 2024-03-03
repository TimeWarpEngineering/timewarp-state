namespace TimeWarp.Features.StateTransactions;

public class ExceptionNotification : INotification
{
  //public TRequest Request { get; set; }
  public string RequestName { get; set; }

  public Exception Exception { get; init; }
}
