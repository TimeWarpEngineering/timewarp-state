namespace TestApp.Client.Features.Counter
{
  using BlazorState;

  public partial class CounterState
  {
    public class ThrowExceptionAction : IAction
    {
      public string Message { get; set; }
    }
  }
}