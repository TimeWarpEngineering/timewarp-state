namespace Test.App.EndToEnd.Tests;

public static class ConfiguredRenderModes
{
  public const string InteractiveAutoRenderMode = "InteractiveAutoRenderMode";
  public const string InteractiveServerRenderMode = "InteractiveServerRenderMode";
  public const string InteractiveWebAssemblyRenderMode = "InteractiveWebAssemblyRenderMode";
  public const string None = "None";

  // InteractiveAuto resolves after the component is interactive: AssignedRenderMode.GetType().Name
  // becomes InteractiveServerRenderMode or InteractiveWebAssemblyRenderMode, not InteractiveAutoRenderMode.
  public static string ForCurrentRenderMode(string currentRenderMode)
  {
    return currentRenderMode switch
    {
      RenderModes.Server => InteractiveServerRenderMode,
      RenderModes.Wasm => InteractiveWebAssemblyRenderMode,
      RenderModes.Static => None,
      _ => throw new ArgumentOutOfRangeException(nameof(currentRenderMode), currentRenderMode, "Unknown current render mode.")
    };
  }
}
