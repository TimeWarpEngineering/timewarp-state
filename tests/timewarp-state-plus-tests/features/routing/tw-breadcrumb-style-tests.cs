#region Purpose
// Guard: TwBreadcrumb owns its CSS and must not copy Bootstrap's breadcrumb class contract.
#endregion

// ReSharper disable UnusedType.Global
namespace TwBreadcrumbStyle_;

public class TwBreadcrumb_Should
{
  public static void Not_Use_Bootstrap_Class_Contract()
  {
    string razorPath = ComponentPath("TwBreadcrumb.razor");
    string cssPath = ComponentPath("TwBreadcrumb.razor.css");

    File.Exists(cssPath).ShouldBeTrue($"expected isolated CSS at {cssPath}");

    string razor = File.ReadAllText(razorPath);
    string markup = System.Text.RegularExpressions.Regex.Replace
    (
      razor,
      @"@\*.*?\*@",
      string.Empty,
      System.Text.RegularExpressions.RegexOptions.Singleline
    );
    markup.ShouldNotContain("class=\"breadcrumb\"");
    markup.ShouldNotContain("class=\"breadcrumb-item");
    markup.ShouldNotContain("text-muted");
    markup.ShouldContain("aria-label=\"breadcrumb\"");
    markup.ShouldContain("tw-breadcrumb");
    markup.ShouldContain("aria-current");

    string css = File.ReadAllText(cssPath);
    css.ShouldContain(".tw-breadcrumb");
    css.ShouldContain("display: flex");
    css.ShouldContain("list-style: none");
  }

  private static string ComponentPath(string fileName)
  {
    DirectoryInfo? directoryInfo = new(AppContext.BaseDirectory);
    while (directoryInfo is not null)
    {
      string candidate = Path.Combine
      (
        directoryInfo.FullName,
        "source",
        "timewarp-state-plus",
        "features",
        "routing",
        "components",
        fileName
      );
      if (File.Exists(candidate))
      {
        return candidate;
      }

      directoryInfo = directoryInfo.Parent;
    }

    throw new InvalidOperationException($"Could not locate {fileName} from {AppContext.BaseDirectory}.");
  }
}
