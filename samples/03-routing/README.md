# Routing in TimeWarp.State

This sample demonstrates TimeWarp.State.Plus's comprehensive routing feature that provides stack-based navigation management. The routing system enables breadcrumb navigation, direct route changes, and back navigation capabilities through state management.

## Key Features

1. **Stack-Based Navigation**
   - Maintains a history of visited routes
   - Enables breadcrumb-style navigation
   - Supports multi-step back navigation

2. **Core Actions**
   - `ChangeRoute`: Direct navigation with absolute URI handling
   - `GoBack`: Stack-based back navigation with configurable steps
   - `PushRouteInfo`: Route stack management with title synchronization

3. **Thread Safety**
   - Semaphore-based synchronization for route updates
   - Safe concurrent navigation handling

4. **Integration**
   - Works seamlessly with Blazor's NavigationManager
   - Automatic page title synchronization
   - Built-in logging support

## Examples

This directory contains examples of the routing feature implemented in different Blazor render modes:

- [Wasm](Wasm/): Implementation using Blazor's Interactive WebAssembly render mode

## Key Benefits

1. **Breadcrumb Navigation**: Built-in support for implementing breadcrumb trails
2. **History Management**: Stack-based route history for complex navigation flows
3. **Thread Safety**: Protected route updates in concurrent scenarios
4. **Title Synchronization**: Automatic page title management
5. **Logging Integration**: Built-in route change logging

## Best Practices

1. Use `PushRouteInfo` to maintain accurate navigation history
2. Implement breadcrumbs using `RouteState.Routes`
3. Handle navigation events through state actions
4. Consider thread safety in concurrent scenarios
5. Utilize logging for debugging navigation flows
