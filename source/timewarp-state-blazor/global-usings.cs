#region Purpose
// Project-wide global usings for TimeWarp.State.Blazor.
#endregion

#region Design
// One list so individual files need no using directives. Namespaces match the types that moved
// out of TimeWarp.State, plus the BCL namespaces those files used from TimeWarp.State's global
// usings that the Razor SDK does not import.
#endregion

global using JetBrains.Annotations;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Forms;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.JSInterop;
global using System.Collections;
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Runtime.Serialization;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using TimeWarp.Features.Developer;
global using TimeWarp.Features.StateTransactions;
global using TimeWarp.Features.JavaScriptInterop;
global using TimeWarp.Features.ReduxDevTools;
global using TimeWarp.Features.RenderSubscriptions;
global using TimeWarp.Mediator;
global using TimeWarp.State;
global using TimeWarp.State.Extensions;
