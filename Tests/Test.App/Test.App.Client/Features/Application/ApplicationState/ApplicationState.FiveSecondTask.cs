namespace Test.App.Client.Features.Application;

public partial class ApplicationState
{
  public static class FiveSecondTaskActionSet
  {
    [TrackAction]
    internal sealed record Action : IAction;

    [UsedImplicitly]
    internal sealed class Handler : ActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}

      public override async Task Handle(Action action, CancellationToken cancellationToken)
      {
        Console.WriteLine("Start five second task");
        await Task.Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken);
        Console.WriteLine("Five second task complete");
      }
    }
  }

  public async Task FiveSecondTask(CancellationToken? externalCancellationToken = null)
  {
    using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
      ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
      : null;

    await Sender.Send
    (
      new FiveSecondTaskActionSet.Action(),
      linkedCts?.Token ?? CancellationToken
    );
  }
}
