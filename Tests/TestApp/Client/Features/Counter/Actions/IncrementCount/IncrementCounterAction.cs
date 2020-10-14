namespace TestApp.Client.Features.Counter
{
  using BlazorState;

  public partial class CounterState
  {
    public class IncrementCounterAction : IAction
    {
      public int Amount { get; set; }
    }
  }
}