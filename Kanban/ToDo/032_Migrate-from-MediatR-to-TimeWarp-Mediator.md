# Task 032: Migrate from MediatR to TimeWarp.Mediator

## Description

- Migrate TimeWarp.State from MediatR to TimeWarp.Mediator (a fork of MediatR)
- Since TimeWarp.Mediator maintains API compatibility with MediatR, this should be primarily package references and namespace updates
- Create migration documentation for both TimeWarp.State users and general MediatR users

## Requirements

- Replace MediatR package references with TimeWarp.Mediator in all project files
- Update all namespace imports from MediatR to TimeWarp.Mediator
- Update service registration to use TimeWarp.Mediator methods
- All existing tests must continue to pass
- Create comprehensive migration documentation
- Document any API differences discovered during migration

## Checklist

### Design
- [x] Research current MediatR usage across codebase (40+ files identified)
- [ ] Run all tests to ensure baseline functionality before migration
  - [ ] Run ./RunTests.ps1 to verify unit tests pass
  - [ ] Run ./RunE2ETests.ps1 to verify end-to-end tests pass
  - [ ] Run ./RunTestApp.ps1 to verify test application works
  - [ ] Verify architecture tests in TimeWarp.State.Policies still pass
- [ ] Identify TimeWarp.Mediator package version and compatibility
- [ ] Plan migration strategy for minimal disruption

### Implementation
- [ ] Update Directory.Packages.props with TimeWarp.Mediator version
- [ ] Update package references in 2 .csproj files:
  - [ ] Source/TimeWarp.State/TimeWarp.State.csproj
  - [ ] Tests/Test.App/Test.App.Contracts/Test.App.Contracts.csproj
- [ ] Update global using statements in 8 GlobalUsings.cs files:
  - [ ] Source/TimeWarp.State/GlobalUsings.cs
  - [ ] Source/TimeWarp.State.Plus/GlobalUsings.cs
  - [ ] Tests/TimeWarp.State.Tests/GlobalUsings.cs
  - [ ] Tests/Test.App/Test.App.Contracts/GlobalUsings.cs
  - [ ] Tests/Test.App/Test.App.Client/GlobalUsings.cs
  - [ ] Tests/Client.Integration.Tests/GlobalUsings.cs
  - [ ] Samples/02-ActionTracking/Wasm/Sample02Wasm/GlobalUsings.cs
- [ ] Update service registration in ServiceCollectionExtensions.AddTimeWarpState.cs
- [ ] Verify all MediatR interfaces still work (IRequest, IRequestHandler, IMediator, ISender, IPublisher)
- [ ] Test pipeline behaviors (5 implementations)
- [ ] Test pre/post processors (6 implementations)
- [ ] Test notification handlers (9 implementations)
- [ ] Verify Redux DevTools integration continues to work
- [ ] Verify all builds pass after package updates
- [ ] Test NuGet package creation with ./BuildNuGets.ps1

### Documentation
- [ ] Create migration guide for TimeWarp.State users
- [ ] Create general MediatR → TimeWarp.Mediator migration document
- [ ] Update ai-context.md with new dependency information
- [ ] Update Claude.md to reflect new TimeWarp.Mediator dependency
- [ ] Document any breaking changes or API differences

### Review
- [ ] Consider Accessibility Implications
- [ ] Consider Monitoring and Alerting Implications
- [ ] Consider Performance Implications (should be minimal/none)
- [ ] Consider Security Implications (should be minimal/none)
- [ ] Code Review

## Notes

- TimeWarp.Mediator is a fork of MediatR, so API compatibility should be maintained
- Current MediatR usage includes:
  - Core interfaces: IRequest, IRequestHandler, IMediator, ISender, IPublisher
  - Pipeline behaviors: 5 implementations for Redux DevTools, state transactions, action tracking
  - Pre/post processors: 6 implementations for state initialization, render subscriptions, persistence
  - Notifications: 9 handlers for various state management events
- Migration should be primarily mechanical (package + namespace changes)
- Need to create documentation for both internal migration and general MediatR users

## Implementation Notes

- Start with package references, then global usings, then service registration
- Test thoroughly at each step to ensure no breaking changes
- Document any unexpected API differences for future reference