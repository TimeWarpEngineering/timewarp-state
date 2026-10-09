#region Purpose
// Action whose handler always throws, used to test that a failed action rolls state back without crashing the UI.
#endregion

#region Design
// The handler throws new Exception(action.Message). ThrowExceptionPage and its E2E test assert that CounterState.Guid
// is unchanged afterwards, which shows StateTransactionBehavior restored the clone.
#endregion

namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public static class ThrowExceptionActionSet
  {
    public sealed class Action : IAction
    {
      public string Message { get; }

      public Action(string message)
      {
        Message = message;
      }
    }

    internal sealed class Handler : BaseActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}

      /// <summary>
      /// Intentionally throw so we can test exception handling.
      /// </summary>
      public override ValueTask Handle
      (
        Action action,
        CancellationToken cancellationToken
      ) => throw new Exception(action.Message);
    }
  }
}
