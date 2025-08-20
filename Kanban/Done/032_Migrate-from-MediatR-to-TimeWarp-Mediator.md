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
- [x] Run all tests to ensure baseline functionality before migration
  - [x] Run ./RunTests.ps1 to verify unit tests pass
  - [x] Run ./RunE2ETests.ps1 to verify end-to-end tests pass
  - [x] Run ./RunTestApp.ps1 to verify test application works (has pre-existing startup issue)
  - [x] Verify architecture tests in TimeWarp.State.Policies still pass
- [x] Identify TimeWarp.Mediator package version and compatibility (v13.0.0)
- [x] Plan migration strategy for minimal disruption

### Implementation
- [x] Update Directory.Packages.props with TimeWarp.Mediator version
- [x] Update package references in 2 .csproj files:
  - [x] Source/TimeWarp.State/TimeWarp.State.csproj
  - [x] Tests/Test.App/Test.App.Contracts/Test.App.Contracts.csproj
- [x] Update global using statements in 8 GlobalUsings.cs files:
  - [x] Source/TimeWarp.State/GlobalUsings.cs
  - [x] Source/TimeWarp.State.Plus/GlobalUsings.cs
  - [x] Tests/TimeWarp.State.Tests/GlobalUsings.cs
  - [x] Tests/Test.App/Test.App.Contracts/GlobalUsings.cs
  - [x] Tests/Test.App/Test.App.Client/GlobalUsings.cs
  - [x] Tests/Client.Integration.Tests/GlobalUsings.cs
  - [x] Samples/02-ActionTracking/Wasm/Sample02Wasm/GlobalUsings.cs
- [x] Update service registration in ServiceCollectionExtensions.AddTimeWarpState.cs
- [x] Verify all MediatR interfaces still work (IRequest, IRequestHandler, IMediator, ISender, IPublisher)
- [x] Test pipeline behaviors (5 implementations)
- [x] Test pre/post processors (6 implementations)
- [x] Test notification handlers (9 implementations)
- [x] Verify Redux DevTools integration continues to work
- [x] Verify all builds pass after package updates
- [x] Test NuGet package creation with ./BuildNuGets.ps1

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

## Progress Updates

### Completed (2025-08-20)
- Successfully migrated all package references from MediatR 12.4.1 to TimeWarp.Mediator 13.0.0
- Updated all namespace imports across 40+ files
- Changed service registration from AddMediatR to AddMediator
- Fixed analyzer tests to use TimeWarp.Mediator.Contracts.dll
- Updated all XML comments and documentation references
- Verified all tests pass (unit, integration, e2e)
- Bumped version to 12.0.0-beta.1
- Created PR #554 for the migration
- Note: Test.App.Server has a pre-existing startup issue unrelated to migration