namespace TestApp.Client.Features.Counter
{
  using BlazorState;
  using MediatR;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Base;

  public partial class CounterState
  {
    internal class ThrowServerSideExceptionHandler : BaseHandler<ThrowServerSideExceptionAction>
    {
      public ThrowServerSideExceptionHandler(IStore aStore) : base(aStore) { }

      /// <summary>
      /// Intentionally throw so we can test exception handling.
      /// </summary>
      /// <param name="aThrowServerSideExceptionAction"></param>
      /// <param name="aCancellationToken"></param>
      /// <returns></returns>
      public override Task<Unit> Handle
      (
        ThrowServerSideExceptionAction aThrowServerSideExceptionAction,
        CancellationToken aCancellationToken
      ) => throw new HttpRequestException(aThrowServerSideExceptionAction.Message)
      {
        Source = $"{nameof(TestApp)}.Server"
      };
    }
  }
}