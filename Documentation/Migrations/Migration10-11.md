---
uid: TimeWarpState:Migration10-11.md
title: Migrate From 10.X to 11.X
---

# Migration From 10.x to 11.x

This guide will help you migrate your application from Blazor-State 10.x to TimeWarp.State 11.x.

## Required Changes

### 1. Update .NET Version
- Update your project to use .NET 8.0 or later
- Update your project file's `TargetFramework` to `net8.0`

### 2. Package References
- Remove the `Blazor-State` NuGet package
- Add the `TimeWarp.State` NuGet package
- Optionally add new packages based on your needs:
  - `TimeWarp.State.Plus` for additional features
  - `TimeWarp.State.Policies` for architecture enforcement

### 3. Namespace Updates
Replace all occurrences of `BlazorState` with `TimeWarp.State`:
- Update using statements
- Update base class references
- Update interface implementations

Common replacements:
```csharp
// Old
using BlazorState;
using static BlazorState.Features.Routing.RouteState;
class MyComponent : BlazorStateComponent

// New
using TimeWarp.State;
using static TimeWarp.State.Features.Routing.RouteState;
class MyComponent : TimeWarpStateComponent
```

### 4. Component Changes
- Replace `BlazorStateComponent` with `TimeWarpStateComponent`
- Replace `BlazorStateDevToolsComponent` with `TimeWarpStateDevComponent`
- Replace `BlazorStateInputComponent` with `TimeWarpStateInputComponent`

### 5. Configuration Updates
In your `Program.cs`:
```csharp
// Old
services.AddBlazorState();

// New
services.AddTimeWarpState();
```

### 6. Redux DevTools Configuration
If you're using Redux DevTools:
```csharp
// Old
services.AddBlazorState(options => 
    options.UseReduxDevTools()
);

// New
services.AddTimeWarpState(options => 
    options.UseReduxDevTools()
);
```

## Breaking Changes

### State Implementation
- All state classes must now properly implement the State pattern
- Public properties in state classes must be read-only
- Actions must be properly nested within their state classes

### Component Lifecycle
- Changes to render subscription management with new `RenderSubscriptionContext`
- Updated component lifecycle hooks in `TimeWarpStateComponent`

### Time Travel Debugging
- Time travel debugging feature has been removed
- Use Redux DevTools for debugging state changes

## Optional Feature Migrations

### 1. Action Tracking
If you want to track action processing:
```csharp
services.AddTimeWarpState(options => 
    options.UseActionTracking()
);
```

### 2. Persistent State
To use the new persistent state feature:
```csharp
[PersistentState]
public class MyState : State<MyState>
{
    // Your state properties
}
```

### 3. Enhanced Routing
To use the new routing components:
```razor
<TwBreadcrumb />
<TwPageTitle />
```

### 4. Timer System
To use the new timer system from TimeWarp.State.Plus:
```csharp
services.AddTimeWarpState(options => 
    options.UseTimers(timerOptions => {
        timerOptions.AddTimer("MyTimer", TimeSpan.FromMinutes(5));
    })
);
```

### 5. Feature Flags
To use the new feature flag system:
```csharp
services.AddTimeWarpState(options => 
    options.UseFeatureFlags()
);
```

## Architecture Enforcement

Consider adding the TimeWarp.State.Policies package to enforce architectural rules:
```xml
<PackageReference Include="TimeWarp.State.Policies" Version="11.0.0" />
```

This will help ensure:
- Proper state implementation
- Correct action nesting
- Appropriate constructor injection
- JSON serialization support

## Analyzer Updates

The TimeWarp.State.Analyzer has been enhanced with new rules. Address any analyzer warnings to ensure your code follows best practices:
- State implementation rules
- Action nesting rules
- Property accessibility rules

## Testing Updates

If you're using the testing infrastructure:
- Update test base classes to use new TimeWarp.State namespaces
- Consider adding Playwright end-to-end tests using the new testing infrastructure
- Use the new architecture testing capabilities from TimeWarp.State.Policies

## Additional Resources

- Review the [Release Notes](xref:TimeWarpState:Release.11.0.0.md) for a complete list of changes
- Check out the new samples in the repository for implementation examples
- Consult the updated documentation for detailed feature guides
