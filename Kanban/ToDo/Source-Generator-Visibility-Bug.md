# Task: Fix ActionSetMethodGenerator Visibility Bug

## Description
The ActionSetMethodGenerator is currently generating public methods regardless of the state class's visibility. This causes compilation errors when the state class is internal but the generated method is public, resulting in conflicting accessibility.

## Requirements
- The generated method's visibility should match the containing state class's visibility
- If state class is internal, generated method should be internal
- If state class is public, generated method should be public

## Implementation Notes
- Issue found in ActionSetMethodGenerator.cs line 82
- Currently hardcoded to generate public methods:
```csharp
public async Task {{methodName}}
```
- Need to extract state class visibility and use it in method generation
- Affects all generated action methods

## Steps to Reproduce
1. Create an internal state class
2. Add an ActionSet
3. Compilation fails with CS0262: "Partial declarations have conflicting accessibility modifiers"

## Example
```csharp
// State class
internal sealed partial class CounterState : State<CounterState>
{
    // ...
}

// Generated method (incorrectly public)
public async Task IncrementCount() // Should be internal to match state class
{
    // ...
}
```

## Files to Modify
- Source/TimeWarp.State.SourceGenerator/ActionSetMethodGenerator.cs

## Checklist
- [ ] Extract state class visibility in source generator
- [ ] Update GenerateMethodCode to use correct visibility
- [ ] Add tests for different visibility scenarios
- [ ] Update documentation if needed
- [ ] Test with internal and public state classes
