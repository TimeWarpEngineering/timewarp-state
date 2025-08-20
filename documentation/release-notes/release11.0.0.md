---
uid: TimeWarpState:Release.11.0.0.md
title: Release 11.0.0
---

## Release 11.0.0

### Major Changes

**Rebranding to TimeWarp.State**
We're excited to announce that Blazor-State has been rebranded to TimeWarp.State. 
This change reflects our evolving vision and commitment to providing a robust state management solution for Blazor applications.

### Breaking Changes

See [Migrations](xref:TimeWarpState:Migration10-11.md) for instructions on how to migrate from version 10.0 to 11.0.

- `TimeWarp.State` now requires .NET 8.0 or later. 
- All references to `BlazorState` should be updated to `TimeWarp.State`.
- The package name has changed from `Blazor-State` to `TimeWarp.State`.
  You'll need to update your package references and namespaces accordingly.

### New Features

- Enabled chained inherits from State<> - so users can make/use their own BaseState<>
- Introduced `TimeWarp.State.Plus` NuGet package with additional features:
  - New Timer system with MultiTimer support and activity-based reset capabilities
  - Feature Flag system for managing feature toggles
  - Enhanced routing with TwBreadcrumb and TwPageTitle components
  - Improved caching with TimeWarpCacheableState
- Added `TimeWarp.State.Policies` NuGet package for architecture policies:
  - BeNestedInStateCustomRule for enforcing proper nesting
  - HaveInjectableConstructor rule
  - HaveJsonConstructor rule
  - Action, ActionHandler, ActionSet, and State policies
- Implemented ActionSet source generator for simplified action creation
- Added support for persistent state with `[PersistentState]` attribute
- Introduced `RenderSubscriptionContext` for improved render subscription management
- Added `ActionTrackingState` for better action tracking capabilities
- Enhanced component rendering with new RenderMode and RenderReasons tracking

### Improvements

- Optimized CI/CD pipelines for better performance and reliability
- Enhanced caching mechanisms for NuGet packages in GitHub Actions
- Improved logging capabilities with new Logger implementation
- Updated to use the latest MediatR version
- Refactored and improved the test suite:
  - Added comprehensive Playwright end-to-end tests
  - New architecture tests for enforcing conventions
  - Enhanced integration tests
- Optimized TwPageTitle component with improved rendering performance
- Added state initialization pre-processor
- Enhanced JavaScript interop capabilities

### Documentation

- Completely revamped documentation structure and content
- Added new tutorials and updated existing ones for TimeWarp.State
- Implemented automated documentation publishing workflow
- Added comprehensive samples demonstrating various features:
  - Sample 00: Basic State and Action Handler usage
  - Sample 01: Redux DevTools integration
  - Sample 02: Action Tracking capabilities
  - Sample 03: Advanced routing features

### Developer Experience

- Added support for central package management
- Improved developer tooling, including updated EditorConfig and coding standards
- Enhanced source generators for better development productivity
- Added TimeWarp.State.Analyzer for code analysis:
  - ActionAnalyzer
  - StateImplementationAnalyzer
  - StateInheritanceAnalyzer
  - StateReadOnlyPublicPropertiesAnalyzer

### Other Changes

- Removed time travel debugging feature to optimize performance and reduce complexity
- Updated all dependencies to their latest stable versions
- Improved error handling and exception reporting throughout the library
- Added .ai folder to the repository for prompting AI-generated content
- Reorganized project structure for better maintainability
- Enhanced build and test scripts

### Migration Guide

For detailed instructions on how to migrate from Blazor-State to TimeWarp.State, please refer to our [Migration Guide](xref:TimeWarpState:Migration10-11.md).
