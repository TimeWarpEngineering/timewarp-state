#region Purpose
// AddTimeWarpStateBlazor registers render subscriptions, JavaScript dispatch, and the server HttpClient.
#endregion

#region Design
// TryAdd and JavaScriptDispatchRegistry.GetOrAdd make the call idempotent and order-independent
// relative to UseReduxDevTools and AddJavaScriptDispatch. An HttpClient the host already registered
// is left alone. Browser hosts already have HttpClient, so this method skips that registration there.
#endregion

namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers Blazor features on top of <c>AddTimeWarpState</c>:
  /// render subscriptions, JavaScript dispatch, and a server-side <see cref="HttpClient"/>
  /// whose base address is <see cref="NavigationManager.BaseUri"/>.
  /// </summary>
  /// <remarks>
  /// Call this from Blazor hosts. Without it, <c>RenderSubscriptionsPostProcessor</c> cannot be
  /// constructed and components do not re-render after an action.
  /// </remarks>
  public static IServiceCollection AddTimeWarpStateBlazor(this IServiceCollection serviceCollection)
  {
    ArgumentNullException.ThrowIfNull(serviceCollection);

    serviceCollection.TryAddScoped<JsonRequestHandler>();
    JavaScriptDispatchRegistry.GetOrAdd(serviceCollection);
    serviceCollection.TryAddScoped<RenderSubscriptionContext>();
    EnsureServerHttpClient(serviceCollection);

    return serviceCollection;
  }

  private static void EnsureServerHttpClient(IServiceCollection serviceCollection)
  {
    // WebAssembly hosts register HttpClient themselves.
    if (OperatingSystem.IsBrowser()) return;

    serviceCollection.TryAddScoped
    (
      serviceProvider =>
      {
        NavigationManager navigationManager = serviceProvider.GetRequiredService<NavigationManager>();

        return new HttpClient
        {
          BaseAddress = new Uri(navigationManager.BaseUri)
        };
      }
    );
  }
}
