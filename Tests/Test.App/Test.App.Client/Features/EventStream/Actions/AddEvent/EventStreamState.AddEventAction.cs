namespace Test.App.Client.Features.EventStream;

internal partial class EventStreamState
{
  public static class AddEventActionSet
  {

    internal sealed class Action : IAction
    {
      public required string Message { get; init; }
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
