namespace TestApp.Client.Features.Counter;

using BlazorState;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TestApp.Client.Features.Base;

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
