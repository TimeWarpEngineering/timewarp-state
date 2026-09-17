#region Purpose
// Regression tests: PushRouteInfo truncates to an existing URL instead of pushing a duplicate.
#endregion

// ReSharper disable UnusedType.Global
namespace PushRouteInfo_;

using FakeItEasy;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeWarp.Mediator;
using TimeWarp.Features.Routing;
using TimeWarp.State;

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

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
      InvokeAsync<TValue>(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
      if (identifier != "eval")
      {
        throw new InvalidOperationException($"Unexpected JS identifier: {identifier}");
      }

      return new ValueTask<TValue>((TValue)(object)Title);
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
    SemaphoreSlim semaphoreSlim = new(1, 1);
    A.CallTo(() => store.GetSemaphore(typeof(RouteState))).Returns(semaphoreSlim);

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
    string title
  )
  {
    nav.SetUri(url);
    jsRuntime.Title = title;
    await handler.Handle(new RouteState.PushRouteInfoActionSet.Action(), CancellationToken.None);
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
}
