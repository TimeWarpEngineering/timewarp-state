---
uid: TimeWarp.State:03-Routing.md
title: TimeWarp.State Routing Tutorial
description: Learn how to implement stack-based routing in TimeWarp.State applications
---

# TimeWarp.State Routing Tutorial

> [!TIP]
> View the complete reference implementation for this tutorial:
> - [Sample03Wasm Project](./Sample03Wasm/)
> - [Program.cs with Routing Configuration](./Sample03Wasm/Program.cs)
> - [MainLayout with TwBreadcrumb](./Sample03Wasm/Layout/MainLayout.razor)
> - [Example Pages with TwPageTitle](./Sample03Wasm/Pages/)

## What is Stack-Based Routing?

Stack-based routing in TimeWarp.State.Plus is a comprehensive navigation management system that provides:

- Maintained history of visited routes
- Breadcrumb-style navigation
- Multi-step back navigation
- Thread-safe route updates
- Automatic page title synchronization

This is particularly useful for:
- Complex navigation flows
- Breadcrumb trail implementation
- History-based navigation
- Concurrent route handling
- Debugging navigation with ReduxDevTools

## Prerequisites

> [!IMPORTANT]
> The tutorials in this series must be completed in sequence. Each tutorial builds upon the previous one:
> 
> 1. First complete [Sample00 StateActionHandler tutorial](xref:TimeWarp.State:00-StateActionHandler.md)
>    - Establishes basic TimeWarp.State concepts
>    - Sets up state management foundation
>    - Implements action/handler pattern
> 
> 2. Then complete [Sample01 ReduxDevTools tutorial](xref:TimeWarp.State:01-ReduxDevTools.md)
>    - Adds TimeWarp.State.Plus package
>    - Integrates Redux DevTools
>    - Sets up required components
> 
> 3. Only then proceed with this routing tutorial
>    - Builds on the established foundation
>    - Uses features from previous tutorials
>
> Attempting to skip tutorials or complete them out of order will result in errors, as each tutorial depends on the setup and concepts from the previous ones.

Additional requirements:
- Understanding of TimeWarp.State concepts (State, Actions, and Handlers)
- [Redux DevTools Extension](https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd) installed in Chrome

## Implementation Steps

### 1. Start from ReduxDevTools Sample

First, complete the [Sample01 ReduxDevTools tutorial](xref:TimeWarp.State:01-ReduxDevTools.md), but use `Sample03Wasm` as the project name:

```pwsh
dotnet new blazorwasm -n Sample03Wasm --use-program-main
```

Follow all steps in the Sample01 tutorial until you have a working application with ReduxDevTools integration.

### 2. Add Required Dependencies

The TimeWarp.State.Plus package should already be installed from the ReduxDevTools tutorial. If not:

```pwsh
dotnet add package TimeWarp.State.Plus --prerelease
```

### 3. Configure Route State

Update your Program.cs to include routing configuration:

```csharp
namespace Sample03Wasm;

public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    builder.Services.AddScoped
    (
      sp => new HttpClient 
      { 
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
      }
    );
    
    builder.Services.AddTimeWarpState
    (
      options =>
      {
        options.UseReduxDevTools();
        options.Assemblies = new[]
        {
          typeof(Program).Assembly,
          typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly
        };
      }
    );

    await builder.Build().RunAsync();
  }
}
```

### 4. Add Navigation Components

Update `Layout/MainLayout.razor` to include the TwBreadcrumb component:

```razor
@inherits LayoutComponentBase

<div class="page">
  <div class="sidebar">
    <NavMenu />
  </div>

  <main>
    <div class="top-row px-4">
      <a href="https://learn.microsoft.com/aspnet/core/" target="_blank">About</a>
    </div>

    <div class="px-4">
      <TwBreadcrumb MaxLinks=3 />
    </div>

    <article class="content px-4">
      @Body
    </article>
  </main>
</div>
```

The `TwBreadcrumb` component from TimeWarp.State.Plus provides:
- Automatic route history tracking
- Configurable number of displayed links through `MaxLinks` parameter
- Built-in back navigation support
- Proper breadcrumb styling

### 5. Update Page Titles

In each of your pages (e.g., Home.razor, Counter.razor, Weather.razor), replace the `PageTitle` component with `TwPageTitle`:

```razor
// Before
<PageTitle>Home</PageTitle>

// After
<TwPageTitle>Home</TwPageTitle>
```

The `TwPageTitle` component:
- Automatically updates the browser's title
- Integrates with the route state
- Maintains proper titles in the breadcrumb trail
- Synchronizes with navigation history

## Best Practices

### 1. Route State Management
- Use `TwPageTitle` consistently across all pages
- Implement `TwBreadcrumb` in your main layout
- Let the routing system handle navigation state
- Avoid manual route manipulation unless necessary

### 2. Thread Safety
- Route updates are automatically synchronized
- Use the built-in concurrent navigation handling
- Leverage semaphore-based protection for custom navigation logic
- Monitor route stack modifications in development

### 3. ReduxDevTools Integration
- Monitor route state changes in real-time
- Use time-travel debugging for navigation issues
- Track route stack modifications
- Analyze navigation patterns during development

## Troubleshooting

### Common Issues and Solutions

1. **Routes Not Updating**
   - Verify RouteState initialization in Program.cs
   - Check NavigationManager service registration
   - Ensure proper action dispatch sequence
   - Validate route state subscription

2. **Breadcrumbs Not Showing**
   - Confirm TwBreadcrumb placement in MainLayout
   - Verify TwPageTitle usage on all pages
   - Check route state updates in ReduxDevTools
   - Validate MaxLinks parameter value

3. **Navigation History Issues**
   - Ensure consistent TwPageTitle implementation
   - Monitor route stack in ReduxDevTools
   - Verify concurrent navigation handling
   - Check for custom navigation interference

### Debugging Tips

1. **Using ReduxDevTools**
   ```csharp
   // Enable detailed logging in development
   builder.Services.AddTimeWarpState
   (
     options =>
     {
       options.UseReduxDevTools
       (
         config =>
         {
           config.EnableDetailedLogs = true;
           config.TraceLimit = 25;
         }
       );
     }
   );
   ```

2. **Route State Inspection**
   ```csharp
   // Inject IState<RouteState> to inspect current route
   @inject IState<RouteState> RouteState

   @code
   {
     protected override void OnInitialized()
     {
       var currentRoute = RouteState.Value.CurrentRoute;
       var routeStack = RouteState.Value.RouteStack;
     }
   }
   ```

This tutorial demonstrates how to implement comprehensive routing features in a TimeWarp.State application using the built-in TwPageTitle and TwBreadcrumb components, while maintaining debuggability through ReduxDevTools integration.
