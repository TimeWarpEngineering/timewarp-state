---
uid: TimeWarp.State.Installation.md
title: Installation
---

## Installation

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

Console hosts reference `TimeWarp.State` only and call `AddTimeWarpState`. See the [13.0.0-beta.1 migration](../migrations/migration13.0.0-beta.1.md).

Check out the latest NuGet packages on the [TimeWarp Enterprises NuGet page](https://www.nuget.org/profiles/TimeWarp.Enterprises).

* [TimeWarp.State](https://www.nuget.org/packages/TimeWarp.State/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State/)
* [TimeWarp.State.Blazor](https://www.nuget.org/packages/TimeWarp.State.Blazor/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State.Blazor?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Blazor/)
* [TimeWarp.State.Plus](https://www.nuget.org/packages/TimeWarp.State.Plus/) [![nuget](https://img.shields.io/nuget/v/TimeWarp.State.Plus?logo=nuget)](https://www.nuget.org/packages/TimeWarp.State.Plus/)
