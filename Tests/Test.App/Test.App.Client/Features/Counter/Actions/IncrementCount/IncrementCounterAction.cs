namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public class IncrementCounterAction : IAction
  {
    public int Amount { get; init; }
  }
}
