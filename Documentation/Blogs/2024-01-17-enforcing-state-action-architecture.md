# Enforcing State-Action Architecture with TimeWarp.State Analyzer

TimeWarp.State enforces a clean architectural pattern where Actions must be nested within their corresponding State types. This design decision helps maintain clear boundaries and relationships between States and their Actions. Let's explore how the TimeWarp.State Analyzer enforces this pattern through static code analysis.

## Why Nest Actions in States?

Nesting Actions within their State classes provides several benefits:

1. **Clear Ownership**: Each Action explicitly belongs to a specific State
2. **Encapsulation**: Actions are tightly coupled with their State's implementation
3. **Discoverability**: Developers can easily find all Actions that affect a State
4. **Type Safety**: The compiler ensures Actions are properly scoped

## The Analyzer in Action

The TimeWarp.State Analyzer enforces this pattern through the `TW0001` diagnostic rule. Here are some examples:

### ✅ Valid Code

```csharp
public class CounterState : IState
{
    public int Count { get; private set; }

    // Correctly nested Action
    public class IncrementAction : IAction { }
}
```

### ❌ Invalid Code

```csharp
// Error TW0001: The Action 'IncrementAction' is not a nested type of its State
public class IncrementAction : IAction { }
```

## How It Works

The analyzer:

1. Identifies types implementing `IAction`
2. Verifies they are nested within a type implementing `IState`
3. Reports violations as compilation errors

This ensures architectural compliance at compile-time rather than runtime.

## Advanced Scenarios

The analyzer handles various scenarios:

### Record-based Actions
```csharp
public class WeatherState : IState
{
    public record UpdateTemperatureAction(double Temperature) : IAction;
}
```

### Inherited Actions
```csharp
public class BaseState : IState
{
    public abstract class BaseAction : IAction { }
    
    public class ConcreteAction : BaseAction { }
}
```

## Best Practices

1. Name Actions clearly to indicate their purpose
2. Keep Actions focused and specific to their parent State
3. Use appropriate type declarations (class/record/struct) based on needs
4. Consider inheritance for shared Action behavior

## Conclusion

The TimeWarp.State Analyzer helps maintain clean architecture by enforcing proper Action nesting. This design pattern promotes maintainable and discoverable code while preventing common architectural mistakes at compile-time.

For more information, check out the [TimeWarp.State documentation](https://timewarpengineering.github.io/timewarp-state/).
