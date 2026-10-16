#region Purpose
// WebAssembly entry for the telemetry sample. Posts store spans to DevTools in Development.
#endregion

#region Design
// The server owns the document. This host does not add a root component. In Development it asks
// the server for the inspector origin (GET /_timewarp/state-devtools) and registers the browser
// exporter. Snapshots turn on only when that origin resolves. An absent inspector leaves the
// store running with no exporter. Release publish of an app can omit this package; this sample
// keeps the call visible.
#endregion

namespace Sample04Wasm;

public class Program
{
  public static async Task Main(string[] args)
  {
    WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.Services.AddGeneratedMediator<ClientPipeline>();

    string? endpoint = await TimeWarpStateDevToolsWasm.ResolveEndpointAsync
    (
      builder.HostEnvironment.IsDevelopment(),
      builder.Configuration[TimeWarpStateDevToolsWasm.EndpointConfigurationKey],
      builder.HostEnvironment.BaseAddress
    );

    builder.Services.AddTimeWarpState();
    builder.Services.AddTimeWarpStateBlazor();
    builder.Services.AddTimeWarpStateTelemetry
    (
      options =>
      {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
          return;
        }

        options.IncludeSnapshots = true;
        options.JsonSerializerOptions = Sample04WasmJsonContext.Default.Options;
      }
    );
    builder.Services.AddTimeWarpStateDevToolsWasmExporter(endpoint);

    await builder.Build().RunAsync();
  }
}
