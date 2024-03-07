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
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<Unit> Handle
    (
      ThrowExceptionAction action,
      CancellationToken cancellationToken
    ) => throw new Exception(action.Message);
  }
}
