#region Purpose
// Applies a choice from the State panel's store picker: pin a store, pin untagged, or follow live.
#endregion

#region Design
// One action, so the picker never dispatches a pin and a selection reset as two steps.
// The picker's option values are StoreOptionFollow, StoreOptionUntagged, or a store id. Store ids
// are Guids, so neither marker collides with a real id. A blank value follows live, like the
// original page did for a missing option value. Picking a store clears the selected row, because
// the row may belong to a different store.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Store picker option that follows the default store.</summary>
  public const string StoreOptionFollow = "follow:";

  /// <summary>Store picker option that pins the untagged bucket.</summary>
  public const string StoreOptionUntagged = "untagged:";

  /// <summary>Sets <see cref="InspectorState.SelectedStoreId"/> from a picker option and clears the selection.</summary>
  public static class SelectStoreOptionActionSet
  {
    /// <summary>The select-store-option action.</summary>
    public sealed class Action : IAction
    {
      /// <summary>Creates the action.</summary>
      public Action(string option)
      {
        Option = option ?? "";
      }

      /// <summary><see cref="StoreOptionFollow"/>, <see cref="StoreOptionUntagged"/>, or a store id.</summary>
      public string Option { get; }
    }

    /// <summary>Writes the pin and clears <see cref="InspectorState.SelectedSequence"/>.</summary>
    public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        InspectorState.SelectedStoreId = action.Option switch
        {
          "" or StoreOptionFollow => null,
          StoreOptionUntagged => "",
          _ => action.Option
        };
        InspectorState.SelectedSequence = 0;
        return default;
      }
    }
  }
}
