# Task 009: Create Sample Application Demonstrating TimeWarp State Plus Routing Feature

## Description

Create a new sample application that demonstrates the TimeWarp.State.Plus Routing Feature's comprehensive navigation management system. This feature provides stack-based navigation history that enables breadcrumb navigation, along with direct route changes and back navigation capabilities through state management.

## Requirements

- Demonstrate all three core actions of the Routing Feature:
  1. ChangeRoute: Direct navigation with absolute URI handling
  2. GoBack: Stack-based back navigation with configurable steps
  3. PushRouteInfo: Route stack management with title synchronization
- Implement a breadcrumb component showcasing the primary use case:
  - Display navigation history using RouteState.Routes
  - Show page titles in breadcrumb trail
  - Enable navigation through breadcrumb clicks
- Show proper integration with NavigationManager
- Demonstrate thread-safe route updates
- Implement logging for route changes
- Show both component-based and manual implementations

## Checklist

### Design
- [ ] Review existing implementation:
  - [ ] RouteState stack management
  - [ ] ChangeRoute action handling
  - [ ] GoBack functionality
  - [ ] PushRouteInfo synchronization
  - [ ] Thread safety mechanisms
- [ ] Design navigation flow demonstrating all actions
- [ ] Design breadcrumb component layout and behavior
- [ ] Plan demonstration scenarios

### Implementation
- [ ] Create sample project structure
- [ ] Implement RouteState integration
- [ ] Create multiple pages with distinct titles
- [ ] Implement breadcrumb component:
  - [ ] Display route stack as breadcrumb trail
  - [ ] Show page titles
  - [ ] Handle breadcrumb navigation
  - [ ] Style breadcrumb UI
- [ ] Add additional navigation components:
  - [ ] Direct route change controls
  - [ ] Back navigation with step selection
  - [ ] Route stack visualization
- [ ] Implement all action handlers:
  - [ ] ChangeRoute with proper URI handling
  - [ ] GoBack with stack management
  - [ ] PushRouteInfo with title sync
- [ ] Add logging integration
- [ ] Implement thread safety
- [ ] Add TimeWarpPageRenderNotifier example
- [ ] Add manual PushRouteInfo example

### Documentation
- [ ] Add README explaining:
  - [ ] All three routing actions
  - [ ] Stack-based navigation concept
  - [ ] Breadcrumb implementation using RouteState
  - [ ] Route state management
  - [ ] Thread safety considerations
  - [ ] Logging integration
- [ ] Document navigation patterns
- [ ] Add inline documentation
- [ ] Include usage examples

### Review
- [ ] Test all routing actions:
  - [ ] Direct route changes
  - [ ] Back navigation
  - [ ] Route stack updates
- [ ] Test breadcrumb functionality:
  - [ ] Correct display of navigation history
  - [ ] Proper handling of breadcrumb navigation
  - [ ] Title synchronization
- [ ] Verify logging output
- [ ] Test thread safety
- [ ] Validate page title synchronization
- [ ] Ensure proper state management
- [ ] Code Review

## Notes

- Reference implementation in Source/TimeWarp.State.Plus/Features/Routing/RouteState
- Primary use case is enabling breadcrumb navigation through RouteState.Routes
- Ensure proper handling of absolute vs relative URIs
- Demonstrate logging of route changes
- Show proper semaphore usage for route updates
- Include examples of both TimeWarpPageRenderNotifier and manual implementations
- Follow patterns from Test.App.Client's GoBackPage implementation

## Implementation Notes

To be added during implementation
