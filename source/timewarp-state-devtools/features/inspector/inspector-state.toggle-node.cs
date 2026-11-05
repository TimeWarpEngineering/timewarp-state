#region Purpose
// Flips one JSON-tree node between open and closed.
#endregion

#region Design
// Reads IsNodeExpanded inside the handler so the tree's click only dispatches the path.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Flips one tree entry in <see cref="InspectorState.ExpandedNodes"/>.</summary>
  public static class ToggleNodeActionSet
  {
    /// <summary>The toggle-node action.</summary>
    public sealed class Action : IAction
    {
      /// <summary>Creates the action.</summary>
      public Action(string path)
      {
        Path = path ?? "";
      }

      /// <summary>Node path from <see cref="StateTree"/>.</summary>
      public string Path { get; }
    }

    /// <summary>Writes the flipped choice onto <see cref="InspectorState"/>.</summary>
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

        bool expanded = InspectorState.IsNodeExpanded(action.Path);
        InspectorState.ExpandedNodes = InspectorState.ExpandedNodes.SetItem(action.Path, !expanded);
        return default;
      }
    }
  }
}
