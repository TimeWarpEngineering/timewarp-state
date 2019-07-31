namespace TestApp.Client.Features.CloneTest
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using TestApp.Client.Features.Base;

  internal partial class CloneTestState
  {
    internal class CloneTestHandler : BaseHandler<CloneTestAction, CloneTestState>
    {
      public CloneTestHandler(IStore aStore) : base(aStore) { }

      protected CloneTestState CloneTestState => Store.GetState<CloneTestState>();

      public override Task<CloneTestState> Handle
      (
        CloneTestAction aIncrementCounterRequest,
        CancellationToken aCancellationToken
      ) => Task.FromResult(CloneTestState);
    }
  }
}