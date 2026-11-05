#region Purpose
// Selects an action-log row and pins the State panel to that row's store.
#endregion

#region Design
// Sequence 0 means no selection and leaves the store pin alone. A known row also pins its store
// (empty for untagged), so the State panel shows the state after that action. One action, so the
// page never dispatches a selection and a pin as two steps. InspectorLog is read, never written.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Sets <see cref="InspectorState.SelectedSequence"/> and pins the row's store.</summary>
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

    /// <summary>Writes the selection and the row's store onto <see cref="InspectorState"/>.</summary>
    public sealed class Handler(IStore store, InspectorLog inspectorLog) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        InspectorState.SelectedSequence = action.Sequence;
        if (inspectorLog.TryGetEntry(action.Sequence, out ActionLogEntry? entry) && entry is not null)
        {
          InspectorState.SelectedStoreId = entry.StoreId;
        }

        return default;
      }
    }
  }
}
