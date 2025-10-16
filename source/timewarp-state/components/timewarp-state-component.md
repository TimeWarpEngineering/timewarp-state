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
- The `Id` serves as a unique key in the subscription system
- Also useful for debugging and component identification

### 3. Render Mode Management

- Supports multiple render modes: Static, PreRendering, Server, and WebAssembly (Wasm)
- Uses .NET 9's `RendererInfo.Name` to determine the current render mode
- Provides `IsPreRendering` property based on `RendererInfo.IsInteractive`
- Exposes `AssignedRenderMode` for the component's assigned render mode

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

### 8. Render Counting

- Tracks the number of times each component instance is rendered
- Provides a `RenderCount` property to access the current render count
- Useful for performance monitoring and debugging

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

5. **Monitor Render Count**
   You can access the number of times a component has been rendered:
   ```csharp
   protected override void OnAfterRender(bool firstRender)
   {
       base.OnAfterRender(firstRender);
       Console.WriteLine($"Component {Id} has rendered {RenderCount} times");
   }
   ```
   
## Best Practices

1. Register render triggers for all relevant states in `OnInitialized`
2. Use `GetState<T>()` to access current state values
3. Leverage `RendererInfo.Name` and `RendererInfo.IsInteractive` for render mode-specific optimizations
4. Utilize the `TestId` parameter for automated testing scenarios
5. Use the `RenderCount` property to monitor and optimize component rendering performance

## Performance Considerations

- The component uses caching mechanisms to optimize repeated operations
- Uses .NET 9's built-in `RendererInfo` API for efficient render mode detection
- State comparisons are only performed when necessary, based on registered triggers
- The component tracks render counts, which can be used to identify unnecessary re-renders and optimize performance


## Extensibility

- Override `ShouldReRender` for custom re-render logic
- Extend the class to add additional state management or rendering features
- Utilize the `RenderCount` property in derived classes for custom rendering logic or performance tracking

By leveraging `TimeWarpStateComponent`, developers can create highly optimized, state-aware Blazor components with minimal boilerplate code and built-in performance monitoring capabilities.
