namespace Test.App.Client.Features.EventStream;

internal partial class EventStreamState
{
  [UsedImplicitly]
  internal class AddEventHandler
  (
    IStore store
  ) : BaseActionHandler<AddEventAction>(store)
  {

    public override Task Handle
    (
      AddEventAction action,
      CancellationToken cancellationToken
    )
    {
      EventStreamState.EventList.Add(action.Message);
      return Task.CompletedTask;
    }
  }
}
