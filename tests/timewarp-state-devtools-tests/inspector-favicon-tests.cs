#region Purpose
// Proves the inspector host page links the TimeWarp PNG and serves those bytes.
#endregion

#region Design
// WebApplicationFactory renders App.razor and MapStaticAssets. The head link is static markup,
// so the first response carries it. logo.png is the MSBuild link to assets/logo.png, not a second
// file under wwwroot. The wordmark SVG is not linked: at 16 px and 32 px it does not read as an icon.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class InspectorFavicon_Should
{
  public static async Task ServeTheBrandPngFromTheHostPage()
  {
    string repo = RepoPaths.Root();
    File.Exists(Path.Combine(repo, "source", "timewarp-state-devtools", "wwwroot", "logo.png")).ShouldBeFalse();
    File.Exists(Path.Combine(repo, "assets", "logo.png")).ShouldBeTrue();

    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();

    string body = await client.GetStringAsync("/");
    int headStart = body.IndexOf("<head", StringComparison.OrdinalIgnoreCase);
    int headEnd = body.IndexOf("</head>", headStart, StringComparison.OrdinalIgnoreCase);
    headStart.ShouldBeGreaterThanOrEqualTo(0);
    headEnd.ShouldBeGreaterThan(headStart);
    string head = body[headStart..headEnd];
    head.ShouldContain("rel=\"icon\"");
    head.ShouldContain("type=\"image/png\"");
    head.ShouldContain("href=\"logo.png\"");
    head.ShouldNotContain("image/svg+xml");
    head.ShouldNotContain("apple-touch-icon");

    using HttpResponseMessage icon = await client.GetAsync("/logo.png");
    icon.StatusCode.ShouldBe(HttpStatusCode.OK);
    icon.Content.Headers.ContentType?.MediaType.ShouldBe("image/png");
    byte[] served = await icon.Content.ReadAsByteArrayAsync();
    served.ShouldBe(File.ReadAllBytes(Path.Combine(repo, "assets", "logo.png")));
  }
}
