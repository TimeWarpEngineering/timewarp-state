namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  internal class ThrowServerSideExceptionHandler
  (
    IStore store,
    HttpClient HttpClient
  ) : BaseActionHandler<ThrowServerSideExceptionAction>(store)
  {

    /// <summary>
    /// Intentionally throw so we can test exception handling.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task Handle
    (
      ThrowServerSideExceptionAction action,
      CancellationToken cancellationToken
    )
    {
      var throwServerSideExceptionRequest = new ThrowServerSideExceptionRequest();

      await HttpClient.GetFromJsonAsync<ThrowServerSideExceptionResponse>
      (
      throwServerSideExceptionRequest.GetRoute()
      , cancellationToken: cancellationToken
      );
    }
  }
}
