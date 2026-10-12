#region Purpose
// Scoped render-kind record that Blazor components fill from RendererInfo.
#endregion

#region Design
// One instance per DI scope, which is one Store. Prerender and the interactive circuit are
// different scopes, so each store gets its own kind. First Observe wins.
#endregion

namespace TimeWarp.State;

/// <summary>
/// <see cref="IStateRenderKind"/> for a Blazor scope.
/// </summary>
public sealed class StateRenderKind : IStateRenderKind
{
  /// <inheritdoc />
  public string? Name { get; private set; }

  /// <inheritdoc />
  public void Observe(bool isInteractive)
  {
    Name ??= isInteractive ? StateRenderKinds.Interactive : StateRenderKinds.Prerender;
  }
}
