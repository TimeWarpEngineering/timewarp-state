#region Purpose
// Names of the ActivitySources the .NET 11 shared framework emits for this host.
#endregion

#region Design
// Subscribing with AddSource is the whole of the framework trace setup. OpenTelemetry.Instrumentation.AspNetCore
// enriches the same HttpRequestIn activity. On .NET 11 the framework sets the semantic tags, and the package
// skips its own when it detects them (AspNetCoreHasNativeOpenTelemetryTags), so the package adds a dependency
// and no spans. WebAssembly uses Microsoft.AspNetCore.Components in the browser. This server process does not
// see those activities. Circuit and SignalR sources are server-only. The names match the framework constants
// in .NET 11 RC3 (Hosting Application Diagnostics, ComponentsActivitySource, CircuitActivitySource,
// SignalRServerActivitySource).
#endregion

namespace Sample04Server;

/// <summary>
/// ActivitySource names emitted by the .NET 11 ASP.NET Core and Blazor shared framework.
/// </summary>
internal static class NativeTraceSources
{
  /// <summary>HTTP server span <c>Microsoft.AspNetCore.Hosting.HttpRequestIn</c>.</summary>
  internal const string AspNetCore = "Microsoft.AspNetCore";

  /// <summary>Blazor component spans. A click is named <c>Event {attribute} -&gt; {component}.{handler}</c>. The WebAssembly host uses this same source.</summary>
  internal const string Components = "Microsoft.AspNetCore.Components";

  /// <summary>Blazor Server circuit start span.</summary>
  internal const string Circuits = "Microsoft.AspNetCore.Components.Server.Circuits";

  /// <summary>SignalR hub spans that carry Blazor Server circuit messages.</summary>
  internal const string SignalRServer = "Microsoft.AspNetCore.SignalR.Server";
}
