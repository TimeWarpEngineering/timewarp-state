# TimeWarp State Action Analyzer

## Overview
The TimeWarp State Action Analyzer enforces the architectural pattern where Action types must be nested within their corresponding State types. This analyzer helps maintain a clean and consistent codebase by ensuring proper organization of state-related code.

## Rules

### TW0001 - Action Nesting Rule
- **Severity**: Error
- **Category**: TimeWarp.State
- **Description**: Actions in TimeWarp.State must be defined as nested types within their corresponding State classes.

### TWD001 - Debug Diagnostic
- **Severity**: Info
- **Category**: Debug
- **Description**: Provides debug information during analyzer execution. Useful for troubleshooting and development.

## Valid Code Examples

### Class-based Actions
```csharp
public class CounterState : IState
{
    public class IncrementAction : IAction { }
}
```

### Record-based Actions
```csharp
public class WeatherState : IState
{
    public record UpdateTemperatureAction(double Temperature) : IAction;
}
```

### Struct-based Actions
```csharp
public class GameState : IState
{
    public struct MoveAction : IAction 
    {
        public int X { get; init; }
        public int Y { get; init; }
    }
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

## Invalid Code Examples
```csharp
// Error: Action not nested in State
public class IncrementAction : IAction { }

// Error: Action not nested in State
public record UpdateTemperatureAction(double Temperature) : IAction;

// Error: Action not nested in State, even with inheritance
public abstract class BaseAction : IAction { }
public class ConcreteAction : BaseAction { }
```

## Implementation Details
The analyzer performs several sophisticated checks:

1. **Type Detection**
   - Analyzes classes, records, and structs
   - Identifies implementations of `IAction` interface
   - Exempts abstract classes from the nesting requirement

2. **Inheritance Handling**
   - Recursively checks base types for `IAction` implementation
   - Allows for complex inheritance hierarchies while maintaining nesting rules

3. **State Validation**
   - Traverses the type hierarchy to find parent types
   - Verifies parent types implement `IState` interface
   - Ensures proper nesting relationship

4. **Debug Support**
   - Includes debug diagnostic (TWD001) for development
   - Provides detailed context during analysis
   - Helps identify complex analysis scenarios

## Configuration
- Rules are enabled by default
- No configuration options are currently supported
- Debug diagnostics can be filtered using standard analyzer configuration

## Best Practices
1. Always nest action types within their corresponding state
2. Use appropriate type declarations (class/record/struct) based on your needs
3. Leverage inheritance when sharing common action behavior
4. Keep actions focused and specific to their parent state
