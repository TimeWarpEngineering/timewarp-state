#region Purpose
// Project-wide global usings for TimeWarp.State.Plus.
#endregion

#region Design
// One list so individual files need no using directives. Timer is aliased to System.Timers.Timer instead of
// importing System.Timers, because the implicit System.Threading using would make Timer ambiguous.
#endregion

global using Blazored.LocalStorage;
global using Blazored.SessionStorage;
global using TimeWarp.State;
global using TimeWarp.State.Extensions;
global using TimeWarp.Features.Persistence;
global using JetBrains.Annotations;
global using TimeWarp.Mediator;
global using Microsoft.AspNetCore.Components;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.JSInterop;
global using System.Reflection;
global using System.Resources;
global using System.Runtime.CompilerServices;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using TimeWarp.State.Plus;
global using TimeWarp.Features.Routing;
global using Timer = System.Timers.Timer;
