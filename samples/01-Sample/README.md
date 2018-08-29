---
uid: Sample:README.md
title: Sample Application Part 01
---

# Blazor-State Sample Application

This sample shows how to add Blazor-State to the standard Visual Studio `Blazor (ASP.NET Core hosted)` template.

## Creating the default project
* Create a new Blazor website using the `Blazor (ASP.NET Core hosted)` template in Visual Studio 
* Name the project `Sample`.
* Select `Sample.Server` as your `Startup Project`
* Select `Sample.Server` as your launch profile.
* Execute and confirm the site is functional.

## Add Blazor-State
* From Nuget Package Manager add the Blazor-State NuGet package to `Sample.Client` project. 
  
  > [!NOTE]
  >You might have to check the `Include prerelease` checkbox.
 
## Configure the services
* In the `Sample.Client` project in the `Starup.cs` file. Add `using BlazorState;`
* In the ConfigureServices method add Blazor-State

[!code-csharp[serviceProvider initialization](./Sample.Client/Startup.cs?name=ConfigureServices)] 

## Create the AppModel
* in same directory as the App.cshtml file create App.cshtml.cs file.
* Name the class AppModel.
* Inherit from BlazorComponent.
* Inject JsonRequestHandler, ReduxDevToolsInterop and RouteManager
* Initialize ReduxDevToolsInterop
* In `App.cshtml` add `@inherits AppModel`

[!code-csharp[serviceProvider initialization](./Sample.Client/App.cshtml.cs)] 

* In the Client project add a folder named `Features`.
  
> [!NOTE]
> See [File Structure](xref:FileStructure:FileStructure.md) for details.

## Adding State data
* In the `Features` folder add a folder named `Counter`.
* Within the `Counter` folder create a class file named `CounterState.cs`.
* Make the class a partial class. [](xref:PartialClasses:PartialClasses.md)
* Add the public `Count` property with a private setter.

[!code-csharp[CounterState](./Sample.Client/Features/Counter/CounterState.cs)]

## Implement State Behavior
* Create `CounterState.Behavior.cs`
* Add `using BlazorState;`
* Make the class a [partial class](xref:PartialClasses:PartialClasses.md)
* Descend from `State<CounterState>`:
* Create a parameterless constructor.
* Create a protected constructor
* Override the `Clone()` method.
* Override the `Initialize()` method.

[!code-csharp[CounterState](./Sample.Client/Features/Counter/CounterState.Behavior.cs)]

> [!NOTE]
>  _Blazor-State will `Clone` the `State` prior to the sending of any `Request`.
>  Thus the `Handler` of the request can modify state as desired._

## Create the `CounterModel` for the `Counter` Page.
  > [!NOTE]
  > see [](xref:CodeBehind:CodeBehind.md) for details.
 * Create a new file named `Counter.cshtml.cs` in the same `Pages` folder as `Counter.cshtml`
 * Name the class `CounterModel`
 * inherit from BlazorComponent
 * implement IBlazorStateComponent

 optionally:
* add property to access State from the store.

[!code-csharp[CounterState](./Sample.Client/Pages/Counter.cshtml.cs?range=8-13)]

## Displaying state in the user interface.
* Edit `Pages\Counter.cshtml` and add the following `inherits` clause.
```
@inherits CounterModel
```
* Change the HTML that displays the value of the counter to `@CounterState.Count`.
* Change the onclick to `@ButtonClick` (See implementation below)
* Remove the @functions sections.

[!code-csharp[CounterState](./Sample.Client/Pages/Counter.cshtml)]

## Sending requests that will mutate the state
Changes to state are done by sending a `Request` through the mediator pipeline.
The `Request` is then handled by a `Handler` which can freely mutate the state.

### Create the `IncrementCounterRequest`
* In the Client project's `Featuers\Counter` folder add a new folder named `IncrementCount`.
* In this folder create a class named `IncrementCounterRequest.cs`.
* The class should inherit from IRequest
as follows:

[!code-csharp[CounterState](./Sample.Client/Features/Counter/IncrementCounter/IncrementCounterRequest.cs)]

### Sending the request to the mediator pipeline

To Send the request to the Pipeline when the user clicks the `Click me` button. 

* Add the following method to `Pages\Counter.cshtml.cs`.

[!code-csharp[CounterState](./Sample.Client/Pages/Counter.cshtml.cs?name=ButtonClick)]

### Handling the request 

The Handler is where we actually mutate the state to fulfill the `Request`.  

* In the `Features\Counter\IncrementCounter` folder create a new class file named
`IncrementCounterHandler.cs` 
* set the namespace to `Sample.Client.Features.Counter` 
* Add `using BlazorState;`
* Make the IncrementCounterHandler a child class of CounterState
* Inherit from `RequestHandler`.
* The generic parameters are the Request Type `IncrementCountRequest` and the state type `CounterState`.
* Override the `Handle` method
as follows:

[!code-csharp[CounterState](./Sample.Client/Features/Counter/IncrementCounter/IncrementCounterHandler.cs)]

## Execute the application

Execute the app and confirm that the "Click me" button properly increments the value.

See the other Sample applications source for more complete examples with Integration and End-to-end Testing projects.

[Run completed sample online]: <https://blazor-state.azurewebsites.net/>
[Getting Started Guide]: (https://blazor.net/docs/get-started.html)