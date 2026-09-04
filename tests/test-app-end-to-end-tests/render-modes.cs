namespace Test.App.EndToEnd.Tests;

public static class RenderModes
{
  public const string Server = "Server";
  // Blazor RendererInfo.Name for interactive WASM is "WebAssembly" (.NET 9+).
  public const string Wasm = "WebAssembly";
  public const string Static = "Static";
  // Add other render modes as needed
}
