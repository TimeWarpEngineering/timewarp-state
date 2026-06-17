namespace Test.App.Client.Features.Purple;

public partial class PurpleState
{
  public static class IncrementCountActionSet
  {
    internal sealed class Action : IAction
    {
      public int Amount { get; init; }
    }
    
    internal sealed class Handler : ActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}
      PurpleState PurpleState => Store.GetState<PurpleState>();

      public override ValueTask<Unit> Handle(Action action, CancellationToken cancellationToken)
      {
        PurpleState.Count += action.Amount;
        return new ValueTask<Unit>(Unit.Value);
      }
    }
  }
}
