namespace TestApp.Client.Features.Counter
{
  using BlazorState;
  using MediatR;
  using System.Net.Http;
  using System.Net.Http.Json;
  using System.Threading;
  using System.Threading.Tasks;
  using TestApp.Api.Features.ExceptionHandlings;
  using TestApp.Client.Features.Base;

  public partial class CounterState
  {
    internal class ThrowServerSideExceptionHandler : BaseActionHandler<ThrowServerSideExceptionAction>
    {
      private readonly HttpClient HttpClient;

      public ThrowServerSideExceptionHandler(IStore aStore, HttpClient aHttpClient) : base(aStore)
      {
        HttpClient = aHttpClient;
      }

      /// <summary>
      /// Intentionally throw so we can test exception handling.
      /// </summary>
      /// <param name="aThrowServerSideExceptionAction"></param>
      /// <param name="aCancellationToken"></param>
      /// <returns></returns>
      public override async Task<Unit> Handle
      (
        ThrowServerSideExceptionAction aThrowServerSideExceptionAction,
        CancellationToken aCancellationToken
      )
      {
        var throwServerSideExceptionRequest = new ThrowServerSideExceptionRequest();

        ThrowServerSideExceptionResponse throwServerSideExceptionResponse =
          await HttpClient.GetFromJsonAsync<ThrowServerSideExceptionResponse>(throwServerSideExceptionRequest.GetRoute());

        return Unit.Value;
      }
    }
  }
}