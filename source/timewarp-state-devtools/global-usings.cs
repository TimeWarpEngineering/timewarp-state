#region Purpose
// Project-wide global usings for the DevTools Blazor app.
#endregion

#region Design
// One list so individual files need no using directives.
#endregion

global using System.Collections.Immutable;
global using System.Globalization;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Cors.Infrastructure;
global using Microsoft.Extensions.Configuration;
global using Microsoft.JSInterop;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Net.Http.Headers;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.AspNetCore.Hosting.StaticWebAssets;
global using Microsoft.Extensions.DependencyInjection;
global using TimeWarp.Mediator;
global using TimeWarp.Mediator.Generated;
global using TimeWarp.State;
global using TimeWarp.State.DevTools;
global using TimeWarp.State.DevTools.Export;
global using TimeWarp.State.DevTools.Features.Inspector;
