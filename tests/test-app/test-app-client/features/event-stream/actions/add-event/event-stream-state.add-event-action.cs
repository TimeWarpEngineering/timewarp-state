namespace Test.App.Client.Features.EventStream;

public partial class EventStreamState
{
  public static class AddEventActionSet
  {

    internal sealed class Action : IAction
    {
      public string Message { get; }
      public Action(string message) 
      {
        Message = message;
      }
    }

    internal sealed class AddEventHandler
    (
      IStore store
    ) : BaseActionHandler<Action>(store)
    {

      public override Task Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        EventStreamState.EventList.Add(action.Message);
        return Task.CompletedTask;
      }
    }
  }
}
