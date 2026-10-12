#region Purpose
// Short, colour-stable store label for an action-log row.
#endregion

#region Design
// pre and live come from timewarp.state.render. Without that tag the label is the first
// four characters of the store id. Hue is a stable hash of the full id so two stores
// that share a label usually look different; with eight buckets two stores share a colour about one
// time in eight. Eight hues match the stylesheet data-hue rules and the --twe-store-hue-* tokens.
// Matches is a prefix match on the id and on the badge word. A substring match over a Guid hits
// nearly every row for any hex character or short word, so the filter would barely narrow the log.
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

  /// <summary>
  /// Badge text. Empty when <paramref name="storeId"/> and <paramref name="render"/> are empty.
  /// </summary>
  public static string Label(string storeId, string render)
  {
    if (string.Equals(render, InspectorCapture.PrerenderRender, StringComparison.OrdinalIgnoreCase))
    {
      return PrerenderLabel;
    }

    if (string.Equals(render, InspectorCapture.InteractiveRender, StringComparison.OrdinalIgnoreCase))
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
  /// True when the store id or the badge text starts with <paramref name="filter"/>, ignoring case.
  /// </summary>
  public static bool Matches(string storeId, string render, string filter)
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
    return label.Length > 0 && label.StartsWith(filter, StringComparison.OrdinalIgnoreCase);
  }
}
