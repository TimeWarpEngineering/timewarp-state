#region Purpose
// Pins which store the State panel folds.
#endregion

#region Design
// Empty StoreId means follow the newest store. A picked id stays put when a later store arrives,
// so looking at a prerender fold is not replaced by the next page load.
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

      /// <summary>Store id to pin, or empty to follow the newest store.</summary>
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
