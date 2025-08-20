# Task 010: Update PersistenceStateSourceGenerator to Incremental Generator

## Description

Update the PersistenceStateSourceGenerator to implement IIncrementalGenerator instead of ISourceGenerator. This migration is necessary as ISourceGenerator is being deprecated in .NET 9, and IIncrementalGenerator provides better build performance by only running the generator on changed files.

## Requirements

- Replace ISourceGenerator implementation with IIncrementalGenerator to ensure future .NET compatibility
- Implement Initialize method with IncrementalGeneratorInitializationContext parameter
- Properly set up source inputs and transformations using the incremental pipeline
- Ensure all existing functionality is maintained
- Update any relevant tests

## Checklist

### Design
- [ ] Review current generator implementation
- [ ] Design incremental pipeline stages
- [ ] Plan transformation steps

### Implementation
- [ ] Update to IIncrementalGenerator interface
- [ ] Implement Initialize method
- [ ] Set up source inputs registration
- [ ] Configure incremental pipeline
- [ ] Update existing tests
- [ ] Add new tests for incremental behavior

### Documentation
- [ ] Update inline documentation
- [ ] Document migration rationale and benefits

### Review
- [ ] Verify generator output matches previous implementation
- [ ] Consider Performance Implications
- [ ] Code Review

## Notes

The migration to IIncrementalGenerator is required as ISourceGenerator is being deprecated in .NET 9. This update will not only make the PersistenceStateSourceGenerator more efficient through incremental generation but also ensure compatibility with future .NET versions.

## Implementation Notes

- Source file: Source/TimeWarp.State.SourceGenerator/PersistenceStateSourceGenerator.cs
