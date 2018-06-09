namespace BlazorState.Behaviors.ReduxDevTools
{
  using System.Collections.Generic;

  /// <summary>
  /// Keeps collection of components so we can cause a ReRender
  /// from a Handler
  /// </summary>
  public class ComponentRegistry
  {
    public ComponentRegistry()
    {
      DevToolsComponents = new List<IDevToolsComponent>();
    }

    internal List<IDevToolsComponent> DevToolsComponents { get; }

    public void ReRenderAll() => DevToolsComponents.ForEach(c => c.ReRender());
  }
}