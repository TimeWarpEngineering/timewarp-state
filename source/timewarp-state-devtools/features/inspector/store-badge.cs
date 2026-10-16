#region Purpose
// Short, colour-stable store label for an action-log row.
#endregion

#region Design
// pre and live come from timewarp.state.render. Without that tag the label is the first
// four characters of the store id. Hue is a stable hash of the full id so two stores
// that share a label usually look different; with eight buckets two stores share a colour about one
// time in eight. Eight hues match the stylesheet data-hue rules and the --twe-store-hue-* tokens.
// Matches is a prefix match on the id, the badge word, and the runtime word (wasm or srv).
// A substring match over a Guid hits nearly every row for any hex character or short word, so the
// filter would barely narrow the log. Runtime text is separate from the store badge so interactive
// server and interactive WebAssembly rows are not both only "live".
// Label and Description both switch on KindOf, so the badge word and its explanation cannot drift.
// Tooltip is Description plus the store id when the badge has text. The log row and the Store picker
// both call Tooltip. An empty label (no id and no known render) uses UntaggedDescription, not the
// prefix sentence. DescriptionElementId names one shared element per explanation. The row button
// points at it, so the store id stays in the tooltip and out of the accessible name.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

/// <summary>
/// Badge text and colour bucket for a store id.
/// </summary>
public static class StoreBadge
{
  /// <summary>Number of id characters shown when the render kind is unknown.</summary>
  public const int PrefixLength = 4;

  /// <summary>Colour buckets. Matches <c>data-hue</c> in the inspector stylesheet.</summary>
  public const int HueCount = 8;

  /// <summary>Badge for a prerender store.</summary>
  public const string PrerenderLabel = "pre";

  /// <summary>Badge for an interactive store.</summary>
  public const string InteractiveLabel = "live";

  /// <summary>Explanation for <see cref="PrerenderLabel"/>.</summary>
  public const string PrerenderDescription =
    "Prerender: a short-lived server store created while rendering a page's HTML; discarded after the request.";

  /// <summary>Explanation for <see cref="InteractiveLabel"/>.</summary>
  public const string InteractiveDescription = "Live: an interactive store.";

  /// <summary>Explanation when the badge is an id prefix because the render kind is unknown.</summary>
  public const string PrefixDescription = "Store id prefix: render kind unknown.";

  /// <summary>Explanation when the row has no store id and no known render kind.</summary>
  public const string UntaggedDescription = "Untagged: no store id.";

  /// <summary>Element id for <see cref="PrerenderDescription"/>.</summary>
  public const string PrerenderDescriptionId = "store-badge-pre";

  /// <summary>Element id for <see cref="InteractiveDescription"/>.</summary>
  public const string InteractiveDescriptionId = "store-badge-live";

  /// <summary>Element id for <see cref="PrefixDescription"/>.</summary>
  public const string PrefixDescriptionId = "store-badge-prefix";

  /// <summary>Element id for <see cref="UntaggedDescription"/>.</summary>
  public const string UntaggedDescriptionId = "store-badge-untagged";

  /// <summary>Short label for <see cref="InspectorCapture.WasmRuntime"/>.</summary>
  public const string WasmLabel = "wasm";

  /// <summary>Short label for <see cref="InspectorCapture.ServerRuntime"/>.</summary>
  public const string ServerLabel = "srv";

  /// <summary>
  /// Badge text. Empty when <paramref name="storeId"/> and <paramref name="render"/> are empty.
  /// </summary>
  public static string Label(string storeId, string render)
  {
    BadgeKind kind = KindOf(render);
    if (kind == BadgeKind.Prerender)
    {
      return PrerenderLabel;
    }

    if (kind == BadgeKind.Interactive)
    {
      return InteractiveLabel;
    }

    if (storeId.Length == 0)
    {
      return "";
    }

    int length = Math.Min(PrefixLength, storeId.Length);
    return storeId[..length];
  }

