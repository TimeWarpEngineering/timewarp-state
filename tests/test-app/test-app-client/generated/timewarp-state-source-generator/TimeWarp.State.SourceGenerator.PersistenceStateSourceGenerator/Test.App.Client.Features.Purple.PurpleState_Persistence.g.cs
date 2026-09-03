#nullable enable

#pragma warning disable CS1591
namespace Test.App.Client.Features.Purple;

public partial class PurpleState
{
  /// <summary>
  /// (Re)loads this [PersistentState] state from its configured persistence store.
  /// </summary>
  public async Task Load(CancellationToken? externalCancellationToken = null)
  {
    using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
      ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
      : null;

    // Load is dispatched through a single hand-written Mediator request whose handler is
    // registered by Mediator's generator (a per-state generated handler would not be).
    await Sender.Send
    (
      new global::TimeWarp.State.Plus.PersistentState.LoadPersistentStateRequest(typeof(PurpleState)),
      linkedCts?.Token ?? CancellationToken
    );
  }
}
#pragma warning restore CS1591
