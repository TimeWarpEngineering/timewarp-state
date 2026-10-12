#region Purpose
// Picks which store the State panel folds: the pinned store, or the default store.
#endregion

#region Design
// The default is the newest store whose render tag is not prerender. A prerender store is thrown
// away with its request, and enhanced navigation can prerender a new page into a fresh store while
// the existing circuit keeps running without a new init. Following the newest store would then
// swap the live fold for a discarded one. When every store is prerender, the newest store wins.
// A store with no render tag counts as live, because the host could not tell.
// A null pin follows the default. An empty pin is the untagged bucket, a real store choice.
// A pin on a store that is no longer listed (trimmed or cleared) falls back to the default.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

/// <summary>
/// Default and effective store for the State panel.
/// </summary>
public static class StoreSelection
{
  /// <summary>
  /// Newest store whose render is not <c>prerender</c>; the newest store when all are prerender;
  /// null when <paramref name="stores"/> is empty.
  /// </summary>
  /// <param name="stores">Stores newest first, as returned by <see cref="InspectorLog.ListStores"/>.</param>
  public static StoreSummary? Default(IReadOnlyList<StoreSummary> stores)
  {
    ArgumentNullException.ThrowIfNull(stores);
    foreach (StoreSummary store in stores)
    {
      if (!string.Equals(store.Render, InspectorCapture.PrerenderRender, StringComparison.OrdinalIgnoreCase))
      {
        return store;
      }
    }

    return stores.Count == 0 ? null : stores[0];
  }

  /// <summary>
  /// Store id the State panel folds: <paramref name="pinnedStoreId"/> when it is listed, otherwise the default.
  /// Empty when there are no stores.
  /// </summary>
  /// <param name="stores">Stores newest first.</param>
  /// <param name="pinnedStoreId">Pinned store id. Null follows the default; empty is the untagged bucket.</param>
  public static string Effective(IReadOnlyList<StoreSummary> stores, string? pinnedStoreId)
  {
    ArgumentNullException.ThrowIfNull(stores);
    if (IsListed(stores, pinnedStoreId))
    {
      return pinnedStoreId!;
    }

    return Default(stores)?.StoreId ?? "";
  }

  /// <summary>True when <paramref name="storeId"/> is non-null and names a store in <paramref name="stores"/>.</summary>
  public static bool IsListed(IReadOnlyList<StoreSummary> stores, string? storeId)
  {
    ArgumentNullException.ThrowIfNull(stores);
    if (storeId is null)
    {
      return false;
    }

    foreach (StoreSummary store in stores)
    {
      if (string.Equals(store.StoreId, storeId, StringComparison.Ordinal))
      {
        return true;
      }
    }

    return false;
  }

  /// <summary>Render tag of <paramref name="storeId"/> in <paramref name="stores"/>, or empty.</summary>
  public static string RenderOf(IReadOnlyList<StoreSummary> stores, string storeId)
  {
    ArgumentNullException.ThrowIfNull(stores);
    foreach (StoreSummary store in stores)
    {
      if (string.Equals(store.StoreId, storeId, StringComparison.Ordinal))
      {
        return store.Render;
      }
    }

    return "";
  }
}
