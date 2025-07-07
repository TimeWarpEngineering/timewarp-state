# TwPageTitle Component

## Overview
The TwPageTitle component is a wrapper around Blazor's built-in PageTitle component that integrates with TimeWarp's route state management. It automatically updates the page title and maintains route state while being optimized for performance.

## Render Optimization
The component inherits from TimeWarpStateComponent which provides built-in parameter change detection:

1. **Parameter Change Detection**
   - Utilizes TimeWarpStateComponent's SetParametersAsync for efficient parameter tracking
   - ChildContent (RenderFragment) changes are automatically detected through CheckComplexParameterChanged
   - Uses reference comparison for RenderFragment optimization

2. **State Management**
   - Gets RouteState without subscription to avoid infinite recursion
   - Only updates route info on first render
   - Prevents unnecessary re-renders when title hasn't changed

## Usage
```razor
<TwPageTitle>Your Page Title</TwPageTitle>
```

## Implementation Details
- Inherits TimeWarpStateComponent for optimized rendering
- Uses built-in parameter change detection
- Avoids unnecessary state subscriptions
- Updates route state only when needed (first render)

## Performance Considerations
The component is optimized by:
1. Using TimeWarpStateComponent's built-in parameter change detection
2. Avoiding state subscriptions that could cause render loops
3. Leveraging reference comparison for RenderFragment changes
4. Minimizing route state updates
