[![Stars](https://img.shields.io/github/stars/TimeWarpEngineering/timewarp-state?logo=github)](https://github.com/TimeWarpEngineering/timewarp-state)
[![workflow](https://github.com/TimeWarpEngineering/timewarp-state/actions/workflows/workflow.yml/badge.svg)](https://github.com/TimeWarpEngineering/timewarp-state/actions)
[![Forks](https://img.shields.io/github/forks/TimeWarpEngineering/timewarp-state)](https://github.com/TimeWarpEngineering/timewarp-state)
[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-state.svg?style=flat-square&logo=github)](https://github.com/TimeWarpEngineering/timewarp-state/issues)
[![Issues Open](https://img.shields.io/github/issues/TimeWarpEngineering/timewarp-state.svg?logo=github)](https://github.com/TimeWarpEngineering/timewarp-state/issues)
[![OpenSSF Scorecard](https://api.scorecard.dev/projects/github.com/TimeWarpEngineering/timewarp-state/badge)](https://scorecard.dev/viewer/?uri=github.com/TimeWarpEngineering/timewarp-state)

[![nuget](https://img.shields.io/nuget/v/TimeWarp.State?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State/)
[![nuget](https://img.shields.io/nuget/dt/TimeWarp.State?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State/)

[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Fgithub.com%2FTimeWarpEngineering%2Ftimewarp-state)](https://twitter.com/intent/tweet?url=https://github.com/TimeWarpEngineering/timewarp-state)
[![Dotnet](https://img.shields.io/badge/dotnet-11.0-blue)](https://dotnet.microsoft.com)

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
[![Twitter](https://img.shields.io/twitter/follow/StevenTCramer.svg)](https://twitter.com/intent/follow?screen_name=StevenTCramer)
[![Twitter](https://img.shields.io/twitter/follow/TheFreezeTeam1.svg)](https://twitter.com/intent/follow?screen_name=TheFreezeTeam1)

<img src="https://raw.githubusercontent.com/TimeWarpEngineering/timewarpengineering.github.io/refs/heads/master/images/LogoNoMarginNoShadow.svg" alt="logo" height="120" style="float: right" />

# TimeWarp.State

**TimeWarp.State** (previously known as Blazor-State) is a fully asynchronous state management library. The core package has no Blazor dependency, so console and other non-UI hosts can dispatch actions through the same store. Blazor components, JavaScript interop, render subscriptions, and Redux DevTools are in **TimeWarp.State.Blazor**. It handles both Reducers and Effects consistently using async Handlers.

By utilizing the TimeWarp.Mediator pipeline, TimeWarp.State enables a flexible, middleware-driven architecture for managing state, similar to the request-processing pipeline in ASP.NET. This approach allows developers to inject custom behaviors, such as logging, validation, and caching, directly into the state management flow.

Hosts consume **TimeWarp.Mediator 14-beta** via generated `AddGeneratedMediator<ClientPipeline>()` / named pipelines — not MediatR and not reflection `AddMediator()`.

In addition to the core library, we offer **[TimeWarp.State.Blazor](/source/timewarp-state-blazor)** (components, JavaScript interop, render subscriptions, Redux DevTools), **[TimeWarp.State.Plus](/source/timewarp-state-plus)** (routing, persistence, action tracking; depends on TimeWarp.State.Blazor; package id unchanged), and **[TimeWarp.State.Telemetry](/source/timewarp-state-telemetry)** (OpenTelemetry action spans for the Aspire dashboard or any OTel backend). The [console sample](samples/07-console/overview.md) dispatches an action without Blazor. The [persistence sample](samples/05-persistence/readme.md) shows session storage and local storage with `[PersistentState]`. The [render control sample](samples/06-render-control/readme.md) shows `ShouldRender`, parameter checks, and `RegisterRenderTrigger`.

## Give a Star! :star:

If you find this project useful, please give it a star. Thanks!

## Getting Started

I recommend the [state and action handler sample](samples/00-state-action-handler) for a step-by-step guide to building a Blazor app with TimeWarp.State. The published docs are at [timewarpengineering.github.io/timewarp-state](https://timewarpengineering.github.io/timewarp-state/).

See full [documentation](https://timewarpengineering.github.io/timewarp-state/).

<img src="https://raw.githubusercontent.com/TimeWarpEngineering/timewarp-state/refs/heads/master/documentation/images/time-warp-state-one-way-flow.drawio.svg" alt="logo" height="400" style="" />

## Installation

```console
dotnet add package TimeWarp.State
dotnet add package TimeWarp.State.Blazor
dotnet add package TimeWarp.State.Plus
dotnet add package TimeWarp.State.Telemetry
```

Blazor hosts call `AddTimeWarpState` and `AddTimeWarpStateBlazor`. Console hosts call `AddTimeWarpState` only. See [Migrate to 12.0.0-beta.10](documentation/migrations/migration12.0.0-beta.10.md).

Check out the latest NuGet packages on the [TimeWarp Enterprises NuGet page](https://www.nuget.org/profiles/TimeWarp.Enterprises).

* [TimeWarp.State](https://www.nuget.org/packages/TimeWarp.State/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State/)
* [TimeWarp.State.Blazor](https://www.nuget.org/packages/TimeWarp.State.Blazor/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State.Blazor?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Blazor/)
* [TimeWarp.State.Plus](https://www.nuget.org/packages/TimeWarp.State.Plus/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State.Plus?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Plus/)
* [TimeWarp.State.Telemetry](https://www.nuget.org/packages/TimeWarp.State.Telemetry/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State.Telemetry?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Telemetry/)

## JavaScript dispatch

JavaScript can dispatch actions with `timeWarpState.DispatchRequest(name, request)`. It is opt-in: allow each action explicitly, everything else is rejected.

```csharp
builder.Services.AddJavaScriptDispatch(b => b.Allow<CounterState.IncrementCountActionSet.Action>());
```

See [Enable JavaScript interop](documentation/topics/enable-javascript-interop.md).

## Action catalog

Mark user-facing actions with `[CatalogAction]` to enumerate and execute them at runtime, for example from a Ctrl-K command palette or as agent tools:

```csharp
public static class AddPasskeyActionSet
{
  [CatalogAction(Description = "Add a passkey to the signed-in account.", Permissions = ["credentials.write"])]
  public sealed class Action : IAction
  {
    public Action(string label) { Label = label; }
    public string Label { get; }
  }
}
```

```csharp
builder.Services.AddActionCatalog(typeof(AssemblyMarker).Assembly);

ActionCatalogEntry entry = actionCatalog.Find("Credentials.AddPasskey")!;
await entry.Execute(store, ["Laptop"]);
```

The source generator emits a per-assembly registry with name, description, permissions, visibility, parameters and input schema, plus a reflection-free `Execute`. It is opt-in: unmarked actions are never cataloged. Permissions are opaque ids that the consumer enforces; `[TrackAction]` (busy indicator) is unrelated. See [Action catalog](documentation/topics/action-catalog.md).

## Releases

View the [Release Notes](https://timewarpengineering.github.io/timewarp-state/ReleaseNotes/Release11.0.0.html) for detailed information on each release.

## Unlicense

[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-state.svg?style=flat-square&logo=github)](https://unlicense.org)  
This project is licensed under the [Unlicense](https://unlicense.org).

## Contributing

Your contributions are welcome! Before starting any work, please open a [discussion](https://github.com/TimeWarpEngineering/timewarp-state/discussions).

Help with the [documentation](https://timewarpengineering.github.io/timewarp-state/) is also greatly appreciated.

## Contact

If you have an issue and don't receive a timely response, feel free to reach out on our [Discord server](https://discord.gg/A55JARGKKP).

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)

