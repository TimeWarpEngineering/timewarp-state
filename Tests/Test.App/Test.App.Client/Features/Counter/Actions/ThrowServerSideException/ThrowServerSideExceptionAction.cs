namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public class ThrowServerSideExceptionAction : IAction
  {
    public required string Message { get; set; }
  }
}
