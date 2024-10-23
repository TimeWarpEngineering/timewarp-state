# TimeWarp State Action Analyzer

## Overview
The TimeWarp State Action Analyzer enforces the architectural pattern where Action types must be nested within their corresponding State types. This analyzer helps maintain a clean and consistent codebase by ensuring proper organization of state-related code.

## Rule Details

- **Rule ID**: TW0001
- **Severity**: Error
- **Category**: TimeWarp.State

### Description
Actions in TimeWarp.State should be defined as nested types within their corresponding State classes. This rule enforces this pattern by flagging any Action that is not properly nested.

### Valid Code Examples
```csharp
public class CounterState : IState
{
    public class IncrementAction : IAction { }
    public record DecrementAction : IAction { }
    public struct ResetAction : IAction { }
}
```

### Invalid Code Examples
```csharp
// Error: Action not nested in State
public class IncrementAction : IAction { }

// Error: Action not nested in State
public record DecrementAction : IAction { }

// Error: Action not nested in State
public struct ResetAction : IAction { }
```

## Implementation Notes
- The analyzer checks classes, records, and structs that implement `IAction`
- Abstract classes implementing `IAction` are exempt from this rule
- The analyzer recursively checks base types for `IAction` implementation
- Checks parent types for `IState` implementation to validate nesting

## Configuration
This rule is enabled by default and cannot be configured.
