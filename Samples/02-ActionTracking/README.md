---
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

## Prerequisites

- An existing TimeWarp.State application
- TimeWarp.State.Plus package

## Implementation Steps

### 1. Install TimeWarp.State.Plus Package

Add the TimeWarp.State.Plus NuGet package to your project:

```bash
dotnet add package TimeWarp.State.Plus --prerelease
```

### 2. Configure Services

Update your Program.cs to add TimeWarp.State.Plus services:

```csharp
builder.Services.AddTimeWarpState();
builder.Services.AddTimeWarpStatePlus(); // Adds Action Tracking and other Plus features
```

### 3. Create Trackable Actions

To make an action trackable, add the `[TrackAction]` attribute to your action class:

```csharp
namespace YourApp.Features.YourFeature;

partial class YourState
{
    public static class LongRunningTaskActionSet
    {
        [TrackAction] // Mark the action for tracking
        public sealed class Action : IAction
        {
            public int Duration { get; }
            
            public Action(int duration)
            {
                Duration = duration;
            }
        }
        
        public sealed class Handler : ActionHandler<Action>
        {
            public Handler(IStore store) : base(store) { }

            public override async Task Handle(Action action, CancellationToken cancellationToken)
            {
                // Simulate long-running task
                await Task.Delay(action.Duration, cancellationToken);
            }
        }
    }
}
```

### 4. Access Action Tracking State

In your components, inject and use the ActionTrackingState to monitor active actions:

```razor
@using TimeWarp.Features.ActionTracking
@inherits TimeWarpStateComponent

<h3>Active Actions</h3>

@if (ActionTrackingState.IsActive)
{
    <p>Currently running actions:</p>
    @foreach(IAction action in ActionTrackingState.ActiveActions)
    {
        <p>@action.GetType().Name</p>
    }
}
else
{
    <p>No active actions</p>
}

@code {
    ActionTrackingState ActionTrackingState => GetState<ActionTrackingState>();

    private async Task StartLongRunningTask()
    {
        await YourState.LongRunningTask(duration: 5000); // 5 seconds
    }
}
```

### 5. Track Specific Action Types

You can check if specific types of actions are running:

```razor
@code {
    bool IsLoadingData => ActionTrackingState.IsAnyActive
    (
        [
            typeof(LoadDataActionSet.Action),
            typeof(RefreshDataActionSet.Action)
        ]
    );
}
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
   - Ensure TimeWarp.State.Plus services are registered
   - Check if the action inherits from IAction

2. **Actions Stuck in Tracking**
   - Ensure proper cancellation token handling
   - Verify the action handler completes properly
   - Check for unhandled exceptions

3. **Multiple Instances of Same Action**
   - This is expected behavior if the same action is dispatched multiple times
   - Implement logic to prevent duplicate actions if needed

## Example Implementation

Here's a complete example of a tracked action:

```csharp
public sealed partial class ApplicationState : State<ApplicationState>
{
    public static class FiveSecondTaskActionSet
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
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    public Task FiveSecondTask() => 
        Sender.Send(new FiveSecondTaskActionSet.Action());
}
```

And its usage in a component:

```razor
@if (ActionTrackingState.IsActive)
{
    <div class="alert alert-info">
        Processing @ActionTrackingState.ActiveActions.Count action(s)...
    </div>
}

<button @onclick=FiveSecondTaskButtonClick>
    Start 5-Second Task
</button>

@code {
    private async Task FiveSecondTaskButtonClick() =>
        await ApplicationState.FiveSecondTask();
}
```

## Advanced Usage

### Concurrent Action Management

You can track multiple actions concurrently:

```csharp
bool IsBusy => ActionTrackingState.IsActive;
bool IsLoadingUsers => ActionTrackingState.IsAnyActive([typeof(LoadUsersAction)]);
bool IsLoadingOrders => ActionTrackingState.IsAnyActive([typeof(LoadOrdersAction)]);
bool IsAnyLoading => ActionTrackingState.IsAnyActive
(
    [
        typeof(LoadUsersAction),
        typeof(LoadOrdersAction)
    ]
);
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

This tutorial demonstrates the core concepts and implementation details of Action Tracking in TimeWarp.State applications. By following these patterns, you can effectively manage and monitor long-running operations in your application.
