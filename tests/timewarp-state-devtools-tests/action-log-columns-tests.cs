#region Purpose
// Proves the action log header, row, and grid template name the same columns.
#endregion

#region Design
// Reads Inspector.razor and its isolated stylesheet. The page needs a circuit and a log to render,
// so the check is the markup and the CSS text: header labels, one cell per column on every row,
// and one --log-columns template shared by the header and .row.
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
    string razor = File.ReadAllText(Path.Combine(RepoPaths.Root(), "source", "timewarp-state-devtools", "components", "pages", "Inspector.razor"));
    string css = File.ReadAllText(Path.Combine(RepoPaths.Root(), "source", "timewarp-state-devtools", "components", "pages", "Inspector.razor.css"));

    string template = CustomProperty(css, "--log-columns");
    CountTracks(template).ShouldBe(Columns.Length);
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
    row.ShouldContain("aria-label=\"@badgeHelp\"");
    row.ShouldContain("title=\"@entry.ActionName\"");
    row.ShouldContain("title=\"@entry.StateTypeName\"");

    razor.ShouldContain("StoreBadge.Tooltip(storeId, render)");
    razor.ShouldContain("aria-label=\"@badgeHelp\"");
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
