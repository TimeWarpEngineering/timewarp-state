namespace TestApp.Client.Features.Counter
{
  using BlazorState;
  using MediatR;
  using System.Threading;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Base;

  public partial class CounterState
  {
    internal class NonNestedHandler : BaseHandler<NonNestedAction>
    {
      public NonNestedHandler(IStore aStore) : base(aStore) { }

      public override Task<Unit> Handle
      (
        NonNestedAction aNonNestedAction,
        CancellationToken aCancellationToken
      ) => Unit.Task;
    }
  }
}