#region Purpose
// Selects an action-log row.
#endregion

#region Design
// Sequence 0 means no selection. Does not touch InspectorLog.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Sets <see cref="InspectorState.SelectedSequence"/>.</summary>
  public static class SelectEntryActionSet
  {
    /// <summary>The select-entry action.</summary>
    public sealed class Action : IAction
    {
      /// <summary>Creates the action.</summary>
      public Action(long sequence)
      {
        Sequence = sequence;
      }

      /// <summary>Row sequence, or 0 to clear.</summary>
      public long Sequence { get; }
    }

    /// <summary>Writes the selection onto <see cref="InspectorState"/>.</summary>
    public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        InspectorState.SelectedSequence = action.Sequence;
        return default;
      }
    }
  }
}
