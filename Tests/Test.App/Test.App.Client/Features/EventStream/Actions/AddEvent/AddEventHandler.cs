namespace Test.App.Client.Features.EventStream;

internal partial class EventStreamState
{
  internal class AddEventHandler
  (
    IStore store
  ) : BaseActionHandler<AddEventAction>(store)
  {

    public override Task Handle
    (
      AddEventAction aAddEventAction,
      CancellationToken aCancellationToken
    )
    {
      EventStreamState.EventList.Add(aAddEventAction.Message);
      return Task.CompletedTask;
    }
  }
}
