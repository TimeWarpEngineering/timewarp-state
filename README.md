[![Stars](https://img.shields.io/github/stars/TimeWarpEngineering/timewarp-state?logo=github)](https://github.com/TimeWarpEngineering/timewarp-state)
[![workflow](https://github.com/TimeWarpEngineering/timewarp-state/actions/workflows/release-build.yml/badge.svg)](https://github.com/TimeWarpEngineering/timewarp-state/actions)
[![Forks](https://img.shields.io/github/forks/TimeWarpEngineering/timewarp-state)](https://github.com/TimeWarpEngineering/timewarp-state)
[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-state.svg?style=flat-square&logo=github)](https://github.com/TimeWarpEngineering/timewarp-state/issues)
[![Issues Open](https://img.shields.io/github/issues/TimeWarpEngineering/timewarp-state.svg?logo=github)](https://github.com/TimeWarpEngineering/timewarp-state/issues)
[![OpenSSF Scorecard](https://api.scorecard.dev/projects/github.com/TimeWarpEngineering/timewarp-state/badge)](https://scorecard.dev/viewer/?uri=github.com/TimeWarpEngineering/timewarp-state)

[![nuget](https://img.shields.io/nuget/v/TimeWarp.State?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State/)
[![nuget](https://img.shields.io/nuget/dt/TimeWarp.State?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State/)
[![nuget](https://img.shields.io/nuget/dt/TimeWarp.State?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State/)

[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Fgithub.com%2FTimeWarpEngineering%2Ftimewarp-state)](https://twitter.com/intent/tweet?url=https://github.com/TimeWarpEngineering/timewarp-state)
[![Dotnet](https://img.shields.io/badge/dotnet-8.0-blue)](https://dotnet.microsoft.com)

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
[![Twitter](https://img.shields.io/twitter/follow/StevenTCramer.svg)](https://twitter.com/intent/follow?screen_name=StevenTCramer)
[![Twitter](https://img.shields.io/twitter/follow/TheFreezeTeam1.svg)](https://twitter.com/intent/follow?screen_name=TheFreezeTeam1)

<img src="https://raw.githubusercontent.com/TimeWarpEngineering/timewarpengineering.github.io/refs/heads/master/images/LogoNoMarginNoShadow.svg" alt="logo" height="120" style="float: right" />

# TimeWarp.State

**TimeWarp.State** (previously known as Blazor-State) is a fully asynchronous state management library for Blazor applications, leveraging the MediatR pipeline to implement the Flux pattern. It handles both Reducers and Effects consistently using async Handlers, simplifying the management of asynchronous operations throughout your app.

By utilizing the MediatR pipeline, TimeWarp.State enables a flexible, middleware-driven architecture for managing state, similar to the request-processing pipeline in ASP.NET. This approach allows developers to inject custom behaviors, such as logging, validation, and caching, directly into the state management flow.

In addition to the core library, we offer **[TimeWarp.State.Plus](/Source/TimeWarp.State.Plus)**, which extends the functionality with enhanced middleware, components, and tools to further streamline state management in complex Blazor applications.

## Give a Star! :star:

If you find this project useful, please give it a star. Thanks!

## Getting Started

I recommend the [tutorial](xref:TimeWarp.State:00-StateActionHandler.md) for a step-by-step guide to building a Blazor app with TimeWarp.State.

See full [documentation](https://timewarpengineering.github.io/timewarp-state/).

<img src="https://raw.githubusercontent.com/TimeWarpEngineering/timewarp-state/refs/heads/master/Documentation/Images/TimeWarpStateOneWayFlow.drawio.svg" alt="logo" height="400" style="" />

## Installation

```console
dotnet add package TimeWarp.State
dotnet add package TimeWarp.State.Plus
```

Check out the latest NuGet packages on the [TimeWarp Enterprises NuGet page](https://www.nuget.org/profiles/TimeWarp.Enterprises).

* [TimeWarp.State](https://www.nuget.org/packages/TimeWarp.State/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State/)
* [TimeWarp.State.Plus](https://www.nuget.org/packages/TimeWarp.State.Plus/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State.Plus?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Plus/)

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

