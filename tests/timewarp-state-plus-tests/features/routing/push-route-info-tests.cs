#region Purpose
// Regression tests: PushRouteInfo truncates to an existing URL instead of pushing a duplicate,
// and can replace the top entry or record a parent under the current URL.
#endregion

#region Design
// Calls the PushRouteInfo handler directly with a fake store, a NavigationManager whose Uri can be set,
// and an IJSRuntime that answers "import" with a module exporting getDocumentTitle.
// ReplaceCurrent and BelowUrl are the opt-in arguments. The no-argument tests stay on truncate-or-push.
#endregion

// ReSharper disable UnusedType.Global
namespace PushRouteInfo_;

using Microsoft.JSInterop;

public class PushRouteInfo_Should
{
  private const string UrlA = "http://localhost/a";
  private const string UrlB = "http://localhost/b";
  private const string UrlC = "http://localhost/c";

  private sealed class MutableNavigationManager : NavigationManager
  {
    public MutableNavigationManager() => Initialize("http://localhost/", "http://localhost/");

    public void SetUri(string uri) => Uri = uri;

    protected override void NavigateToCore(string uri, bool forceLoad) => Uri = uri;
  }

  private sealed class TitleJsRuntime : IJSRuntime
  {
    public string Title { get; set; } = "";

    public bool ThrowOnImport { get; set; }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
      InvokeAsync<TValue>(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
      if (ThrowOnImport)
      {
        throw new JSDisconnectedException("The circuit disconnected.");
      }

      if (identifier != "import")
      {
        throw new InvalidOperationException($"Unexpected JS identifier: {identifier}");
      }

      return new ValueTask<TValue>((TValue)(object)new TitleModule(Title));
    }

    private sealed class TitleModule(string title) : IJSObjectReference
    {
      public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
        InvokeAsync<TValue>(identifier, CancellationToken.None, args);

      public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
      {
        if (identifier != "getDocumentTitle")
        {
          throw new InvalidOperationException($"Unexpected JS identifier: {identifier}");
        }

        return new ValueTask<TValue>((TValue)(object)title);
      }

      public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
  }

  private static
  (
    RouteState.PushRouteInfoActionSet.Handler Handler,
    MutableNavigationManager Nav,
    TitleJsRuntime JsRuntime,
    RouteState RouteState
  )
    CreateHandler()
  {
    ISender<ClientPipeline> sender = A.Fake<ISender<ClientPipeline>>();
    RouteState routeState = new(sender);

    IStore store = A.Fake<IStore>();
    A.CallTo(() => store.GetState<RouteState>()).Returns(routeState);

    MutableNavigationManager nav = new();
    TitleJsRuntime jsRuntime = new();
    RouteState.PushRouteInfoActionSet.Handler handler = new(nav, jsRuntime, store);
    return (handler, nav, jsRuntime, routeState);
  }

  private static async Task PushAsync
  (
    RouteState.PushRouteInfoActionSet.Handler handler,
    MutableNavigationManager nav,
    TitleJsRuntime jsRuntime,
    string url,
    string title,
    bool replaceCurrent = false,
    string? belowUrl = null,
    string? belowTitle = null
  )
  {
    nav.SetUri(url);
    jsRuntime.Title = title;
    await handler.Handle
    (
      new RouteState.PushRouteInfoActionSet.Action(replaceCurrent, belowUrl, belowTitle),
      CancellationToken.None
    );
  }

  public async Task Truncate_To_Oldest_When_Revisiting_A_After_A_B_C()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync(handler, nav, jsRuntime, UrlA, "A");
    await PushAsync(handler, nav, jsRuntime, UrlB, "B");
    await PushAsync(handler, nav, jsRuntime, UrlC, "C");
    await PushAsync(handler, nav, jsRuntime, UrlA, "A revisited");

    routeState.Routes.Select(routeInfo => routeInfo.Url).ShouldBe([UrlA]);
    routeState.Routes.Single().PageTitle.ShouldBe("A revisited");
  }

  public async Task Truncate_To_B_When_Revisiting_B_After_A_B_C_Without_GoBack()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync(handler, nav, jsRuntime, UrlA, "A");
    await PushAsync(handler, nav, jsRuntime, UrlB, "B");
    await PushAsync(handler, nav, jsRuntime, UrlC, "C");
    await PushAsync(handler, nav, jsRuntime, UrlB, "B via browser Back");

