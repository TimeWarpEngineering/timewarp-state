namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState
{
  public static class TwoSecondTaskActionSet
  {
    [TrackAction]
    public class Action : IAction;

    [UsedImplicitly]
    internal class Handler : ActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}
      public override async Task Handle(Action action, CancellationToken cancellationToken)
      {
        Console.WriteLine("Start two Second Task");
        await Task.Delay(millisecondsDelay: 2000, cancellationToken: cancellationToken);
        Console.WriteLine("Two Second Task Complete");
      }
    }
  }
  
  public async Task TwoSecondTask(CancellationToken cancellationToken = default) =>
    await Sender.Send(new TwoSecondTaskActionSet.Action(), cancellationToken);
}
