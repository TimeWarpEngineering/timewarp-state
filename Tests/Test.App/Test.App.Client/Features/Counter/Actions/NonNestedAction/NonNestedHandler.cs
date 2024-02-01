namespace Test.App.Client.Features.Counter;

using System.Threading;
using System.Threading.Tasks;
using Test.App.Client.Features.Base;

public partial class CounterState
{
  internal class NonNestedHandler : BaseActionHandler<NonNestedAction>
  {
    public NonNestedHandler(IStore store) : base(store) { }

    public override Task<Unit> Handle
    (
      NonNestedAction aNonNestedAction,
      CancellationToken aCancellationToken
    ) => Unit.Task;
  }
}
