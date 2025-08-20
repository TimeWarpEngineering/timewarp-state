---
uid: BlazorState:Migration2-3.md
title: Migrate From 2.X to 3.X
---

# Migration

## From 2.X to 3.X

These changes correspond to [updating to .NET Core 3.1](https://devblogs.microsoft.com/aspnet/asp-net-core-updates-in-net-core-3-1/)

### Update the script tag in your index.html or _Host.cshtml files

From:

```html
<script src="_content/BlazorState.js/blazorstate.js"></script>
```

To:

```html
<script src="_content/Blazor-State/blazorstate.js"></script>
```

### Add `UseStaticFiles()` in Server `Startup.cs`

Insert:

Insert into the Server side `Startup.cs` file `Configure` method:

```csharp
aApplicationBuilder.UseStaticFiles();
```

This should be prior to the call to `UseClientSideBlazorFiles`

### Change TargetFramework to 3.1

In your csproj files change,

From:

```xml
<TargetFramework>netcoreapp3.0</TargetFramework>
```

To:

```xml
<TargetFramework>netcoreapp3.1</TargetFramework>
```