#region Purpose
// Render-mode names the test pages display (RendererInfo.Name).
#endregion

#region Design
// WebAssembly is the .NET 9+ name for interactive WASM.
#endregion

namespace Test.App.EndToEnd.Tests;

public static class RenderModes
{
  public const string Server = "Server";
  // Blazor RendererInfo.Name for interactive WASM is "WebAssembly" (.NET 9+).
  public const string Wasm = "WebAssembly";
  public const string Static = "Static";
  // Add other render modes as needed
}
