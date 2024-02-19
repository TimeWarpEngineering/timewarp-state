namespace Test.App.Client.Features.Counter;

using System.Net.Http;
using System.Net.Http.Json;
using Test.App.Contracts.Features.ExceptionHandlings;

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
    /// <param name="aThrowServerSideExceptionAction"></param>
    /// <param name="aCancellationToken"></param>
    /// <returns></returns>
    public override async Task Handle
    (
      ThrowServerSideExceptionAction aThrowServerSideExceptionAction,
      CancellationToken aCancellationToken
    )
    {
      var throwServerSideExceptionRequest = new ThrowServerSideExceptionRequest();

      ThrowServerSideExceptionResponse? throwServerSideExceptionResponse =
        await HttpClient.GetFromJsonAsync<ThrowServerSideExceptionResponse>
        (
          throwServerSideExceptionRequest.GetRoute()
          , cancellationToken: aCancellationToken
        );
    }
  }
}
