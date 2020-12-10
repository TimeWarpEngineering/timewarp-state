namespace BlazorState.Pipeline.State
{
  using MediatR;
  using System;

  public class ExceptionNotification : INotification
  {
    public string RequestName { get; set; }

    public Exception Exception { get; set; }
  }
}
