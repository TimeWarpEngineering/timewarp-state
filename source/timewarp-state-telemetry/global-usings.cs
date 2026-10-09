#region Purpose
// Project-wide global usings for TimeWarp.State.Telemetry.
#endregion

#region Design
// One list so individual files need no using directives; no other design decisions.
#endregion

global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Text.Json;
global using System.Text.Json.Serialization.Metadata;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using TimeWarp.Mediator;
global using TimeWarp.State;
global using TimeWarp.State.Extensions;
global using TimeWarp.State.Telemetry;
