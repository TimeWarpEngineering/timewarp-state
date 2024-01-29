namespace BlazorState.Services;

/// <summary>
/// Indicates when the app is pre-rendering
/// </summary>
/// <remarks>
/// It isn't ecapsulated at all as it doesn't determine the value.
/// Instead it is set by the BlazorStateComponent or a custom one.
/// </remarks>
public class RenderPhaseService
{
  // I need an Id property of type GUID
  public Guid Guid { get; set; } = Guid.NewGuid();
    
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>
  /// If IsBrowser is true, we are in a Blazor WebAssembly environment, and it's not pre-rendering.
  /// If IsBrowser is false, we are either on the server or pre-rendering.
  /// </remarks>
  public bool IsPreRendering { get; private set; } = !OperatingSystem.IsBrowser();

  public void SetInteractive()
  {
    if(IsPreRendering) Console.WriteLine("Interactive");
    IsPreRendering = false;
  }
}
