---
uid: TimeWarp.State.Installation.md
title: Installation
---

## Installation

Requirements: consuming projects target `net11.0` and build with a .NET 11 SDK. The analyzers and source generators in the package compile against Roslyn 5.12 (`Microsoft.CodeAnalysis.CSharp` 5.12.0), so the compiler must be Roslyn 5.12 or later: .NET SDK 11.0.100 (GA) or later. .NET 11 preview and RC SDKs are not supported. See [12.0.0-beta.18](xref:TimeWarpState:Release.12.0.0-beta.18.md).

```console
dotnet add package TimeWarp.State
dotnet add package TimeWarp.State.Blazor
dotnet add package TimeWarp.State.Plus
```

Blazor hosts:

```csharp
builder.Services.AddTimeWarpState();
builder.Services.AddTimeWarpStateBlazor();
```

Console hosts reference `TimeWarp.State` only and call `AddTimeWarpState`. See the [12.0.0-beta.10 migration](../migrations/migration12.0.0-beta.10.md).

Check out the latest NuGet packages on the [TimeWarp Enterprises NuGet page](https://www.nuget.org/profiles/TimeWarp.Enterprises).

* [TimeWarp.State](https://www.nuget.org/packages/TimeWarp.State/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State/)
* [TimeWarp.State.Blazor](https://www.nuget.org/packages/TimeWarp.State.Blazor/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State.Blazor?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Blazor/)
* [TimeWarp.State.Plus](https://www.nuget.org/packages/TimeWarp.State.Plus/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State.Plus?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Plus/)
