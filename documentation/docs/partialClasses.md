---
uid: PartialClasses:PartialClasses.md
title: The use of Partial Classes
---

## How we use Partial classes

We recommend partial classes when declaring States for two reasons.

### Child Classes

We declare all `Request Handlers` as child classes of the state they act upon.
Child classes have access to private members of the parent class. This allows us to make the parent class public data immutable to and allow only the `Handlers` to modify the private data.

This ensures that all attempts to mutate state are passed through the MediatR pipeline.  And yet lets us separate the handlers into a file of their own for better organization.

### Aspects 

Each specific State has explicit aspects.

* Data `<StateName>State.cs`
  > This is the data we consider the state.
* State management functionality `<StateName>State.Behavior.cs`
  > These are required by blazor-state to manage the state
  * Clone
  * Initialize  
  
* Debug/DevTools/Test `<StateName>State.Debug.cs`
  > This portion of the partial class is wrapped in a conditional compilation.  
  As this is only needed when built in Debug mode.
  As the devtools and testing will not be needed in production build
  * Parameterless constructor
  * Hydrate
    > Required by ReduxDevTools to deserialize
  * InitializeTestState
    > Used in test cases to initialize state