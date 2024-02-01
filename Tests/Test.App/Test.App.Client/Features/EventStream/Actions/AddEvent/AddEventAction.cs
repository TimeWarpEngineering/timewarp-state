namespace Test.App.Client.Features.EventStream;

internal partial class EventStreamState
{
  public class AddEventAction : IAction
  {
    public string Message { get; set; }
  }
}
