[![Dotnet](https://img.shields.io/badge/dotnet-8.0-blue)](https://dotnet.microsoft.com)
[![Stars](https://img.shields.io/github/stars/TimeWarpEngineering/blazor-state?logo=github)](https://github.com/TimeWarpEngineering/timewarp-state)
[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
[![workflow](https://github.com/TimeWarpEngineering/timewarp-state/actions/workflows/release-build.yml/badge.svg)](https://github.com/TimeWarpEngineering/timewarp-state/actions)
[![nuget](https://img.shields.io/nuget/v/TimeWarp.State.Plus?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Plus/)
[![nuget](https://img.shields.io/nuget/dt/TimeWarp.State.Plus?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Plus/)
[![Issues Open](https://img.shields.io/github/issues/TimeWarpEngineering/blazor-state.svg?logo=github)](https://github.com/TimeWarpEngineering/timewarp-state/issues)
[![Forks](https://img.shields.io/github/forks/TimeWarpEngineering/blazor-state)](https://github.com/TimeWarpEngineering/timewarp-state)
[![License](https://img.shields.io/github/license/TimeWarpEngineering/blazor-state.svg?style=flat-square&logo=github)](https://github.com/TimeWarpEngineering/timewarp-state/issues)
[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Fgithub.com%2FTimeWarpEngineering%2Fblazor-state)](https://twitter.com/intent/tweet?url=https://github.com/TimeWarpEngineering/timewarp-state)

[![Twitter](https://img.shields.io/twitter/follow/StevenTCramer.svg)](https://twitter.com/intent/follow?screen_name=StevenTCramer)
[![Twitter](https://img.shields.io/twitter/follow/TheFreezeTeam1.svg)](https://twitter.com/intent/follow?screen_name=TheFreezeTeam1)

# TimeWarp.State.Plus

![TimeWarp Logo](https://raw.githubusercontent.com/TimeWarpEngineering/blazor-state/master/Assets/Logo.svg)

TimeWarp.State.Plus extends Blazor-State with additional, features, middleware and components to simplify and enhance your Blazor applications.

## Available Middleware

- **PersistentState**
  - **Key Feature**: Automates the persistence of state in browser storage.
  - **Usage**: Annotate state classes with `[PersistentState]` to enable.
  - **Storage Options**: Supports both `LocalStorage` and `SessionStorage`.

Example:

```csharp
// Register the required services in Program.cs
// serviceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PersistentStatePostProcessor<,>));
// serviceCollection.AddScoped<IPersistenceService, PersistenceService>();


[PersistentState(PersistentStateMethod.LocalStorage)]
public partial class Counter2State : State<Counter2State>
{
// State implementation
}

```

## Available Components
- **InputColor**
  - **Key Feature**: A color picker input component.
  - **Usage**: `<InputColor @bind-Value="Color" />`
  - **ValueType**: System.Drawing.Color

## Available Features
- **ThemeState**
  - **Key Feature**: Manages the `CurrentTheme` of your application (Light, Dark, System).
  - **Actions**: UpdateTheme 
  
## Give a Star! :star:

If you find this project useful, please give it a star. Thanks!

## Getting Started

For a quick start, refer to the [middleware section](https://timewarpengineering.github.io/timewarp-state/Documentation/Middleware) in the full [Documentation](https://timewarpengineering.github.io/timewarp-state/).

## Installation

```console
dotnet add package TimeWarp.State.Plus
```

Check out the latest NuGet packages on the [TimeWarp NuGet page](https://www.nuget.org/profiles/TimeWarp.Enterprises).

* [TimeWarp.State.Plus](https://www.nuget.org/packages/TimeWarp.State.Plus/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State.Plus?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Plus/)

## Releases

View the [Release Notes](https://timewarpengineering.github.io/timewarp-state/ReleaseNotes/Release11.0.0.html) for detailed information on each release.

## UnLicense

[![License](https://img.shields.io/github/license/TimeWarpEngineering/blazor-state.svg?style=flat-square&logo=github)](https://unlicense.org)  
This project is licensed under the [Unlicense](https://unlicense.org).

## Contributing

Your contributions are welcome! Before starting any work, please open a [discussion](https://github.com/TimeWarpEngineering/timewarp-state/discussions).

Help with the [documentation](https://timewarpengineering.github.io/timewarp-state/) is also greatly appreciated.

## Contact

If you have an issue and don't receive a timely response, feel free to reach out on our [Discord server](https://discord.gg/A55JARGKKP).

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
