namespace Test.App.Client.Features.CloneTest;

using System.Threading;
using System.Threading.Tasks;
using Test.App.Client.Features.Base;

internal partial class CloneTestState
{
  internal class CloneTestHandler : BaseActionHandler<CloneTestAction>
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
