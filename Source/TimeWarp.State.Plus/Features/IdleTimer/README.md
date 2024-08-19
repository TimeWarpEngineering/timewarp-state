# IdleTimer Feature

The IdleTimer feature provides a flexible system for managing multiple timers in your application. It's particularly useful for implementing idle timeout functionality, auto-save features, or any scenario where you need to track and respond to periods of inactivity.

## Features

- Manage multiple timers simultaneously
- Configure each timer independently
- Automatically reset timers on activity
- Publish notifications when timers elapse
- Easily integrate with dependency injection

## Installation

1. Add the `TimeWarp.State.Plus.Features.IdleTimer` namespace to your project.
2. Register the necessary services in your dependency injection container (see Usage section).

## Usage

### Configuration

First, configure your timers in your `appsettings.json` file:

```json
{
  "MultiTimerOptions": {
    "Timers": {
      "WarningTimer": {
        "Duration": 300000,
        "ResetOnActivity": true
      },
      "LogoutTimer": {
        "Duration": 600000,
        "ResetOnActivity": true
      },
      "AutoSaveTimer": {
        "Duration": 60000,
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
    services.Configure<MultiTimerOptions>(Configuration.GetSection("MultiTimerOptions"));
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
    private readonly ILogger<YourTimerElapsedHandler> _logger;

    public YourTimerElapsedHandler(ILogger<YourTimerElapsedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TimerElapsedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timer {TimerName} has elapsed", notification.TimerName);

        switch (notification.TimerName)
        {
            case "WarningTimer":
                // Show a warning to the user
                break;
            case "LogoutTimer":
                // Log out the user
                break;
            case "AutoSaveTimer":
                // Perform auto-save
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

1. The `MultiTimerPostProcessor` initializes timers based on the configuration in `MultiTimerOptions`.
2. When a request is processed, timers configured to reset on activity are restarted.
3. When a timer elapses, a `TimerElapsedNotification` is published.
4. Your notification handler(s) respond to the elapsed timer, performing appropriate actions.

## Best Practices

- Use descriptive names for your timers to easily identify their purpose.
- Consider the implications of setting `ResetOnActivity` to false for timers that should run on a fixed schedule.
- Implement proper error handling in your notification handlers.
- Use dependency injection to manage the lifecycle of your handlers and services.

## Troubleshooting

- If timers aren't firing, check your configuration to ensure durations are set correctly (in milliseconds).
- Verify that your notification handlers are properly registered in the dependency injection container.
- Check the logs for any warnings or errors related to timer operations.

## Contributing

[Include information about how others can contribute to this feature]

## License

[Include license information for your project]
