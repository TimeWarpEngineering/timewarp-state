#region Purpose
// Record the current URI on RouteState, truncating back to it when that URI is already in the stack.
#endregion

#region Design
// Same-URL-on-top updates the title in place (page re-render). An older matching URL pops everything
// above it so sidebar revisits and browser Back shrink the breadcrumb instead of duplicating.
// No stack-depth cap: truncate-on-revisit is the unbounded-duplicate fix; unique-URL growth is a real
// trail (TwBreadcrumb.MaxLinks already limits display). Dropping oldest would hide Home. Revisit if
// a consumer reports unique-URL blowup.
// ReplaceCurrent drops only the top entry when its URL differs, then records the current URL.
// Entries under that top stay. A page uses it to swap one section's crumb (style-guide part to
// part) without growing the trail. BelowUrl and BelowTitle record a parent immediately under the
// current entry, resolved with NavigationManager.ToAbsoluteUri. History under that parent is not
// truncated: a buried copy of the parent stays buried. When both are set and the current URL is
// already on top (TwPageTitle pushed it on first render), that entry is lifted and the sibling
// under it is the one ReplaceCurrent drops, so the parent still sits directly under the page.
// The no-argument call keeps truncate-or-push.
// The page title comes from the document-title.js module export. eval is not used.
// A disconnected circuit or a host without the script records an empty title and still pushes the URL.
// Route updates are not serialized by a store semaphore. The transaction clone is the concurrency story.
#endregion

namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class PushRouteInfoActionSet
  {
    public sealed class Action : IAction
    {
      /// <summary>
      /// When true, drop the top entry if its URL differs from the current URL, then record
      /// the current URL. Entries under that top stay.
      /// </summary>
      public bool ReplaceCurrent { get; }

      /// <summary>Route recorded immediately under the current entry. Relative or absolute.</summary>
      public string? BelowUrl { get; }

      /// <summary>Title of <see cref="BelowUrl"/>. Ignored when <see cref="BelowUrl"/> is null.</summary>
      public string? BelowTitle { get; }

      public Action(bool replaceCurrent = false, string? belowUrl = null, string? belowTitle = null)
      {
        ReplaceCurrent = replaceCurrent;
        BelowUrl = belowUrl;
        BelowTitle = belowTitle;
      }
    }

    public sealed class Handler : StateActionHandler<Action>
    {
      private readonly NavigationManager NavigationManager;
      private readonly IJSRuntime JsRuntime;
      public Handler(NavigationManager navigationManager, IJSRuntime jsRuntime, IStore store) : base(store)
      {
        NavigationManager = navigationManager;
        JsRuntime = jsRuntime;
      }
      private RouteState RouteState => Store.GetState<RouteState>();

      public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        string currentUri = NavigationManager.Uri;
        string title = await ReadDocumentTitleAsync(cancellationToken);
        if (action.ReplaceCurrent || action.BelowUrl is not null)
        {
          RecordReplacingOrUnder(action, currentUri, title);
          return;
        }

        TruncateToOrPush(currentUri, title);
      }

      private void RecordReplacingOrUnder(Action action, string currentUri, string title)
      {
        if (action.BelowUrl is null)
        {
          if (action.ReplaceCurrent
              && RouteState.RouteStack.Count > 0
              && RouteState.RouteStack.Peek().Url != currentUri)
          {
            RouteState.RouteStack.Pop();
          }

          TruncateToOrPush(currentUri, title);
          return;
        }

        string belowAbsolute = NavigationManager.ToAbsoluteUri(action.BelowUrl).ToString();
        if (RouteState.RouteStack.Count > 0 && RouteState.RouteStack.Peek().Url == currentUri)
        {
          RouteState.RouteStack.Pop();
        }

        // The current URL was lifted, or it was never on top. The entry now on top is the
        // sibling ReplaceCurrent should drop. Leave the parent in place.
        if (action.ReplaceCurrent
            && RouteState.RouteStack.Count > 0
            && RouteState.RouteStack.Peek().Url != belowAbsolute)
        {
          RouteState.RouteStack.Pop();
        }

        string belowTitle = action.BelowTitle ?? string.Empty;
        if (RouteState.RouteStack.Count == 0 || RouteState.RouteStack.Peek().Url != belowAbsolute)
        {
          RouteState.RouteStack.Push(new RouteInfo(belowAbsolute, belowTitle));
        }
        else if (RouteState.RouteStack.Peek().PageTitle != belowTitle)
        {
          RouteState.RouteStack.Pop();
          RouteState.RouteStack.Push(new RouteInfo(belowAbsolute, belowTitle));
        }

        RouteState.RouteStack.Push(new RouteInfo(currentUri, title));
      }

      private async Task<string> ReadDocumentTitleAsync(CancellationToken cancellationToken)
      {
        const string modulePath = "./_content/TimeWarp.State.Plus/js/document-title.js";
        try
        {
          await using IJSObjectReference module = await JsRuntime.InvokeAsync<IJSObjectReference>
          (
            "import",
            cancellationToken,
            modulePath
          );
          return await module.InvokeAsync<string>("getDocumentTitle", cancellationToken);
        }
        catch (JSDisconnectedException)
        {
          return string.Empty;
        }
        catch (JSException)
        {
          return string.Empty;
        }
        catch (InvalidOperationException)
        {
          // Prerender and tests that have no JavaScript runtime.
          return string.Empty;
        }
      }

      private void TruncateToOrPush(string url, string title)
      {
        RouteInfo[] routes = RouteState.RouteStack.ToArray();
        int matchIndex = Array.FindIndex(routes, routeInfo => routeInfo.Url == url);
        if (matchIndex >= 0)
        {
          for (int i = 0; i <= matchIndex; i++)
          {
            RouteState.RouteStack.Pop();
          }
        }

        RouteState.RouteStack.Push(new RouteInfo(url, title));
      }
    }
  }
}