  /// <summary>
  /// Explanation of the badge <see cref="Label"/> draws for <paramref name="render"/>.
  /// </summary>
  public static string Description(string render)
  {
    BadgeKind kind = KindOf(render);
    if (kind == BadgeKind.Prerender)
    {
      return PrerenderDescription;
    }

    if (kind == BadgeKind.Interactive)
    {
      return InteractiveDescription;
    }

    return PrefixDescription;
  }

  /// <summary>
  /// Tooltip text. <see cref="UntaggedDescription"/> when <see cref="Label"/> is empty.
  /// Otherwise <see cref="Description"/> and, when present, the full store id.
  /// </summary>
  public static string Tooltip(string storeId, string render)
  {
    if (Label(storeId, render).Length == 0)
    {
      return UntaggedDescription;
    }

    string description = Description(render);
    if (storeId.Length == 0)
    {
      return description;
    }

    return description + "\n" + storeId;
  }

  /// <summary>
  /// Id of the shared explanation element for the badge <see cref="Label"/> draws.
  /// </summary>
  public static string DescriptionElementId(string storeId, string render)
  {
    if (Label(storeId, render).Length == 0)
    {
      return UntaggedDescriptionId;
    }

    BadgeKind kind = KindOf(render);
    if (kind == BadgeKind.Prerender)
    {
      return PrerenderDescriptionId;
    }

    if (kind == BadgeKind.Interactive)
    {
      return InteractiveDescriptionId;
    }

    return PrefixDescriptionId;
  }

  /// <summary>
  /// Short runtime label. Empty when <paramref name="runtime"/> is empty or unknown.
  /// </summary>
  public static string RuntimeLabel(string runtime)
  {
    if (string.Equals(runtime, InspectorCapture.WasmRuntime, StringComparison.OrdinalIgnoreCase))
    {
      return WasmLabel;
    }

    if (string.Equals(runtime, InspectorCapture.ServerRuntime, StringComparison.OrdinalIgnoreCase))
    {
      return ServerLabel;
    }

    return "";
  }

  /// <summary>Colour bucket in <c>0 .. <see cref="HueCount"/> - 1</c>. Stable for a store id.</summary>
  public static int Hue(string storeId)
  {
    if (storeId.Length == 0)
    {
      return 0;
    }

    unchecked
    {
      int hash = 17;
      foreach (char character in storeId)
      {
        hash = (hash * 31) + character;
      }

      return (hash & 0x7FFFFFFF) % HueCount;
    }
  }

  /// <summary>
  /// True when the store id, the store badge, or the runtime label starts with <paramref name="filter"/>, ignoring case.
  /// </summary>
  /// <param name="storeId">Store Guid text.</param>
  /// <param name="render">Render tag.</param>
  /// <param name="filter">Filter text. Empty matches every row.</param>
  /// <param name="runtime">Runtime tag. Empty skips the runtime label.</param>
  public static bool Matches(string storeId, string render, string filter, string runtime = "")
  {
    if (filter.Length == 0)
    {
      return true;
    }

    if (storeId.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    string label = Label(storeId, render);
    if (label.Length > 0 && label.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    string runtimeLabel = RuntimeLabel(runtime);
    return runtimeLabel.Length > 0 && runtimeLabel.StartsWith(filter, StringComparison.OrdinalIgnoreCase);
  }

  private static BadgeKind KindOf(string render)
  {
    if (string.Equals(render, InspectorCapture.PrerenderRender, StringComparison.OrdinalIgnoreCase))
    {
      return BadgeKind.Prerender;
    }

    if (string.Equals(render, InspectorCapture.InteractiveRender, StringComparison.OrdinalIgnoreCase))
    {
      return BadgeKind.Interactive;
    }

    return BadgeKind.Unknown;
  }

  private enum BadgeKind
  {
    Prerender,
    Interactive,
    Unknown
  }
}
