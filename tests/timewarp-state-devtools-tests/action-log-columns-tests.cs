#region Purpose
// Proves the action log header, row, and grid template name the same columns.
#endregion

#region Design
// Reads Inspector.razor and its isolated stylesheet. The page needs a circuit and a log to render,
// so the check is the markup and the CSS text: header labels, one cell per column on every row,
// and one --log-columns template shared by the header and .row. Newlines are normalised before
// the shared-rule check. --log-min-width is the border-box floor of the track minimums, the gaps,
// and the horizontal padding.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class ActionLogColumns_Should
{
  private static readonly string[] Columns =
  [
    "time",
    "store",
    "runtime",
    "action",
    "state",
    "duration",
    "result"
  ];

  private static readonly string[] Labels =
  [
    "Time",
    "Store",
    "Runtime",
    "Action",
    "State",
    "Duration",
    "Result"
  ];

  public static void NameEveryColumnOnceInTheHeaderTheRowAndTheTemplate()
  {
    string razor = NormalizeNewlines(File.ReadAllText(Path.Combine(RepoPaths.Root(), "source", "timewarp-state-devtools", "components", "pages", "Inspector.razor")));
    string css = NormalizeNewlines(File.ReadAllText(Path.Combine(RepoPaths.Root(), "source", "timewarp-state-devtools", "components", "pages", "Inspector.razor.css")));

    string template = CustomProperty(css, "--log-columns");
    CountTracks(template).ShouldBe(Columns.Length);
    decimal minWidth = SumTrackFloors(template) + ((Columns.Length - 1) * 0.6m) + 1.4m;
    CustomProperty(css, "--log-min-width").ShouldBe(minWidth.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "rem");
    css.ShouldContain("min-width: var(--log-min-width)");
    css.ShouldContain("column-gap: 0.6rem");
    css.ShouldContain("padding: 0.4rem 0.7rem");
    css.ShouldContain("padding: 0.55rem 0.7rem");
    css.Split("--log-columns:", StringSplitOptions.None).Length.ShouldBe(2);
    css.ShouldContain(".log-head,\n.row");
    css.ShouldContain("grid-template-columns: var(--log-columns)");
    css.ShouldContain("position: sticky");
    css.ShouldContain("text-overflow: ellipsis");
    css.ShouldContain("width: fit-content");
    css.ShouldNotContain("grid-template-columns: 1fr 1fr");
    css.ShouldNotContain("#");

    string head = Slice(razor, "class=\"log-head\"", "</div>");
    head.ShouldContain("aria-hidden=\"true\"");
    head.ShouldNotContain("<button");
    ColumnNames(head).ShouldBe(Columns);
    for (int index = 0; index < Labels.Length; index++)
    {
      head.ShouldContain("data-col=\"" + Columns[index] + "\">" + Labels[index] + "<");
    }

    int headAt = razor.IndexOf("class=\"log-head\"", StringComparison.Ordinal);
    int listAt = razor.IndexOf("<ul>", headAt, StringComparison.Ordinal);
    headAt.ShouldBeGreaterThanOrEqualTo(0);
    listAt.ShouldBeGreaterThan(headAt);
    Slice(razor, "<ul>", "</ul>").ShouldNotContain("log-head");

    string row = Slice(razor, "data-qa=\"row\"", "</button>");
    row.ShouldNotContain("@if");
    ColumnNames(row).ShouldBe(Columns);
    row.ShouldContain("data-qa=\"runtime\"");
    row.ShouldContain("StoreBadge.Tooltip(entry.StoreId, entry.Render)");
    row.ShouldContain("aria-describedby=\"@StoreBadge.DescriptionElementId(entry.StoreId, entry.Render)\"");
    row.ShouldNotContain("aria-label");
    row.ShouldContain("title=\"@entry.ActionName\"");
    row.ShouldContain("title=\"@entry.StateTypeName\"");

    razor.ShouldContain("id=\"@StoreBadge.PrerenderDescriptionId\"");
    razor.ShouldContain("id=\"@StoreBadge.InteractiveDescriptionId\"");
    razor.ShouldContain("id=\"@StoreBadge.PrefixDescriptionId\"");
    razor.ShouldContain("id=\"@StoreBadge.UntaggedDescriptionId\"");
    razor.ShouldNotContain("aria-label=\"@badgeHelp\"");
    razor.ShouldContain("StoreBadge.Tooltip(storeId, render)");
    razor.ShouldContain("<label id=\"store-picker-label\" for=\"store-picker\">Store</label>");
    razor.ShouldContain("aria-labelledby=\"store-picker-label\"");
    razor.ShouldContain("title=\"@StoreSelection.PickerHelp\"");
    razor.ShouldContain("aria-describedby=\"store-picker-help\"");
    razor.ShouldContain("title=\"@StoreSelection.FollowLiveHelp\"");
    razor.ShouldContain("@StoreSelection.FollowLiveHelp");
    razor.ShouldContain("StoreBadge.Tooltip(store.StoreId, store.Render)");
  }

  private static string CustomProperty(string css, string name)
  {
    string marker = name + ":";
    int start = css.IndexOf(marker, StringComparison.Ordinal);
    start.ShouldBeGreaterThanOrEqualTo(0);
    string rest = css[(start + marker.Length)..];
    int end = rest.IndexOf(';', StringComparison.Ordinal);
    end.ShouldBeGreaterThan(0);
    return rest[..end].Trim();
  }

  private static int CountTracks(string template)
  {
    int count = 0;
    int depth = 0;
    bool inTrack = false;
    foreach (char character in template)
    {
      if (character == '(')
      {
        depth++;
      }
      else if (character == ')')
      {
        depth--;
      }

      if (depth == 0 && char.IsWhiteSpace(character))
      {
        inTrack = false;
        continue;
      }

      if (!inTrack)
      {
        count++;
        inTrack = true;
      }
    }

    return count;
  }

  private static string NormalizeNewlines(string text) =>
    text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);

  private static decimal SumTrackFloors(string template)
  {
    decimal sum = 0;
    foreach (string track in SplitTracks(template))
    {
      sum += FloorOf(track);
    }

    return sum;
  }

  private static List<string> SplitTracks(string template)
  {
    List<string> tracks = [];
    int depth = 0;
    int start = 0;
    for (int index = 0; index < template.Length; index++)
    {
      char character = template[index];
      if (character == '(')
      {
        depth++;
      }
      else if (character == ')')
      {
        depth--;
      }

      if (depth == 0 && char.IsWhiteSpace(character))
      {
        if (index > start)
        {
          tracks.Add(template[start..index]);
        }

        start = index + 1;
      }
    }

    if (start < template.Length)
    {
      string tail = template[start..].Trim();
      if (tail.Length > 0)
      {
        tracks.Add(tail);
      }
    }

    return tracks;
  }

  private static decimal FloorOf(string track)
  {
    int rem = track.IndexOf("rem", StringComparison.Ordinal);
    rem.ShouldBeGreaterThan(0);
    int start = rem;
    while (start > 0 && (char.IsDigit(track[start - 1]) || track[start - 1] == '.'))
    {
      start--;
    }

    return decimal.Parse(track[start..rem], System.Globalization.CultureInfo.InvariantCulture);
  }

  private static string Slice(string text, string startMarker, string endMarker)
  {
    int start = text.IndexOf(startMarker, StringComparison.Ordinal);
    start.ShouldBeGreaterThanOrEqualTo(0);
    int end = text.IndexOf(endMarker, start, StringComparison.Ordinal);
    end.ShouldBeGreaterThan(start);
    return text[start..end];
  }

  private static List<string> ColumnNames(string markup)
  {
    List<string> names = [];
    const string marker = "data-col=\"";
    int index = 0;
    while (true)
    {
      int start = markup.IndexOf(marker, index, StringComparison.Ordinal);
      if (start < 0)
      {
        return names;
      }

      start += marker.Length;
      int end = markup.IndexOf('"', start);
      end.ShouldBeGreaterThan(start);
      names.Add(markup[start..end]);
      index = end + 1;
    }
  }
}
