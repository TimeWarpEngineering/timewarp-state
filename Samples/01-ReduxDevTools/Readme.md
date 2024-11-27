---
uid: TimeWarp.State:01-ReduxDevTools.md
title: Adding Redux DevTools to TimeWarp.State
renderMode: WebAssembly
description: Learn how to integrate Redux DevTools with TimeWarp.State for enhanced debugging
---

# Adding Redux DevTools to TimeWarp.State

This tutorial demonstrates how to add Redux DevTools support to your TimeWarp.State Blazor WebAssembly application. Redux DevTools provides powerful debugging capabilities, allowing you to monitor state changes, inspect actions, and understand your application's behavior.

## Prerequisites

- Completed [Sample00 StateActionHandler tutorial](xref:TimeWarp.State:00-StateActionHandler-Wasm.md)
- Understanding of basic TimeWarp.State concepts (State, Actions, and Handlers)
- [Redux DevTools Extension](https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd) installed in Chrome

## Enable Redux DevTools

1. Update Program.cs to enable Redux DevTools:

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTimeWarpState
(
    options =>
    {
        options.UseReduxDevTools(); // Enable Redux DevTools
        options.Assemblies = new Assembly[]
        {
            typeof(Program).Assembly,
        };
    }
);

await builder.Build().RunAsync();
```

## Set up JavaScript Interop

Create App.razor.cs in the same directory as App.razor to handle JavaScript interop initialization:

```csharp
using Microsoft.AspNetCore.Components;
using TimeWarp.State.Pipeline.ReduxDevTools;
using TimeWarp.State.Features.JavaScriptInterop;
using TimeWarp.State.Features.Routing;

namespace Sample00Wasm;

public partial class App : ComponentBase
{
    [Inject] private JsonRequestHandler JsonRequestHandler { get; set; } = null!;
    [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; } = null!;

    [Inject]
    private RouteManager RouteManager { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ReduxDevToolsInterop.InitAsync();
            await JsonRequestHandler.InitAsync();
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
```

## Using Redux DevTools

1. Run the application:
```bash
dotnet run
```

2. Open Chrome DevTools (F12) and navigate to the Redux tab.

3. Interact with the Counter page and observe in Redux DevTools:

![Redux DevTools Interface](../../Documentation/Images/ReduxDevTools.png)

As shown above, Redux DevTools provides a comprehensive view of:
- Actions being dispatched (left panel)
- Current state tree (right panel)
- Action payloads and timestamps
- State changes over time

The interface also shows the RouteState being tracked:

![Route State in Redux DevTools](../../Documentation/Images/ReduxRouteState.png)

This view demonstrates how TimeWarp.State automatically maintains route information in the state tree, making it easy to debug navigation-related issues.

### Key Features in Redux DevTools

1. **Action List**: View a chronological list of dispatched actions
2. **State Tree**: Inspect the current state structure
3. **Action Details**: See the payload and effects of each action
4. **Time Travel**: Jump to any previous state
5. **Route State**: Monitor navigation changes through RouteState

### Debugging Tips

1. Use the "State" tab to inspect current values
2. Check "Action" tab for payload details
3. Use time-travel debugging to reproduce issues
4. Monitor RouteState for navigation-related bugs
5. Export/Import state for sharing bug reports

## Common Issues and Solutions

1. **Redux DevTools Not Showing**
   - Ensure the Chrome extension is installed
   - Verify UseReduxDevTools() is called in Program.cs
   - Check browser console for JavaScript errors

2. **Actions Not Appearing**
   - Confirm JavaScript interop initialization in App.razor.cs
   - Verify inheritance from TimeWarpStateComponent
   - Check action handler registration

3. **State Not Updating**
   - Ensure proper action dispatch
   - Verify handler implementation
   - Check for exceptions in browser console

## Next Steps

- Explore advanced Redux DevTools features
- Implement complex state management patterns
- Add custom middleware for logging
- Integrate with existing debugging workflows

This implementation demonstrates how Redux DevTools enhances the development experience with TimeWarp.State by providing powerful debugging and state inspection capabilities.
