namespace BlazorState.Pipeline.State;

public class ExceptionNotification : INotification
{
  //public TRequest Request { get; set; }
  public string RequestName { get; set; }

  public Exception Exception { get; set; }
}
