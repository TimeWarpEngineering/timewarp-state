#region Purpose
// AddTimeWarpStateBlazor registers render subscriptions and JavaScript dispatch.
// It does not register HttpClient. A server host that needs one calls AddHttpClient or registers its own.
#endregion

#region Design
// TryAdd and JavaScriptDispatchRegistry.GetOrAdd make the call idempotent and order-independent
// relative to UseReduxDevTools and AddJavaScriptDispatch.
#endregion

namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers Blazor features on top of <c>AddTimeWarpState</c>:
  /// render subscriptions and JavaScript dispatch.
  /// This method does not register <see cref="HttpClient"/>. A server host that injects one registers it,
  /// for example <c>builder.Services.AddScoped(sp =&gt; new HttpClient { BaseAddress = new Uri(sp.GetRequiredService&lt;NavigationManager&gt;().BaseUri) })</c>
  /// or <c>AddHttpClient</c>.
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

    return serviceCollection;
  }
}
