namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public class ThrowServerSideExceptionAction : IAction
  {
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string Message { get; set; }
  }
}
