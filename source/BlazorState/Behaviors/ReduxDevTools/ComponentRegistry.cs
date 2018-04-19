namespace BlazorState.Behaviors.ReduxDevTools
{
  using System.Collections.Generic;

  /// <summary>
  /// Keeps collection of components so we can cause areRender 
  /// from an Handler
  /// </summary>
  public class ComponentRegistry
  {
    public ComponentRegistry()
    {
      DevToolsComponents = new List<IDevToolsComponent>();
    }
    public List<IDevToolsComponent> DevToolsComponents { get; }

    public void ReRenderAll() => DevToolsComponents.ForEach(c => c.ReRender());
  }
}
