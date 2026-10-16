#region Purpose
// Proves the rendered inspector names the product TimeWarp DevTools and labels the State plugin.
#endregion

#region Design
// WebApplicationFactory prerenders the host page. HeadOutlet writes PageTitle into that first
// response, and the header markup is in the same body. The State label is one item in a plugin
// row. There is no plugin registry and no second plugin.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class InspectorProductName_Should
{
  public static async Task RenderTheProductNameAndTheStatePlugin()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    string body = await client.GetStringAsync("/");

    ElementText(body, "<title", "</title>").ShouldBe("TimeWarp DevTools");
    ElementText(body, "data-qa=\"brand\"", "</h1>").ShouldBe("TimeWarp DevTools");
    body.ShouldContain("aria-label=\"Plugins\"");
    ElementText(body, "data-qa=\"plugin\"", "</span>").ShouldBe("State");
    body.ShouldContain("Current state, and the action that produced it.");
  }

  private static string ElementText(string body, string marker, string endTag)
  {
    int markerIndex = body.IndexOf(marker, StringComparison.Ordinal);
    markerIndex.ShouldBeGreaterThanOrEqualTo(0);
    int contentStart = body.IndexOf('>', markerIndex);
    contentStart.ShouldBeGreaterThan(markerIndex);
    contentStart++;
    int endIndex = body.IndexOf(endTag, contentStart, StringComparison.Ordinal);
    endIndex.ShouldBeGreaterThan(contentStart);
    return body[contentStart..endIndex].Trim();
  }
}
