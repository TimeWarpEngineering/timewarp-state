namespace TestApp.Client.Features.CloneTest
{
  using BlazorState;
  using MediatR;
  using System.Threading;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Base;

  internal partial class CloneTestState
  {
    internal class CloneTestHandler : BaseHandler<CloneTestAction>
    {
      public CloneTestHandler(IStore aStore) : base(aStore) { }

      protected CloneTestState CloneTestState => Store.GetState<CloneTestState>();

      public override Task<Unit> Handle
      (
        CloneTestAction aCloneTestAction,
        CancellationToken aCancellationToken
      ) => Unit.Task;
    }
  }
}