#region Purpose
// IncrementCount action for the WebAssembly counter: adds Amount to Count.
#endregion

#region Design
// Nested ActionSet inside the partial WasmCounterState so the handler can set the private Count.
// The handler is synchronous and returns a completed ValueTask.
#endregion

namespace Sample04Wasm.Features.Counter;

partial class WasmCounterState
{
  public static class IncrementCountActionSet
  {
    public sealed class Action : IAction
    {
      public int Amount { get; }

      public Action(int amount)
      {
        Amount = amount;
      }
    }

    public sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) { }

      private WasmCounterState WasmCounterState => Store.GetState<WasmCounterState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        WasmCounterState.Count += action.Amount;
        return default;
      }
    }
  }
}
