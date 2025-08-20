# States Actions and Handlers in TimeWarp.State

The StateActionHandler pattern is a fundamental concept in TimeWarp.State that implements unidirectional data flow. This pattern consists of three main components:

## Components

### 1. State
- Represents the current state of your application or a portion of it
- Is immutable from the outside
- Can only be modified through Actions and their Handlers
- Inherits from `State<TState>`

### 2. Action
- Represents an intent to change the state
- Contains any data needed to perform the state change
- Is immutable
- Implements `IAction`

### 3. Handler
- Processes the Action and updates the State
- Is the only component allowed to modify State
- Inherits from `ActionHandler<TAction>`

## Flow

1. Components interact with State through read-only properties
2. When a state change is needed, an Action is dispatched
3. The corresponding Handler processes the Action
4. The Handler updates the State
5. Components are automatically notified of State changes

## Examples

This directory contains examples of the StateActionHandler pattern implemented in different Blazor render modes:

- [Auto](Auto/): Implementation using Blazor's Interactive Auto render mode
- [Server](Server/): Implementation using Blazor's Interactive Server render mode
- [Wasm](Wasm/): Implementation using Blazor's Interactive WebAssembly render mode

Each example demonstrates the same core concepts but highlights considerations specific to that render mode.

## Key Benefits

1. **Predictable State Changes**: All state modifications follow the same pattern
2. **Immutable State**: State can only be modified through handlers
3. **Separation of Concerns**: Clear separation between state, actions, and their handlers
4. **Testability**: Actions and their effects are easily testable
5. **Debugging**: State changes are traceable through actions

## Best Practices

1. Keep States focused and cohesive
2. Make Actions intention-revealing
3. Keep Handlers pure and predictable
4. Use meaningful names that reflect the domain
5. Follow the immutability principle
