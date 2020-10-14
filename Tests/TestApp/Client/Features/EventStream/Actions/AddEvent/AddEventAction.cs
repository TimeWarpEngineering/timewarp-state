namespace TestApp.Client.Features.EventStream
{
  using BlazorState;

  internal partial class EventStreamState
  {
    public class AddEventAction : IAction
    {
      public string Message { get; set; }
    }
  }
}