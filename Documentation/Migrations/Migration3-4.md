---
uid: BlazorState:Migration3-4.md
title: Migrate From 3.X to 4.X
---

# Migration

## From 3.X to 4.X

These changes correspond to [Migrating from ASP.NET Core 3.1 to 5.0](https://docs.microsoft.com/en-us/aspnet/core/migration/31-to-50)

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