namespace TestApp.Client.Features.Counter
{
  using BlazorState;

  public partial class CounterState
  {
    public class ThrowServerSideExceptionAction : IAction
    {
      public string Message { get; set; }
    }
  }
}