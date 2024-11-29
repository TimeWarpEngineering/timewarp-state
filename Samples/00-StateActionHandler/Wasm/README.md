---
uid: TimeWarp.State:00-StateActionHandler-Wasm.md
title: TimeWarp.State Blazor WebAssembly Tutorial
renderMode: WebAssembly
description: Learn TimeWarp.State basics using Blazor WebAssembly
---

# TimeWarp.State Blazor WebAssembly Tutorial

## State, Actions, and Handlers

This tutorial will walk you through the steps to create a Blazor WebAssembly application with TimeWarp.State.

> [!NOTE]
> This tutorial uses Blazor WebAssembly standalone mode. For tutorials covering other modes (Auto or Server), please see the respective tutorials in this series.

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download).

### Creating the Project

1. Create a new Blazor WebAssembly project:
```bash
dotnet new blazorwasm  -n Sample00Wasm
```

2. Navigate to the project directory:
```bash
cd Sample00Wasm
```

3. Test the application:
```bash
dotnet run
```

4. Open the URL shown in the command output (e.g., http://localhost:5000) and test the counter functionality.

> [!NOTE]
> The counter resets to zero when you navigate away and return because each time you leave the page,
> the counter component is destroyed.
> When you return, a new instance of the component is created, starting the count afresh.

### Install TimeWarp.State Package

Add the TimeWarp.State NuGet package:

```bash
dotnet add package TimeWarp.State --prerelease
```

### Configure Services

Create GlobalUsings.cs to centralize common using statements:

```csharp
// GlobalUsings.cs
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using Microsoft.Extensions.DependencyInjection;
global using TimeWarp.State;
```

Update Program.cs to add TimeWarp.State services:

```csharp
// Program.cs
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTimeWarpState();

await builder.Build().RunAsync();
```

### Add Features

1. Create a `Features` folder in the project root.
2. Inside `Features`, add a `Counter` folder.
3. Inside `Counter`, add `CounterState.cs`:

```csharp
// CounterState.cs
namespace Sample00Wasm.Features.Counter;

internal sealed partial class CounterState : State<CounterState>
{
    public int Count { get; private set; }
    
    public override void Initialize()
    {
        Count = 3;
    }
}
```

### UI Integration

Modify `Pages/Counter.razor`:

```razor
@page "/counter"
@using Sample00Wasm.Features.Counter
@inherits TimeWarp.State.TimeWarpStateComponent

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<p role="status">Current count: @currentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    CounterState CounterState => GetState<CounterState>();
    private int currentCount => CounterState.Count;

    private async Task IncrementCount()
    {
        await CounterState.IncrementCount(amount: 5);
    }
}
```

### Implementing State Mutation through Actions and Handlers

Create `CounterState.IncrementCount.cs` in the Counter folder:

```csharp
namespace Sample00Wasm.Features.Counter;

partial class CounterState
{
    public static class IncrementCountActionSet
    {
        public sealed class Action : IAction
        {
            public int Amount { get; }
            
            public Action(int amount)
            {
                Amount = amount;
            }
        }
        
        public sealed class Handler : ActionHandler<Action>
        {
            public Handler(IStore store) : base(store) { }
            
            private CounterState CounterState => Store.GetState<CounterState>();

            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                CounterState.Count += action.Amount;
                return Task.CompletedTask;
            }
        }
    }
}
```

### Validate

Run the application and test the counter functionality:

```bash
dotnet run
```

You should see:
1. The counter starts at 3 (initialized in CounterState)
2. Each click increases the count by 5
3. The state persists during the current browser session
4. If you refresh the page, the state resets

### Key Differences from Server Mode

1. **State Location**: In WebAssembly mode, state is maintained in the browser's memory
2. **Network Independence**: Once loaded, the application can work offline
3. **Performance Characteristics**:
   - Initial load is slower (WebAssembly download)
   - Actions process faster (no network round-trip)
   - Memory usage is limited to browser resources
4. **Browser Limitations**: Must work within browser sandbox constraints

### WebAssembly-Specific Considerations

1. **State Persistence**:
   - State is lost on page refresh by default
   - Consider using browser storage for persistence if needed
2. **Memory Management**:
   - Be mindful of memory usage as it's limited by the browser
   - Consider cleanup strategies for large state objects
3. **Performance**:
   - Minimize state size for better performance
   - Use appropriate data structures for WebAssembly context

This implementation demonstrates how TimeWarp.State works in a WebAssembly context while maintaining the same clean architecture and patterns used in other render modes.
