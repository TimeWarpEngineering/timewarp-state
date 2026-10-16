#region Purpose
// Project-wide global usings for the browser DevTools exporter.
#endregion

#region Design
// One list so individual files, including the shared ingest sources, need no using directives.
// OpenTelemetry is intentionally absent: this package does not reference it.
#endregion

global using System.Diagnostics;
global using System.Net.Http;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Threading.Channels;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using Microsoft.Extensions.DependencyInjection;
global using TimeWarp.State.DevTools.Export;
global using TimeWarp.State.DevTools.Wasm;
