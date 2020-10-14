namespace TestApp.Client.Features.Counter
{
  using BlazorState;
  using MediatR;
  using System.Threading;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Base;
  using static TestApp.Client.Features.Counter.WrongNesting;

  public partial class CounterState
  {
    internal class ImproperNestedHandler : BaseHandler<ImproperNestedAction>
    {
      public ImproperNestedHandler(IStore aStore) : base(aStore) { }

      public override Task<Unit> Handle
      (
        ImproperNestedAction aImproperNestedAction,
        CancellationToken aCancellationToken
      ) => Unit.Task;
    }
  }
}