# Task 050: Migrate Mediator - Update Documentation

## Description

- Update all documentation to reflect the migration from TimeWarp.Mediator to martinothamar/Mediator
- Create migration guide for consumers
- Update references in README, claude.md, and other docs

## Requirements

- Update all documentation referencing TimeWarp.Mediator
- Create migration guide for library consumers
- Document breaking changes

## Checklist

### Documentation
- [ ] Update `readme.md`:
  - [ ] Change references from TimeWarp.Mediator to Mediator
  - [ ] Update any code examples
- [ ] Update `claude.md`:
  - [ ] Update dependency references
  - [ ] Update any mediator-related guidance
- [ ] Update `documentation/overview.md`:
  - [ ] Change TimeWarp.Mediator pipeline references
- [ ] Update `documentation/partials/terminology.md`:
  - [ ] Update mediator pattern references
- [ ] Create migration guide in `documentation/migrations/`:
  - [ ] Document all breaking changes
  - [ ] Provide before/after code examples
  - [ ] List required consumer code changes
- [ ] Update source file comments:
  - [ ] `source/timewarp-state/features/pipeline/state-transaction-behavior.cs`
  - [ ] `source/timewarp-state/extensions/timewarp-state-options.cs`
  - [ ] `source/timewarp-state/extensions/service-collection-extensions.log-timewarp-state-middleware.cs`
  - [ ] Any other files with TimeWarp.Mediator references

## Notes

**Breaking changes to document:**

1. **Pipeline behavior signature change:**
   - Parameter order changed
   - Delegate invocation changed from `next()` to `next(message, cancellationToken)`

2. **Pre/Post processor changes:**
   - Interface -> Abstract class
   - Method rename: `Process()` -> `Handle()`
   - Return type: `Task` -> `ValueTask`

3. **Handler return types:**
   - `Task` -> `ValueTask<Unit>`
   - `Task<T>` -> `ValueTask<T>`

4. **Notification handlers:**
   - `Task` -> `ValueTask`

5. **Service registration:**
   - New configuration API

**Files to update:**
1. `readme.md`
2. `claude.md`
3. `documentation/overview.md`
4. `documentation/partials/terminology.md`
5. Create `documentation/migrations/migration12-13.md`

## Implementation Notes

