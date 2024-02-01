namespace Test.App.Client.Features.Counter;

using System.Threading;
using System.Threading.Tasks;
using Test.App.Client.Features.Base;
using static Test.App.Client.Features.Counter.WrongNesting;

public partial class CounterState
{
  internal class ImproperNestedHandler : BaseActionHandler<ImproperNestedAction>
  {
    public ImproperNestedHandler(IStore store) : base(store) { }

    public override Task<Unit> Handle
    (
      ImproperNestedAction aImproperNestedAction,
      CancellationToken aCancellationToken
    ) => Unit.Task;
  }
}
