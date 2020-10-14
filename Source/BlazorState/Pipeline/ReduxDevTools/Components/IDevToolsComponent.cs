namespace BlazorState.Pipeline.ReduxDevTools
{
  /// <summary>
  /// Implementation is required to allow DevTools to ReRender components
  /// When using Time Travel
  /// </summary>
  public interface IDevToolsComponent
  {
    Subscriptions Subscriptions { get; set; }
  }
}