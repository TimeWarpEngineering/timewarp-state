# MultiTimer System

The MultiTimer System is a feature of TimeWarp.State.Plus that provides a flexible framework for managing multiple timers in your application. This system can be used for various purposes such as implementing timeout functionality, scheduling regular events, auto-save features, or any scenario where you need to track and respond to timed events.

## Features

- Manage multiple timers simultaneously
- Configure each timer independently
- Optionally reset timers on activity
- Publish notifications when timers elapse
- Seamless integration with TimeWarp.State.Plus and dependency injection

## Usage

### Configuration

Configure your timers in your `appsettings.json` file:

```json
{
  "MultiTimerOptions": {
    "Timers": {
      "ActivityWarningTimer": {
        "Duration": 300000,
        "ResetOnActivity": true
      },
      "SessionExpirationTimer": {
        "Duration": 600000,
        "ResetOnActivity": true
      },
      "PeriodicBackupTimer": {
        "Duration": 900000,
        "ResetOnActivity": false
      }
    }
  }
}
```

### Registering Services

In your `Startup.cs` or wherever you configure your services:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<MultiTimerOptions>(Configuration.GetSection(nameof(MultiTimerOptions)));
    services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(MultiTimerPostProcessor<,>));
    
    // Register your notification handlers
    services.AddScoped<INotificationHandler<TimerElapsedNotification>, YourTimerElapsedHandler>();
}
```

### Implementing a Notification Handler

Create a handler to respond to timer elapsed events:

```csharp
public class YourTimerElapsedHandler : INotificationHandler<TimerElapsedNotification>
{
    private readonly ILogger<YourTimerElapsedHandler> Logger;

    public YourTimerElapsedHandler(ILogger<YourTimerElapsedHandler> logger)
    {
        Logger = logger;
    }

    public Task Handle(TimerElapsedNotification notification, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Timer {TimerName} has elapsed", notification.TimerName);

        switch (notification.TimerName)
        {
            case "ActivityWarningTimer":
                // Show a warning to the user about impending session expiration
                break;
            case "SessionExpirationTimer":
                // End the user's session
                break;
            case "PeriodicBackupTimer":
                // Perform a periodic backup
                // Note: This timer doesn't reset on activity, so it will fire regularly
                break;
        }

        // Optionally restart the timer
        notification.RestartTimer();

        return Task.CompletedTask;
    }
}
```

## How It Works

1. The `TimerState` initializes timers based on the configuration in `MultiTimerOptions`.
2. The `MultiTimerPostProcessor` processes requests and triggers the `ResetTimersOnActivity` ActionSet.
3. The `ResetTimersOnActivityActionSet` resets timers configured to reset on activity.
4. When a timer elapses, a `TimerElapsedNotification` is published.
5. Your notification handler(s) respond to the elapsed timer, performing appropriate actions.

## ActionSets

The system uses ActionSets to manage timer operations:

- `AddTimerActionSet`: Adds a new timer to the `TimerState`.
- `RemoveTimerActionSet`: Removes a timer from the `TimerState`.
- `UpdateTimerActionSet`: Updates an existing timer's configuration.
- `ResetTimersOnActivityActionSet`: Resets timers configured to reset on activity.

To use these ActionSets, you can send actions to the `TimerState`:

```csharp
await TimerState.AddTimer("NewTimer", new TimerConfig { Duration = 5000, ResetOnActivity = true });
await TimerState.ResetTimersOnActivity();
```

## Best Practices

- Use descriptive names for your timers to easily identify their purpose.
- Consider the implications of setting `ResetOnActivity` to false for timers that should run on a fixed schedule.
- Implement proper error handling in your notification handlers.
- Use dependency injection to manage the lifecycle of your handlers and services.

## Troubleshooting

- If timers aren't firing, check your configuration to ensure durations are set correctly (in milliseconds).
- Verify that your notification handlers are properly registered in the dependency injection container.
- Check the logs for any warnings or errors related to timer operations.
