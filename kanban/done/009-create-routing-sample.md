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
- [x] Review existing implementation:
  - [x] RouteState stack management
  - [x] ChangeRoute action handling
  - [x] GoBack functionality
  - [x] PushRouteInfo synchronization
  - [x] Thread safety mechanisms
- [x] Design navigation flow demonstrating all actions
- [x] Design breadcrumb component layout and behavior
- [x] Plan demonstration scenarios

### Implementation
- [x] Create sample project structure
- [x] Implement RouteState integration
- [x] Create multiple pages with distinct titles
- [x] Implement breadcrumb component:
  - [x] Display route stack as breadcrumb trail
  - [x] Show page titles
  - [x] Handle breadcrumb navigation
  - [x] Style breadcrumb UI
- [x] Add additional navigation components:
  - [x] Direct route change controls
  - [x] Back navigation with step selection
  - [x] Route stack visualization
- [x] Implement all action handlers:
  - [x] ChangeRoute with proper URI handling
  - [x] GoBack with stack management
  - [x] PushRouteInfo with title sync
- [x] Add logging integration
- [x] Implement thread safety
- [x] Add TimeWarpPageRenderNotifier example
- [x] Add manual PushRouteInfo example

### Documentation
- [x] Add README explaining:
  - [x] All three routing actions
  - [x] Stack-based navigation concept
  - [x] Breadcrumb implementation using RouteState
  - [x] Route state management
  - [x] Thread safety considerations
  - [x] Logging integration
- [x] Document navigation patterns
- [x] Add inline documentation
- [x] Include usage examples

### Review
- [x] Test all routing actions:
  - [x] Direct route changes
  - [x] Back navigation
  - [x] Route stack updates
- [x] Test breadcrumb functionality:
  - [x] Correct display of navigation history
  - [x] Proper handling of breadcrumb navigation
  - [x] Title synchronization
- [x] Verify logging output
- [x] Test thread safety
- [x] Validate page title synchronization
- [x] Ensure proper state management
- [x] Code Review

## Notes

- Reference implementation in Source/TimeWarp.State.Plus/Features/Routing/RouteState
- Primary use case is enabling breadcrumb navigation through RouteState.Routes
- Ensure proper handling of absolute vs relative URIs
- Demonstrate logging of route changes
- Show proper semaphore usage for route updates
- Include examples of both TimeWarpPageRenderNotifier and manual implementations
- Follow patterns from Test.App.Client's GoBackPage implementation

## Implementation Notes

### Components Overview

1. TwPageTitle Component
   - Integrates with TimeWarp's route state management
   - Wraps Blazor's built-in PageTitle component
   - Performance optimized:
     - Inherits TimeWarpStateComponent for efficient parameter tracking
     - Uses built-in parameter change detection
     - Avoids state subscriptions to prevent render loops
     - Updates route state only on first render
     - Leverages reference comparison for RenderFragment changes
   - Simple usage: `<TwPageTitle>Your Page Title</TwPageTitle>`

2. TwBreadcrumb Component
   - Provides stack-based navigation history visualization
   - Features:
     - Configurable maximum number of displayed links through MaxLinks parameter
     - Shows ellipsis when routes exceed MaxLinks
     - Displays newest-to-oldest navigation history
     - Automatic active state handling for current route
     - Direct navigation through breadcrumb clicks using GoBack action
   - Implementation details:
     - Inherits TimeWarpStateComponent
     - Uses RouteState.Routes for navigation history
     - Calculates correct "steps back" for each breadcrumb link
     - Prevents default link behavior and uses state-managed navigation
     - Semantic HTML with proper accessibility attributes and CSS classes for customization

### Integration Points

1. Route State Management
   - Components work together to maintain consistent navigation state:
     - TwPageTitle updates the route info in the state
     - TwBreadcrumb reads from the route state to display history
     - Both components leverage TimeWarpStateComponent for optimization

2. Navigation Flow
   - TwPageTitle: Updates page title and maintains route state
   - TwBreadcrumb: Provides UI for navigating through route history
   - Both components work with RouteState actions:
     - ChangeRoute: Used for direct navigation
     - GoBack: Used by breadcrumb navigation
     - PushRouteInfo: Managed by TwPageTitle for route stack updates

3. Performance Considerations
   - Both components are optimized for minimal re-renders
   - State updates are carefully managed to prevent infinite loops
   - Parameter change detection is leveraged for efficiency
   - Reference comparison is used where applicable

### Usage Guidelines

1. Page Title Management
   - Use TwPageTitle on each page to maintain accurate navigation history
   - Title updates automatically sync with route state
   - No manual PushRouteInfo calls needed when using TwPageTitle

2. Breadcrumb Navigation
   - Place TwBreadcrumb component where navigation history should be displayed
   - Configure MaxLinks based on UI requirements
   - Breadcrumb links automatically handle navigation through GoBack action

3. State Integration
   - Components handle state management internally
   - No additional setup required beyond component placement
   - Thread-safe route updates are managed by the underlying RouteState
