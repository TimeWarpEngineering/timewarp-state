#region Purpose
// Global usings for the telemetry sample (ASP.NET Core, OpenTelemetry tracing, TimeWarp.State.Telemetry).
#endregion

#region Design
// Global using directives only; no types.
#endregion

global using System.Text.Json;
global using System.Text.Json.Serialization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Hosting.StaticWebAssets;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.Extensions.DependencyInjection;
global using OpenTelemetry.Trace;
global using Sample04Server.Components;
global using Sample04Server.Features.Counter;
global using TimeWarp.Mediator;
global using TimeWarp.State;
global using TimeWarp.State.DevTools.Export;
global using TimeWarp.State.Telemetry;
