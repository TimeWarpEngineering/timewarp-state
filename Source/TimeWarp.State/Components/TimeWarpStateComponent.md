# TimeWarpStateComponent

## Overview

The `TimeWarpStateComponent` is a crucial base class in the TimeWarp.State library, designed to enhance Blazor components with advanced state management and rendering optimization features. It implements `ITimeWarpStateComponent` and serves as a foundation for building performant, state-aware Blazor components.

## Key Features

### 1. Dependency Injection

- Injects essential services:
  - `IStore`: For state management
  - `ILogger`: For logging
  - `IMediator`: For mediator pattern implementation
  - `Subscriptions`: For managing state subscriptions

### 2. Instance Tracking

- Generates a unique `Id` for each component instance
- The primary purpose of the ID is for placing of subscriptions
- Also useful for debugging and component identification

### 3. Render Mode Management

- Supports multiple render modes: Static, PreRendering, Server, and WebAssembly (Wasm)
- Caches the reflection information used to determine the `ConfiguredRenderMode`
- Provides `ConfiguredRenderMode` property for render mode-specific logic

### 4. State Management

- `GetState<T>`: Retrieves the current state and optionally subscribes to state changes
- `GetPreviousState<T>`: Retrieves the previous state for comparison

### 5. Render Optimization

- Custom `ShouldRender` implementation for optimized rendering
- `RegisterRenderTrigger`: Allows registration of state-specific render conditions
- `ShouldReRender`: Determines if a re-render is necessary based on state changes

### 6. Lifecycle Management

- Overrides `OnAfterRender` for render mode-specific logic
- Implements `IDisposable` for proper cleanup of subscriptions

### 7. Testing Support

- `TestId` parameter for assigning identifiers useful in automated testing

## Usage

1. **Inherit from TimeWarpStateComponent**
   ```csharp
   public class MyComponent : TimeWarpStateComponent
   ```

2. **Register Render Triggers**
   ```csharp
   protected override void OnInitialized()
   {
       base.OnInitialized();
       RegisterRenderTrigger<UserState>(previousState => 
           UserState.Name != previousState.Name);
   }
   ```

3. **Use State in Render**
   To efficiently access and display state in your components, create a property that retrieves the state. This approach allows you to easily use the state in your Razor markup. For example:
   ```csharp
   protected UserState UserState => GetState<UserState>();
   ```
   Then in your Razor:
   ```razor
   <p>@UserState.Name</p>
   ```

4. **Optimize Renders**
   The component will automatically optimize renders based on registered triggers.

## Best Practices

1. Register render triggers for all relevant states in `OnInitialized`
2. Use `GetState<T>()` to access current state values
3. Leverage `ConfiguredRenderMode` for render mode-specific optimizations
4. Utilize the `TestId` parameter for automated testing scenarios

## Performance Considerations

- The component uses caching mechanisms to optimize repeated operations
- Reflection is used on if `ConfiguredRenderMode` is accessed and results are cached for performance
- State comparisons are only performed when necessary, based on registered triggers

## Extensibility

- Override `ShouldReRender` for custom re-render logic
- Extend the class to add additional state management or rendering features

By leveraging `TimeWarpStateComponent`, developers can create highly optimized, state-aware Blazor components with minimal boilerplate code.