    routeState.Routes.Select(routeInfo => routeInfo.Url).ShouldBe([UrlB, UrlA]);
    routeState.Routes.Select(routeInfo => routeInfo.PageTitle).ShouldBe(["B via browser Back", "A"]);
  }

  public async Task Update_Title_In_Place_When_Pushing_The_Same_Url_Twice()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync(handler, nav, jsRuntime, UrlA, "First");
    await PushAsync(handler, nav, jsRuntime, UrlA, "Updated");

    routeState.Routes.Count().ShouldBe(1);
    routeState.Routes.Single().Url.ShouldBe(UrlA);
    routeState.Routes.Single().PageTitle.ShouldBe("Updated");
  }

  public async Task Still_Push_When_The_Url_Is_New()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync(handler, nav, jsRuntime, UrlA, "A");
    await PushAsync(handler, nav, jsRuntime, UrlB, "B");
    await PushAsync(handler, nav, jsRuntime, UrlC, "C");

    routeState.Routes.Select(routeInfo => routeInfo.Url).ShouldBe([UrlC, UrlB, UrlA]);
  }

  public async Task Push_With_An_Empty_Title_When_JavaScript_Disconnects()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();
    jsRuntime.ThrowOnImport = true;

    await PushAsync(handler, nav, jsRuntime, UrlA, "A");

    routeState.Routes.Single().Url.ShouldBe(UrlA);
    routeState.Routes.Single().PageTitle.ShouldBe(string.Empty);
  }

  public async Task Replace_Current_Drops_Only_The_Top_Entry()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync(handler, nav, jsRuntime, UrlA, "Style Guide");
    await PushAsync(handler, nav, jsRuntime, UrlB, "Brand");
    await PushAsync(handler, nav, jsRuntime, UrlC, "Color Tokens", replaceCurrent: true);

    routeState.Routes.Select(routeInfo => routeInfo.Url).ShouldBe([UrlC, UrlA]);
    routeState.Routes.Select(routeInfo => routeInfo.PageTitle).ShouldBe(["Color Tokens", "Style Guide"]);
  }

  public async Task Replace_Current_On_The_Same_Url_Updates_The_Title()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync(handler, nav, jsRuntime, UrlA, "Style Guide");
    await PushAsync(handler, nav, jsRuntime, UrlB, "Brand");
    await PushAsync(handler, nav, jsRuntime, UrlB, "Brand renamed", replaceCurrent: true);

    routeState.Routes.Select(routeInfo => routeInfo.Url).ShouldBe([UrlB, UrlA]);
    routeState.Routes.Select(routeInfo => routeInfo.PageTitle).ShouldBe(["Brand renamed", "Style Guide"]);
  }

  public async Task Below_Url_On_An_Empty_Stack_Records_The_Parent_Then_The_Page()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync
    (
      handler,
      nav,
      jsRuntime,
      UrlB,
      "Brand",
      belowUrl: "/style-guide",
      belowTitle: "Style Guide"
    );

    routeState.Routes.Select(routeInfo => routeInfo.Url).ShouldBe([UrlB, "http://localhost/style-guide"]);
    routeState.Routes.Select(routeInfo => routeInfo.PageTitle).ShouldBe(["Brand", "Style Guide"]);
  }

  public async Task Below_Url_Lifts_The_Current_Top_So_The_Parent_Sits_Under_It()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync(handler, nav, jsRuntime, UrlB, "Brand");
    await PushAsync
    (
      handler,
      nav,
      jsRuntime,
      UrlB,
      "Brand",
      belowUrl: UrlA,
      belowTitle: "Style Guide"
    );

    routeState.Routes.Select(routeInfo => routeInfo.Url).ShouldBe([UrlB, UrlA]);
    routeState.Routes.Select(routeInfo => routeInfo.PageTitle).ShouldBe(["Brand", "Style Guide"]);
  }

  public async Task Below_Url_When_The_Parent_Is_Already_On_Top_Pushes_The_Page()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync(handler, nav, jsRuntime, UrlA, "Style Guide");
    await PushAsync
    (
      handler,
      nav,
      jsRuntime,
      UrlB,
      "Brand",
      belowUrl: UrlA,
      belowTitle: "Style Guide"
    );

    routeState.Routes.Select(routeInfo => routeInfo.Url).ShouldBe([UrlB, UrlA]);
  }

  public async Task Replace_And_Below_Url_Swap_A_Sibling_And_Keep_The_Parent()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync(handler, nav, jsRuntime, UrlA, "Style Guide");
    await PushAsync(handler, nav, jsRuntime, UrlB, "Brand");
    await PushAsync
    (
      handler,
      nav,
      jsRuntime,
      UrlC,
      "Color Tokens",
      replaceCurrent: true,
      belowUrl: UrlA,
      belowTitle: "Style Guide"
    );

    routeState.Routes.Select(routeInfo => routeInfo.Url).ShouldBe([UrlC, UrlA]);
    routeState.Routes.Select(routeInfo => routeInfo.PageTitle).ShouldBe(["Color Tokens", "Style Guide"]);
  }

  public async Task Replace_And_Below_Url_Drop_The_Sibling_Under_A_Page_Already_On_Top()
  {
    (RouteState.PushRouteInfoActionSet.Handler handler, MutableNavigationManager nav, TitleJsRuntime jsRuntime, RouteState routeState) =
      CreateHandler();

    await PushAsync(handler, nav, jsRuntime, UrlA, "Style Guide");
    await PushAsync(handler, nav, jsRuntime, UrlB, "Brand");
    await PushAsync(handler, nav, jsRuntime, UrlC, "Color Tokens");
    await PushAsync
    (
      handler,
      nav,
      jsRuntime,
      UrlC,
      "Color Tokens",
      replaceCurrent: true,
      belowUrl: UrlA,
      belowTitle: "Style Guide"
    );

    routeState.Routes.Select(routeInfo => routeInfo.Url).ShouldBe([UrlC, UrlA]);
    routeState.Routes.Select(routeInfo => routeInfo.PageTitle).ShouldBe(["Color Tokens", "Style Guide"]);
  }
}
