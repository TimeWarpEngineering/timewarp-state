namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public class ThrowExceptionAction : IAction
  {
    public string Message { get; set; }
  }
}
