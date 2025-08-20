---
uid: BlazorState:Migration4-5.md
title: Migrate From 4.X to 5.X
---

# Migration

## From 4.X to 5.X

The biggest requirement will be to migrate to dotnet 6. See [Migrate from ASP.NET Core 5.0 to 6.0](https://docs.microsoft.com/en-us/aspnet/core/migration/50-to-60?view=aspnetcore-6.0&tabs=visual-studio)

After completing that dotnet 6 migration the blazor-state specific changes are as follows:

### Remove the script tag in your index.html or _Host.cshtml files

Delete:

```html
<script src="_content/BlazorState.js/blazorstate.js"></script>
```
