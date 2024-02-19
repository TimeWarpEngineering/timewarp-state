namespace Test.App.Client.Features.EventStream;

internal partial class EventStreamState
{
  internal class AddEventHandler : BaseActionHandler<AddEventAction>
  {
    public AddEventHandler(IStore store) : base(store) { }

    public override Task Handle
    (
      AddEventAction aAddEventAction,
      CancellationToken aCancellationToken
    )
    {
      EventStreamState._Events.Add(aAddEventAction.Message);
      return Task.CompletedTask;
    }
  }
}
