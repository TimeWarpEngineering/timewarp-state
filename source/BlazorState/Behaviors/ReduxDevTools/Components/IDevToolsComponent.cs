namespace BlazorState.Behaviors.ReduxDevTools
{
  /// <summary>
  /// Implementation is required to allow DevTools to ReRender components
  /// When using Time Travel
  /// </summary>
  /// <example>
  /// </example>
  internal interface IDevToolsComponent
  {
    ComponentRegistry ComponentRegistry { get; set; }

    void ReRender();
  }
}