#region Purpose
// Pins which store the State panel folds.
#endregion

#region Design
// Null StoreId means follow the default store (StoreSelection.Default). Empty StoreId pins the
// untagged bucket, so "no pin" and "spans with no store tag" never collide. A picked id stays put
// when a later store arrives, so looking at a prerender fold is not replaced by the next page load.
// The action always pins (empty is the untagged bucket); FollowDefaultStore clears the pin to null.
// Two actions, because the generated SelectStore method does not carry a nullable parameter.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Sets <see cref="InspectorState.SelectedStoreId"/>.</summary>
  public static class SelectStoreActionSet
  {
    /// <summary>The select-store action.</summary>
    public sealed class Action : IAction
    {
      /// <summary>Creates the action.</summary>
      public Action(string storeId)
      {
        StoreId = storeId ?? "";
      }

      /// <summary>Store id to pin, or empty for the untagged bucket.</summary>
      public string StoreId { get; }
    }

    /// <summary>Writes the pinned store onto <see cref="InspectorState"/>.</summary>
    public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        InspectorState.SelectedStoreId = action.StoreId;
        return default;
      }
    }
  }
}
