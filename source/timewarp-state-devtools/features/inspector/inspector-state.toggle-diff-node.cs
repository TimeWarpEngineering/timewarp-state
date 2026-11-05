#region Purpose
// Flips one diff node between open and closed.
#endregion

#region Design
// Diff choices are stored under DiffKey so they never share a choice with the tree. Reads
// IsDiffNodeExpanded inside the handler so the diff's click only dispatches the path.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Flips one diff entry in <see cref="InspectorState.ExpandedNodes"/>.</summary>
  public static class ToggleDiffNodeActionSet
  {
    /// <summary>The toggle-diff-node action.</summary>
    public sealed class Action : IAction
    {
      /// <summary>Creates the action.</summary>
      public Action(string path)
      {
        Path = path ?? "";
      }

      /// <summary>Node path from <see cref="StateDiff"/>.</summary>
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

        bool expanded = InspectorState.IsDiffNodeExpanded(action.Path);
        InspectorState.ExpandedNodes = InspectorState.ExpandedNodes.SetItem(DiffKey(action.Path), !expanded);
        return default;
      }
    }
  }
}
