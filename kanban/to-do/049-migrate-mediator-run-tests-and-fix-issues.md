# Task 049: Migrate Mediator - Run Tests and Fix Issues

## Description

- Run all tests after migration tasks are complete
- Fix any remaining compilation errors or test failures
- Verify the application works correctly

## Requirements

- All unit tests pass
- All integration tests pass
- All E2E tests pass
- Test application runs successfully

## Checklist

### Implementation
- [ ] Run `dotnet build` and fix any compilation errors
- [ ] Run unit tests: `dotnet test tests/timewarp-state-tests/`
- [ ] Run analyzer tests: `dotnet test tests/timewarp-state-analyzer-tests/`
- [ ] Run plus tests: `dotnet test tests/timewarp-state-plus-tests/`
- [ ] Run client integration tests: `dotnet test tests/client-integration-tests/`
- [ ] Run architecture tests: `dotnet test tests/test-app-architecture-tests/`
- [ ] Run E2E tests: `dotnet test tests/test-app-end-to-end-tests/`
- [ ] Start test application and verify basic functionality

### Review
- [ ] Review any test failures and create follow-up tasks if needed
- [ ] Document any behavioral changes observed

## Notes

**Common issues to watch for:**

1. **Missing Unit.Value returns** - Handler methods that don't return `Unit.Value`
2. **ValueTask handling** - Improper awaiting or caching of ValueTask
3. **Pipeline order** - Behaviors executing in wrong order
4. **Async state machine differences** - ValueTask vs Task behavior

**Commands:**
```bash
# Build everything
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/timewarp-state-tests/
```

## Implementation Notes

