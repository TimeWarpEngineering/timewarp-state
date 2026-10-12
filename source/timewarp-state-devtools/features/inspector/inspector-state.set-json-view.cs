#region Purpose
// Selects Tree or Raw for the state panel.
#endregion

#region Design
// Page-local presentation. Does not touch InspectorLog.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Sets <see cref="InspectorState.JsonView"/>.</summary>
  public static class SetJsonViewActionSet
  {
    /// <summary>The set-json-view action.</summary>
    public sealed class Action : IAction
    {
      /// <summary>Creates the action.</summary>
      public Action(string view)
      {
        View = view ?? "";
      }

      /// <summary><c>tree</c> or <c>raw</c>.</summary>
      public string View { get; }
    }

    /// <summary>Writes the view onto <see cref="InspectorState"/>.</summary>
    public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        if (action.View == JsonViewTree || action.View == JsonViewRaw)
        {
          InspectorState.JsonView = action.View;
        }

        return default;
      }
    }
  }
}
