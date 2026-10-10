#region Purpose
// Shared imports for the persistence sample, including the .NET 10 PersistentState alias.
#endregion

#region Design
// Global using directives only. The PersistentStateAttribute alias points [PersistentState] at
// TimeWarp.Features.Persistence.
#endregion

global using System.Text.Json.Serialization;
global using Blazored.LocalStorage;
global using Blazored.SessionStorage;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using Microsoft.Extensions.DependencyInjection;
global using TimeWarp.Features.Persistence;
global using TimeWarp.Mediator;
global using TimeWarp.State;
global using TimeWarp.State.Plus;
global using TimeWarp.State.Plus.Extensions;
// .NET 10 added Microsoft.AspNetCore.Components.PersistentStateAttribute. The Blazor WebAssembly
// SDK imports that namespace globally, so [PersistentState] is ambiguous without this alias.
global using PersistentStateAttribute = TimeWarp.Features.Persistence.PersistentStateAttribute;
