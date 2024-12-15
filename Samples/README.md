# TimeWarp.State Samples

This directory contains sample projects that demonstrate various features and capabilities of TimeWarp.State. Each sample focuses on specific aspects of the library and includes examples in different Blazor render modes.

## Available Samples

### [00-StateActionHandler](00-StateActionHandler/)
Demonstrates the fundamental StateActionHandler pattern, which implements unidirectional data flow in TimeWarp.State. This sample showcases:
- State management through immutable states
- Action-based state modifications
- Handler implementation for processing actions
- Examples in multiple Blazor render modes (Auto, Server, WebAssembly)

### [01-ReduxDevTools](01-ReduxDevTools/)
Shows integration with Redux DevTools for enhanced debugging capabilities:
- Real-time state monitoring
- Action tracking and time-travel debugging
- State inspection and modification
- WebAssembly implementation example

### [02-ActionTracking](02-ActionTracking/)
Illustrates action tracking functionality:
- Monitoring action execution
- Performance tracking
- Action state management
- WebAssembly implementation example

### [03-Routing](03-Routing/)
Demonstrates TimeWarp.State.Plus's comprehensive routing features:
- Stack-based navigation management
- Breadcrumb navigation support
- Route history tracking
- Thread-safe route updates
- Page title synchronization
- WebAssembly implementation example

## Getting Started

Each sample project contains its own README with detailed information about:
- Core concepts demonstrated
- Implementation details
- Key benefits
- Best practices
- Specific considerations for different Blazor render modes

Choose a sample based on the feature you want to explore and refer to its README for detailed guidance.

## Sample Structure

Each sample may include implementations in different Blazor render modes:
- **Auto**: Interactive Auto render mode
- **Server**: Interactive Server render mode
- **Wasm**: Interactive WebAssembly render mode

This helps demonstrate how TimeWarp.State can be used effectively across different Blazor hosting models.

## Best Practices

When exploring these samples, consider:
1. Reading the sample's README first to understand its purpose and concepts
2. Examining the implementation in your preferred render mode
3. Following the demonstrated patterns in your own applications
4. Using the samples as reference for implementing similar features
5. Understanding the specific considerations for each render mode
