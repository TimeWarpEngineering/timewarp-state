---
uid: TimeWarp.State:02-ActionTracking.md
title: Adding Action Tracking to TimeWarp.State
renderMode: WebAssembly
description: Learn how to implement action tracking in TimeWarp.State for monitoring long-running operations
---

# Adding Action Tracking to TimeWarp.State

This tutorial demonstrates how to implement action tracking in your TimeWarp.State application. Action tracking allows you to monitor the state and progress of actions in your application, which is particularly useful for managing long-running operations, showing loading indicators, and handling concurrent operations.

## Prerequisites

- Completed [Sample01 ReduxDevTools tutorial](xref:TimeWarp.State:01-ReduxDevTools.md)
- Understanding of basic TimeWarp.State concepts (State, Actions, and Handlers)
- Familiarity with TimeWarp.State.Plus package

## Implementation Overview

Action tracking in TimeWarp.State consists of several key components:

1. **ActionTrackingState**: Maintains the state of tracked actions
2. **TrackActionAttribute**: Marks actions for tracking
3. **ActionTrackingBehavior**: Handles the lifecycle of tracked actions

## Getting Started

1. Add the TimeWarp.State.Plus package (if not already added):
```bash
dotnet add package TimeWarp.State.Plus --prerelease
```

2. Enable Action Tracking in Program.cs:
```csharp
builder.Services.AddTimeWarpStatePlus();
```

## Tracking Actions

1. Mark actions for tracking using the TrackActionAttribute:
```csharp
[TrackAction]
public class LongRunningAction : IAction
{
    public Guid ActionGuid { get; init; } = Guid.NewGuid();
    // Additional properties
}
```

2. Implement the action handler:
```csharp
internal class LongRunningActionHandler : ActionHandler<LongRunningAction>
{
    public override async Task Handle
    (
        LongRunningAction action,
        CancellationToken cancellationToken
    )
    {
        // Simulate long-running operation
        await Task.Delay(5000, cancellationToken);
        
        // Perform actual work
        // ...
    }
}
```

## Monitoring Active Actions

1. Create a component to display active actions:
```razor
@inherits TimeWarpStateComponent

@inject IState<ActionTrackingState> ActionTrackingState

<h3>Active Actions</h3>

@foreach (var action in ActionTrackingState.Value.ActiveActions)
{
    <div class="action-item">
        <span>@action.Key</span>
        <span>Started: @action.Value.StartedUtc.ToLocalTime()</span>
    </div>
}
```

2. Style the active actions display:
```css
.action-item {
    margin: 10px;
    padding: 10px;
    border: 1px solid #ddd;
    border-radius: 4px;
}
```

## Example Implementation

The example below demonstrates a complete implementation:

1. Create a counter with delayed increment:
```csharp
[TrackAction]
public record DelayedIncrementAction : IAction
{
    public Guid ActionGuid { get; init; } = Guid.NewGuid();
}

internal class DelayedIncrementActionHandler : ActionHandler<DelayedIncrementAction>
{
    private readonly IState<CounterState> CounterState;

    public DelayedIncrementActionHandler(IState<CounterState> counterState)
    {
        CounterState = counterState;
    }

    public override async Task Handle
    (
        DelayedIncrementAction action,
        CancellationToken cancellationToken
    )
    {
        await Task.Delay(3000, cancellationToken);
        CounterState.Value = CounterState.Value with { Count = CounterState.Value.Count + 1 };
    }
}
```

2. Update the counter component:
```razor
@page "/counter"
@inherits TimeWarpStateComponent

@inject IState<CounterState> CounterState
@inject IDispatcher Dispatcher

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<p role="status">Current count: @CounterState.Value.Count</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>
<button class="btn btn-secondary" @onclick="DelayedIncrement">Delayed Increment</button>

<ActiveActions />

@code {
    private void IncrementCount()
    {
        var action = new IncrementCounterAction();
        Dispatcher.Dispatch(action);
    }

    private void DelayedIncrement()
    {
        var action = new DelayedIncrementAction();
        Dispatcher.Dispatch(action);
    }
}
```

## Key Features

1. **Automatic Tracking**: Actions marked with [TrackAction] are automatically tracked
2. **Lifecycle Management**: Actions are tracked from dispatch to completion
3. **Error Handling**: Failed actions are marked with error information
4. **Concurrent Operations**: Multiple actions can be tracked simultaneously
5. **Real-time Updates**: UI automatically updates as actions progress

## Best Practices

1. **Selective Tracking**: Only track actions that benefit from monitoring
2. **Meaningful Names**: Use clear action names for better tracking
3. **Error Handling**: Implement proper error handling in action handlers
4. **UI Feedback**: Use tracking state to show appropriate loading indicators
5. **Resource Management**: Clean up resources when actions complete

## Common Issues and Solutions

1. **Actions Not Being Tracked**
   - Verify [TrackAction] attribute is applied
   - Ensure TimeWarp.State.Plus is properly configured
   - Check action implements IAction correctly

2. **Actions Stuck in Tracking**
   - Verify proper exception handling
   - Check for cancellation token usage
   - Ensure action handler completes properly

3. **UI Not Updating**
   - Verify component inherits from TimeWarpStateComponent
   - Check ActionTrackingState injection
   - Ensure proper component lifecycle handling

## Next Steps

- Implement custom tracking UI
- Add timeout handling
- Integrate with loading indicators
- Add error handling and retry logic
- Implement action cancellation

This implementation demonstrates how Action Tracking enhances the development experience with TimeWarp.State by providing powerful operation monitoring and management capabilities.
