#region Purpose
// Project-wide global usings for the DevTools exporter.
#endregion

#region Design
// One list so individual files need no using directives.
#endregion

global using System.Diagnostics;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Hosting;
global using OpenTelemetry;
global using OpenTelemetry.Trace;
global using TimeWarp.State.DevTools.Export;
