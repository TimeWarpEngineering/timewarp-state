# TimeWarpStateActionAnalyzer

The `TimeWarpStateActionAnalyzer` is a Roslyn diagnostic analyzer that enforces TimeWarp.State architectural conventions by ensuring Actions are properly nested within their corresponding State classes.

## Purpose

This analyzer validates that any class, record, or struct implementing `IAction` is defined as a nested type within a class that implements `IState`. This enforces a clean architectural boundary and maintains clear relationships between Actions and their States.

## Diagnostic Rules

### TW0001: Action Must Be Nested in State
- **Severity**: Error
- **Title**: TimeWarp.State Action should be a nested type of its State
- **Description**: TimeWarp.State Actions should be nested types of their corresponding States.
- **Message Format**: The Action '{0}' is not a nested type of its State

### TWD001: Debug Information (Internal)
- **Severity**: Info
- **Purpose**: Provides debug information during development of the analyzer
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

## Usage Example

```csharp
// ❌ Invalid - Action not nested in State
public record InvalidAction : IAction { }

// ✅ Valid - Action properly nested in State
public class MyState : State<MyState>
{
    public record MyAction : IAction { }
}
```

## Testing

The analyzer includes comprehensive tests covering various scenarios:
- Invalid record actions
- Invalid class actions
- Invalid struct actions
- Invalid descendant class actions

See `TimeWarpStateActionAnalyser_Tests.cs` for specific test cases.
