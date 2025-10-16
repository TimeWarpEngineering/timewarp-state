# Task 037: Factor Out Blazor-Specific Code for Console App Support

## Description

- Refactor TimeWarp.State to separate core state management functionality from Blazor-specific features
- Enable TimeWarp.State to be used in console applications and other non-Blazor contexts
- Create a new TimeWarp.State.Blazor package for all Blazor-specific functionality

## Requirements

- TimeWarp.State package should have no Blazor dependencies
- All existing functionality must be preserved through package composition
- Maintain backward compatibility for existing users
- Console applications should be able to use core state management features

## Checklist

### Design
- [ ] Identify all Blazor-specific code in TimeWarp.State
- [ ] Design package structure and dependencies
- [ ] Plan migration path for existing users
- [ ] Create architecture tests to ensure no Blazor dependencies in core

### Implementation
- [ ] Create TimeWarp.State.Blazor project structure
- [ ] Move Blazor components to TimeWarp.State.Blazor
  - [ ] TimeWarpStateComponent and related components
  - [ ] TimeWarpStateDevComponent (Redux DevTools)
  - [ ] RenderModeDisplay component
  - [ ] ReduxDevTools.razor
  - [ ] TimeWarpJavaScriptInterop.razor
- [ ] Move JavaScript interop functionality
  - [ ] JsonRequestHandler
  - [ ] ReduxDevToolsInterop
  - [ ] All wwwroot assets (JS/TS files)
- [ ] Move render subscription features
  - [ ] RenderSubscriptionContext
  - [ ] RenderSubscriptionsPostProcessor
- [ ] Remove Microsoft.AspNetCore.Components.Web dependency from TimeWarp.State
- [ ] Update service registration extensions
  - [ ] Create separate registration for Blazor features
  - [ ] Maintain core registration in TimeWarp.State
- [ ] Consider renaming TimeWarp.State.Plus to TimeWarp.State.Blazor.Plus
- [ ] Create console app sample to validate functionality
- [ ] Update all existing samples to reference appropriate packages

### Documentation
- [ ] Update package descriptions and README files
- [ ] Document migration guide for existing users
- [ ] Create usage examples for console applications
- [ ] Update ai-context.md with new package structure

### Review
- [ ] Verify no breaking changes for existing users
- [ ] Ensure clean separation of concerns
- [ ] Test in both Blazor and console contexts
- [ ] Performance impact assessment
- [ ] Review package size reduction for console apps

## Notes

- TimeWarp.State will contain: Store, State, Actions, Handlers, Pipeline behaviors (minus Redux DevTools), Core attributes
- TimeWarp.State.Blazor will contain: All components, JS interop, Redux DevTools, Render subscriptions
- TimeWarp.State.Plus has Blazor dependencies (routing components, Blazored storage) - consider if it should become TimeWarp.State.Blazor.Plus
- This separation will enable broader adoption of TimeWarp.State in different application types

## Implementation Notes

- Start by creating the new project structure
- Move files incrementally, testing at each step
- Ensure analyzers and source generators work with new structure