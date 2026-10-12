#region Purpose
// Unpins the State panel so it follows the default store again.
#endregion

#region Design
// The store selector's Follow live option dispatches this. Null SelectedStoreId means follow
// StoreSelection.Default, the newest store that is not a prerender.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Clears <see cref="InspectorState.SelectedStoreId"/>.</summary>
  public static class FollowDefaultStoreActionSet
  {
    /// <summary>The follow-default-store action.</summary>
    public sealed class Action : IAction;

    /// <summary>Sets <see cref="InspectorState.SelectedStoreId"/> to null.</summary>
    public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        InspectorState.SelectedStoreId = null;
        return default;
      }
    }
  }
}
