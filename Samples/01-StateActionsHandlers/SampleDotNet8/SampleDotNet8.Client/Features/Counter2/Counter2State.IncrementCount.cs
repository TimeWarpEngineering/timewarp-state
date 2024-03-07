namespace SampleDotNet8.Client.Features.Counter2;

public partial class Counter2State
{
  public static class IncrementCount
  {
    public class Action : IAction
    {
      public int Amount { get; init; }
    }

    public class Handler
    (
      IStore store
    ) : ActionHandler<Action>(store)
    {

      Counter2State Counter2State => Store.GetState<Counter2State>();

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        Counter2State.Count += action.Amount;
        return Task.CompletedTask;
      }
    }
  }
}
