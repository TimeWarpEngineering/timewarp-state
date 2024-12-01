uid: TimeWarp.State:02-ActionTracking.md
title: TimeWarp.State Action Tracking Tutorial
description: Learn how to implement Action Tracking in TimeWarp.State applications
---

# TimeWarp.State Action Tracking Tutorial

## What is Action Tracking?

Action Tracking is a powerful feature in TimeWarp.State.Plus that allows you to monitor and manage long-running actions in your application. It provides:

- Real-time visibility into active actions
- Ability to track multiple concurrent actions
- Automatic cleanup when actions complete
- Error handling for failed actions

This is particularly useful for:
- Loading states and progress indicators
- Managing concurrent operations
- Monitoring background tasks
- Error handling and recovery

## Demo Video

Check out this quick demonstration of the Action Tracking feature:
[Action Tracking Demo](https://yoclip.app/share/ejqq4l6k52eca8gth9xnm251?pw=uju1sp3lzuszbfmuffe0c9zk)

## Prerequisites

- Completed [Sample00 StateActionHandler tutorial](xref:TimeWarp.State:00-StateActionHandler-Wasm.md)
- Understanding of basic TimeWarp.State concepts (State, Actions, and Handlers)

## Implementation Steps

### 1. Complete Sample00 Tutorial

First, complete the [Sample00 StateActionHandler tutorial](xref:TimeWarp.State:00-StateActionHandler-Wasm.md), but use `Sample02Wasm` as the project name:

```bash
dotnet new blazorwasm -n Sample02Wasm --use-program-main
```

Follow all steps in the Sample00 tutorial until you have a working counter application. This will be our starting point for adding Action Tracking.

### 2. Add TimeWarp.State.Plus Package

Add the TimeWarp.State.Plus NuGet package to your project:

```bash
dotnet add package TimeWarp.State.Plus --prerelease
```

### 3. Configure Services

Update your Program.cs to register both your application assembly and the TimeWarp.State.Plus assembly, and configure the ActiveActionBehavior:

```csharp
namespace Sample02Wasm;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        
        builder.Services.AddTimeWarpState
        (
            options =>
            {
                options.Assemblies = new[]
                {
                    typeof(Program).Assembly,
                    typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly
                };
            }
        );

        // Register the ActiveActionBehavior for action tracking
        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ActiveActionBehavior<,>));

        await builder.Build().RunAsync();
    }
}
```

### 4. Create Demo State and Actions

Create a new file `Features/Demo/DemoState.cs`:

```csharp
namespace Sample02Wasm.Features.Demo;

internal sealed partial class DemoState : State<DemoState>
{
    public override void Initialize() { }
}
```

Create `Features/Demo/DemoState.TwoSecondAction.cs`:

```csharp
namespace Sample02Wasm.Features.Demo;

partial class DemoState
{
    public static class TwoSecondActionSet
    {
        [TrackAction]
        public sealed class Action : IAction { }
        
        public sealed class Handler : ActionHandler<Action>
        {
            public Handler(IStore store) : base(store) { }

            public override async Task Handle
            (
                Action action,
                CancellationToken cancellationToken
            )
            {
                // Simulate a 2-second action
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }
}
```

Create `Features/Demo/DemoState.FiveSecondAction.cs`:

```csharp
namespace Sample02Wasm.Features.Demo;

partial class DemoState
{
    public static class FiveSecondActionSet
    {
        [TrackAction]
        public sealed class Action : IAction { }
        
        public sealed class Handler : ActionHandler<Action>
        {
            public Handler(IStore store) : base(store) { }

            public override async Task Handle
            (
                Action action,
                CancellationToken cancellationToken
            )
            {
                // Simulate a 5-second action
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}
```

### 5. Create Demo Page

Create `Pages/Demo.razor`:

```razor
@page "/demo"
@using Sample02Wasm.Features.Demo
@using TimeWarp.Features.ActionTracking
@inherits TimeWarpStateComponent

<PageTitle>Action Tracking Demo</PageTitle>

<h1>Action Tracking Demo</h1>

<div class="mb-3">
    <h3>Action Status</h3>
    <p><strong>Any Active Actions:</strong> @ActionTrackingState.IsActive</p>
    <p><strong>Two Second Action Running:</strong> @IsTwoSecondActionRunning</p>
    <p><strong>Five Second Action Running:</strong> @IsFiveSecondActionRunning</p>
</div>

<div class="mb-3">
    <button class="btn btn-primary me-2" @onclick="StartTwoSecondAction">
        Start 2-Second Action
    </button>
    <button class="btn btn-primary" @onclick="StartFiveSecondAction">
        Start 5-Second Action
    </button>
</div>

<div class="mb-3">
    <h3>Active Actions</h3>
    @if (ActionTrackingState.IsActive)
    {
        foreach (var action in ActionTrackingState.ActiveActions)
        {
            <div class="alert alert-info">
                Running: @(action.GetType().FullName.Split("+")[1].Replace("ActionSet.Action", "").Replace("ActionSet", ""))
            </div>
        }
    }
    else
    {
        <p>No active actions</p>
    }
</div>

@code {
    DemoState DemoState => GetState<DemoState>();
    ActionTrackingState ActionTrackingState => GetState<ActionTrackingState>();

    bool IsTwoSecondActionRunning => ActionTrackingState.IsAnyActive
    (
        [typeof(DemoState.TwoSecondActionSet.Action)]
    );

    bool IsFiveSecondActionRunning => ActionTrackingState.IsAnyActive
    (
        [typeof(DemoState.FiveSecondActionSet.Action)]
    );

    private async Task StartTwoSecondAction() => 
        await DemoState.TwoSecond();

    private async Task StartFiveSecondAction() => 
        await DemoState.FiveSecond();
}
```

### 6. Update Navigation

Update the navigation menu in `Shared/NavMenu.razor` to include the Demo page:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="demo">
        <span class="oi oi-timer" aria-hidden="true"></span> Demo
    </NavLink>
</div>
```

## Best Practices

1. **Action Granularity**
   - Track significant operations that affect UI state
   - Avoid tracking very short-lived actions
   - Consider grouping related actions

2. **Error Handling**
   - The tracking behavior automatically handles exceptions
   - Actions are removed from tracking even if they fail
   - Implement proper error handling in your action handlers

3. **Performance Considerations**
   - Action tracking has minimal overhead
   - The tracking state is efficiently updated
   - Use specific action type checks when possible

## Troubleshooting

### Common Issues

1. **Actions Not Being Tracked**
   - Verify the `[TrackAction]` attribute is applied
   - Ensure TimeWarp.State.Plus assembly is registered in Program.cs
   - Check if the action inherits from IAction
   - Verify ActiveActionBehavior is registered as a pipeline behavior

2. **Actions Stuck in Tracking**
   - Ensure proper cancellation token handling
   - Verify the action handler completes properly
   - Check for unhandled exceptions

3. **Multiple Instances of Same Action**
   - This is expected behavior if the same action is dispatched multiple times
   - Implement logic to prevent duplicate actions if needed

## Advanced Usage

### Track Multiple Action Types

You can track multiple types of actions:

```razor
@code {
    bool IsAnyActionRunning => ActionTrackingState.IsAnyActive
    (
        [
            typeof(DemoState.TwoSecondActionSet.Action),
            typeof(DemoState.FiveSecondActionSet.Action)
        ]
    );
}
```

### Custom Action Tracking UI

Create reusable components for action tracking:

```razor
@inherits TimeWarpStateComponent

<div class="action-tracker">
    @if (ActionTrackingState.IsActive)
    {
        foreach (var action in ActionTrackingState.ActiveActions)
        {
            <div class="action-item">
                <span class="action-name">@GetActionDisplayName(action)</span>
                <div class="spinner-border spinner-border-sm"></div>
            </div>
        }
    }
</div>

@code {
    private string GetActionDisplayName(IAction action) =>
        action.GetType().Name.Replace("Action", "");
}
```

This tutorial demonstrates how to add Action Tracking to an existing TimeWarp.State application. The example shows how to track and monitor actions of varying durations, providing real-time feedback to users about the application's state.
