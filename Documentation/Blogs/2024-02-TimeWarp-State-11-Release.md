# TimeWarp.State 11.0.0: A New Era in Blazor State Management

We're thrilled to announce the release of TimeWarp.State 11.0.0, marking a significant milestone in our journey. Previously known as Blazor-State, this release represents not just a rebranding but a major evolution in state management for Blazor applications.

## What's New in TimeWarp.State?

### 🚀 Major Enhancements

#### TimeWarp.State.Plus Package
We've introduced a new package packed with advanced features:
- **Timer System**: Robust timer management with MultiTimer support and activity-based reset capabilities
- **Enhanced Routing**: New TwBreadcrumb and TwPageTitle components
- **Improved Caching**: TimeWarpCacheableState for better state persistence

#### TimeWarp.State.Policies Package
Enforce architectural patterns with our new policies package:
- BeNestedInStateCustomRule for proper nesting
- HaveInjectableConstructor rule
- HaveJsonConstructor rule
- Comprehensive Action, ActionHandler, ActionSet, and State policies

### 💡 Key Features

- **Flexible State Inheritance**: Chain inherits from State<> to create custom BaseState<> implementations
- **ActionSet Source Generator**: Simplified action creation
- **Persistent State**: New [PersistentState] attribute for state persistence
- **Enhanced Action Tracking**: Improved capabilities with ActionTrackingState
- **Advanced Rendering Control**: Fine-grained control over component re-rendering through TimeWarpStateComponent
- **Better Performance**: New RenderMode and RenderReasons tracking for optimized rendering

### 🛠 Developer Experience

- **Improved Analyzers**: TimeWarp.State.Analyzer provides comprehensive code analysis
- **Enhanced Source Generators**: Improved development productivity
- **Comprehensive Samples**: New examples demonstrating various features:
  - Basic State and Action Handler usage
  - Redux DevTools integration
  - Action Tracking capabilities
  - Advanced routing features

## Breaking Changes

TimeWarp.State 11.0.0 requires .NET 8.0 or later. The rebranding means you'll need to update:
- Package references from `Blazor-State` to `TimeWarp.State`
- Namespace references from `BlazorState` to `TimeWarp.State`

For detailed migration instructions, check our [Migration Guide](https://timewarpengineering.github.io/timewarp-state/Migrations/Migration9-10.html).

## Looking Forward

This release sets a new foundation for state management in Blazor applications. With improved performance, enhanced developer tools, and new features, TimeWarp.State 11.0.0 provides a robust solution for managing application state.

We're committed to continuing this evolution, making state management in Blazor applications more powerful and developer-friendly.

Ready to get started? Check out our [documentation](https://timewarpengineering.github.io/timewarp-state/) for comprehensive guides and examples.
