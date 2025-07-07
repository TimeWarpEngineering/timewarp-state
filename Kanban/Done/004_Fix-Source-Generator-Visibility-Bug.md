# Task: Fix ActionSetMethodGenerator Visibility Bug

## Description
The ActionSetMethodGenerator is currently declaring the partial class with explicit public visibility, which conflicts with the original class's visibility when it's internal.

## Requirements
- Remove explicit visibility modifier from the partial class declaration in the generator
- Let the partial class inherit visibility from the original class declaration

## Implementation Notes
- Issue found in ActionSetMethodGenerator.cs line 82
- Currently declaring with explicit public visibility:
```csharp
public partial class {{parentClassName}}
```
- Should remove visibility modifier:
```csharp
partial class {{parentClassName}}
```

## Files to Modify
- Source/TimeWarp.State.SourceGenerator/ActionSetMethodGenerator.cs

## Checklist
- [ ] Remove 'public' modifier from partial class declaration
- [ ] Test with internal and public state classes
