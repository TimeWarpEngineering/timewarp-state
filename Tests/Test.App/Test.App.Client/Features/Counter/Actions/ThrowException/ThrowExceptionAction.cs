namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public class ThrowExceptionAction : IAction
  {
    public required string Message { get; init; }
  }
}
