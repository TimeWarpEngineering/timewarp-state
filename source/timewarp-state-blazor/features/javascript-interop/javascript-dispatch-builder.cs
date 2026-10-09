#region Purpose
// Fluent builder used by AddJavaScriptDispatch to allow actions for JavaScript dispatch.
#endregion

#region Design
// Sealed with an internal constructor, so it is only created over the shared JavaScriptDispatchRegistry. The
// non-generic Allow checks IAction at runtime; the generic one enforces it with a constraint.
#endregion

namespace TimeWarp.Features.JavaScriptInterop;

/// <summary>
/// Opts actions in to JavaScript dispatch. See <see cref="ServiceCollectionExtensions.AddJavaScriptDispatch"/>.
/// </summary>
public sealed class JavaScriptDispatchBuilder
{
  private readonly JavaScriptDispatchRegistry Registry;

  internal JavaScriptDispatchBuilder(JavaScriptDispatchRegistry registry)
  {
    Registry = registry;
  }

  /// <summary>
  /// Allows JavaScript to dispatch <typeparamref name="TAction"/> by its full name,
  /// assembly-qualified name, or <paramref name="alias"/>.
  /// </summary>
  /// <param name="alias">Optional short, stable wire name, for example <c>"Counter.Increment"</c>.</param>
  public JavaScriptDispatchBuilder Allow<TAction>(string? alias = null)
  where TAction : class, IAction
  {
    Registry.Add(typeof(TAction), alias);
    return this;
  }

  /// <summary>
  /// Allows JavaScript to dispatch <paramref name="actionType"/>.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="actionType"/> does not implement <see cref="IAction"/>.</exception>
  public JavaScriptDispatchBuilder Allow(Type actionType, string? alias = null)
  {
    ArgumentNullException.ThrowIfNull(actionType);
    if (!typeof(IAction).IsAssignableFrom(actionType))
    {
      throw new ArgumentException($"'{actionType.FullName}' is not an action. Only {nameof(IAction)} types can be dispatched from JavaScript.", nameof(actionType));
    }

    Registry.Add(actionType, alias);
    return this;
  }
}
