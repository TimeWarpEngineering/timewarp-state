#region Purpose
// Lets a component record RendererInfo onto IStateRenderKind when the host registered one.
#endregion

#region Design
// IServiceProvider is always in the Blazor container. GetService keeps a component working
// when AddTimeWarpStateBlazor has not registered IStateRenderKind. Missing means no render tag.
// The interactive flag is a delegate so RendererInfo is read only when a kind is registered.
// RendererInfo throws when the component has no renderer; that call is skipped.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Writes <see cref="IStateRenderKind"/> from a component that knows <c>RendererInfo</c>.
/// </summary>
internal static class StateRenderKindObserver
{
  /// <summary>
  /// Records the render kind when an <see cref="IStateRenderKind"/> is registered.
  /// </summary>
  /// <param name="serviceProvider">The component service provider. Null skips the call.</param>
  /// <param name="isInteractive">
  /// Reads whether the renderer is interactive. Invoked only when a kind is registered.
  /// </param>
  internal static void Observe(IServiceProvider? serviceProvider, Func<bool> isInteractive)
  {
    ArgumentNullException.ThrowIfNull(isInteractive);
    if (serviceProvider?.GetService<IStateRenderKind>() is not IStateRenderKind stateRenderKind)
    {
      return;
    }

    bool interactive;
    try
    {
      interactive = isInteractive();
    }
    catch (InvalidOperationException)
    {
      // RendererInfo throws when this component has no renderer.
      return;
    }

    stateRenderKind.Observe(interactive);
  }
}
