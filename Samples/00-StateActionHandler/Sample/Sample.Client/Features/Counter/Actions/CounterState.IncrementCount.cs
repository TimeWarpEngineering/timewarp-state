// CounterState.IncrementCount.cs
namespace Sample.Client.Features.Counter;

using TimeWarp.State;

public partial class CounterState
{
  internal static class IncrementCountActionSet
  {
    internal sealed class Action : IAction
    {
      public int Amount { get; }
      public Action(int amount)
      {
        Amount = amount;
      }
    }
    
    internal sealed class Handler : ActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}
      
      private CounterState CounterState => Store.GetState<CounterState>();
      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        CounterState.Count += action.Amount;
        return Task.CompletedTask;
      }
    }
  }
  
  public async Task IncrementCount(int amount = 1, CancellationToken cancellationToken = default) =>
    await Sender.Send(new IncrementCountActionSet.Action(amount), cancellationToken);
}
