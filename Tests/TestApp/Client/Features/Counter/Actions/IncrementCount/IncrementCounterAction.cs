namespace TestApp.Client.Features.Counter
{
  using BlazorState;

  internal partial class CounterState
  {
    public class IncrementCounterAction : IAction
    {
      public int Amount { get; set; }
    }
  }
}