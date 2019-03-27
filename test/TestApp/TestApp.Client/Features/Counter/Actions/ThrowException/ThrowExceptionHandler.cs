namespace TestApp.Client.Features.Counter
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using System;
  using TestApp.Client.Features.Base;

  public partial class CounterState
  {
    public class ThrowExceptionHandler : BaseHandler<ThrowExceptionAction, CounterState>
    {
      public ThrowExceptionHandler(IStore aStore) : base(aStore) { }

      public override Task<CounterState> Handle(
        ThrowExceptionAction aThrowExceptionAction, 
        CancellationToken aCancellationToken)
      {
        // Intentionally throw so we can test exception handling.
        throw new Exception(aThrowExceptionAction.Message);
      }
    }
  }
}