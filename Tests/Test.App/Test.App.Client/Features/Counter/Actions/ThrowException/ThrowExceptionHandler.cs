namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  internal class ThrowExceptionHandler
  (
    IStore store
  ) : BaseActionHandler<ThrowExceptionAction>(store)
  {
    /// <summary>
    /// Intentionally throw so we can test exception handling.
    /// </summary>
    /// <param name="aThrowExceptionAction"></param>
    /// <param name="aCancellationToken"></param>
    /// <returns></returns>
    public override Task<Unit> Handle
    (
      ThrowExceptionAction aThrowExceptionAction,
      CancellationToken aCancellationToken
    ) => throw new Exception(aThrowExceptionAction.Message);
  }
}
