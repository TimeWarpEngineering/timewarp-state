namespace Test.App.Client.Features.Counter;

public partial class CounterZeroState
{
  public class IncrementCounterAction : IAction
  {
    public int Amount { get; set; }
  }
}
