# TimeWarp State Action Analyzer

## Overview
The TimeWarp State Action Analyzer enforces the architectural pattern where Action types must be nested within their corresponding State types. This analyzer helps maintain a clean and consistent codebase by ensuring proper organization of state-related code.

## Purpose
This analyzer validates that any class, record, or struct implementing `IAction` is defined as a nested type within a class that implements `IState`. This enforces a clean architectural boundary and maintains clear relationships between Actions and their States.

## Diagnostic Rules

### TW0001: Action Must Be Nested in State
- **Severity**: Error
- **Title**: TimeWarp.State Action should be a nested type of its State
- **Description**: TimeWarp.State Actions should be nested types of their corresponding States.
- **Message Format**: The Action '{0}' is not a nested type of its State

### TWD001: Debug Information
- **Severity**: Info
- **Category**: Debug
- **Description**: Provides debug information during analyzer execution. Useful for troubleshooting and development.
- **Usage**: Not intended for end users

## Implementation Details

The analyzer:
1. Registers for syntax node analysis of:
   - Class declarations
   - Record declarations
   - Struct declarations

2. For each type declaration, it:
   - Checks if the type implements `IAction` (directly or through inheritance)
   - Verifies the type is not abstract
   - Ensures the type is nested within a class implementing `IState`

## Key Methods

- `Initialize`: Sets up the analyzer configuration and registers syntax node actions
- `AnalyzeTypeDeclaration`: Main analysis logic for type declarations
- `ImplementsIAction`: Recursively checks if a type implements IAction
- `IsNestedInIState`: Verifies if the type is nested within an IState implementation

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
// ❌ Invalid - Action not nested in State
public record InvalidAction : IAction { }

// ❌ Invalid - Action not nested in State
public class MyAction : IAction { }

// ❌ Invalid - Action not nested in State, even with inheritance
public abstract class BaseAction : IAction { }
public class ConcreteAction : BaseAction { }
```

## Testing

The analyzer includes comprehensive tests covering various scenarios:
- Invalid record actions
- Invalid class actions
- Invalid struct actions
- Invalid descendant class actions

See `TimeWarpStateActionAnalyser_Tests.cs` for specific test cases.

## Best Practices
1. Always nest action types within their corresponding state
2. Use appropriate type declarations (class/record/struct) based on your needs
3. Leverage inheritance when sharing common action behavior
4. Keep actions focused and specific to their parent state
