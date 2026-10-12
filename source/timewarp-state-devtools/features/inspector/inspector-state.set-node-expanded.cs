#region Purpose
// Records whether one JSON-tree path is expanded.
#endregion

#region Design
// The choice lives on InspectorState so a new log row re-renders without collapsing the tree.
// SetItem replaces the dictionary; the previous map stays with the transaction clone.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Sets one entry in <see cref="InspectorState.ExpandedNodes"/>.</summary>
  public static class SetNodeExpandedActionSet
  {
    /// <summary>The set-node-expanded action.</summary>
    public sealed class Action : IAction
    {
      /// <summary>Creates the action.</summary>
      public Action(string path, bool expanded)
      {
        Path = path ?? "";
        Expanded = expanded;
      }

      /// <summary>Node path from <see cref="StateTree"/> or <see cref="StateDiff"/>.</summary>
      public string Path { get; }

      /// <summary>True to expand the node.</summary>
      public bool Expanded { get; }
    }

    /// <summary>Writes the choice onto <see cref="InspectorState"/>.</summary>
    public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        if (action.Path.Length == 0)
        {
          return default;
        }

        InspectorState.ExpandedNodes = InspectorState.ExpandedNodes.SetItem(action.Path, action.Expanded);
        return default;
      }
    }
  }
}
