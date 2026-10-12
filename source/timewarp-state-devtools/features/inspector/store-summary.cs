#region Purpose
// One store in the inspector's store menu.
#endregion

#region Design
// Menu order is the sequence of each store's first span, newest first. LatestSequence is
// the highest sequence, used by callers that care about the last activity. Render is the
// host label when the span carried one.
// StoreId empty is the single bucket for spans that predate the store tag.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

/// <summary>
/// A store that has contributed at least one span or folded snapshot.
/// </summary>
public sealed class StoreSummary
{
  /// <summary>Value of <c>timewarp.state.store</c>. Empty when the span had no store tag.</summary>
  public required string StoreId { get; init; }

  /// <summary><c>prerender</c>, <c>interactive</c>, or empty.</summary>
  public string Render { get; init; } = "";

  /// <summary>Highest sequence seen for this store.</summary>
  public required long LatestSequence { get; init; }
}
