namespace TestApp.Client.Features.Counter
{
  using BlazorState;

  internal partial class CounterState
  {
    public class ThrowExceptionAction : IAction
    {
      public string Message { get; set; }
    }
  }
}