namespace TestApp.Client.Features.Counter
{
  using BlazorState;
  using MediatR;
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Base;

  public partial class CounterState
  {
    internal class ThrowExceptionHandler : BaseHandler<ThrowExceptionAction>
    {
      public ThrowExceptionHandler(IStore aStore) : base(aStore) { }

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
}