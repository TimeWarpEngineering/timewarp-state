---
uid: TimeWarp.State:00-StateActionHandler-Wasm.md
title: TimeWarp.State Blazor Interactive WebAssembly Tutorial
renderMode: InteractiveWebAssembly
description: Learn TimeWarp.State basics using Blazor with Interactive WebAssembly render mode
---

# TimeWarp.State Blazor Interactive WebAssembly Tutorial

## State, Actions, and Handlers

This tutorial will walk you through the steps to create a Blazor application with TimeWarp.State using Interactive WebAssembly render mode.

> [!NOTE]
> This tutorial uses Blazor's Interactive WebAssembly render mode. For tutorials covering other render modes (Auto or Server-only), please see the respective tutorials in this series.

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download).

### Creating the Project

1. Create a new Blazor project:
```bash
dotnet new blazor --use-program-main --interactivity WebAssembly -n Sample00Wasm
```

2. Navigate to the project directory:
```bash
cd Sample00Wasm
```

3. Test the application:
```bash
dotnet run --project ./Sample00Wasm/Sample00Wasm.csproj
```

4. Open the URL shown in the command output (e.g., http://localhost:5256) and test the counter functionality.

> [!NOTE]
> The counter resets to zero when you navigate away and return because each time you leave the page,
> the counter component is destroyed.
> When you return, a new instance of the component is created, starting the count afresh.

### Install TimeWarp.State Package

Add the TimeWarp.State NuGet package to the Client project:

```bash
dotnet add ./Sample00Wasm.Client/Sample00Wasm.Client.csproj package TimeWarp.State --prerelease
```

### Configure Services

Create GlobalUsings.cs files to centralize common using statements:

For the Client project:
```csharp
// Sample00Wasm.Client/GlobalUsings.cs
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using Microsoft.Extensions.DependencyInjection;
global using TimeWarp.State;
```

Update Program.cs in the Client project:

```csharp
// Sample00Wasm.Client/Program.cs
namespace Sample00Wasm.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        
        builder.Services.AddTimeWarpState();
        
        await builder.Build().RunAsync();
    }
}
```

### Add Features

1. Create a `Features` folder in the Client project.
2. Inside `Features`, add a `Counter` folder.
3. Inside `Counter`, add `CounterState.cs`:

```csharp
// CounterState.cs
namespace Sample00Wasm.Client.Features.Counter;

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

Modify `Pages/Counter.razor` in the Client project:

```razor
@page "/counter"
@rendermode InteractiveWebAssembly
@using Sample00Wasm.Client.Features.Counter
@inherits TimeWarp.State.TimeWarpStateComponent

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<p role="status">Current count: @currentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    CounterState CounterState => GetState<CounterState>();
    private int currentCount => CounterState.Count;

    private void IncrementCount()
    {
        // Empty for now
    }
}
```

### Implementing State Mutation through Actions and Handlers

Create `CounterState.IncrementCount.cs` in the Counter folder:

```csharp
namespace Sample00Wasm.Client.Features.Counter;

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

### Update Counter Component

Update the `IncrementCount` method in `Counter.razor`:

```razor
private async Task IncrementCount()
{
    await CounterState.IncrementCount(amount: 5);
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