namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState
{
  public static class FiveSecondTask
  {
    [TrackAction]
    public record Action : IAction;

    [UsedImplicitly]
    internal class Handler
    (
      IStore store
    ): ActionHandler<Action>(store)
    {
      private ActionTrackingState ActionTrackingState => Store.GetState<ActionTrackingState>();

      public override async Task Handle(Action action, CancellationToken cancellationToken)
      {
        Console.WriteLine("Start five second task");
        await Task.Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken);
        Console.WriteLine("Five second task complete");
      }
    }
  }
}
