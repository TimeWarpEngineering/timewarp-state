#region Purpose
// AddJavaScriptDispatch: allow-lists the actions JavaScript may dispatch through timeWarpState.DispatchRequest.
#endregion

#region Design
// Opt-in: nothing is dispatchable until allowed. A builder writes into the shared JavaScriptDispatchRegistry
// (GetOrAdd on the service collection), so call order relative to AddTimeWarpState does not matter.
#endregion

namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  /// <summary>
  /// Allows JavaScript to dispatch the given actions through <c>timeWarpState.DispatchRequest</c>.
  /// Nothing is dispatchable from JavaScript until it is allowed here.
  /// </summary>
  /// <param name="serviceCollection">The service collection.</param>
  /// <param name="configure">Allows actions, for example <c>b => b.Allow&lt;CounterState.IncrementCountActionSet.Action&gt;()</c>.</param>
  /// <remarks>Call again to allow more. Order relative to <see cref="AddTimeWarpState"/> does not matter.</remarks>
  public static IServiceCollection AddJavaScriptDispatch
  (
    this IServiceCollection serviceCollection,
    Action<JavaScriptDispatchBuilder> configure
  )
  {
    ArgumentNullException.ThrowIfNull(serviceCollection);
    ArgumentNullException.ThrowIfNull(configure);

    configure(new JavaScriptDispatchBuilder(JavaScriptDispatchRegistry.GetOrAdd(serviceCollection)));
    return serviceCollection;
  }
}
