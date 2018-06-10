---
uid: BlazorStateSample:README.md
title: Blazor-State Sample Application Part 01
---

# Blazor-State Sample Application Part 01
This sample shows how to add Blazor-State to the standard Visual Studio `Blazor Hosted` template.

> [!NOTE]
>[Run completed sample online]

## Creating the project
1. Create a new Blazor website using the `Hosted Blazor` template in Visual Studio 
   (See the [Blazor Getting Started Guide] for details on how to use Blazor).
2. Name the project `BlazorStateSample`.
3. Add the Blazor-State NuGet package to `BlazorStateSample.Client` project. 
  Note that you might have to check the `Include prerelease` checkbox.
 
## Configure the services
1. In the `BlazorStateSample.Client` project in the `Program.cs` file. 
2. Add `using BlazorState;`
3. Change the serviceProvider initialization code to add Blazor-State

[!code-csharp[serviceProvider initialization](../BlazorStateSample/BlazorStateSample.Client/Program.cs?name=serviceProvider initialization)] 

## File Strucutre of: Feature, State, Request, Handler 
With the mediator pattern for each `Request` there is an associated `Handler` 
and possibly other items like a Validator, Mapper etc.. 
The Request, Handler and associated items are what we call a `Feature`.  
I organize the Features by the State they act upon.

1. In the Client project add a folder named `Features`.

## Adding state
1. In the `Features` folder add a folder named `Counter`.
1. Within the `Counter` folder create a class file named `CounterState.cs`.
2. Add `using BlazorState;`

Your class should:
* descend from `State<CounterState>`:
* override the `Clone()` method. 
* override the `Initialize()` method. To set the initial values of the state.

> [!NOTE]
>  _Blazor-State will `Clone` the `State` prior to the sending of any `Request`.
>  Thus the `Handler` of the request can modify state as desired._

4. Enter the code as follows:

[!code-csharp[CounterState](../BlazorStateSample/BlazorStateSample.Client/Features/Counter/CounterState.cs?name=CounterState)]

## Seperation of HTML and C#.
I like to keep my C# code separated from the HTML and yet co-located.
Instead of a `@functions` sections for each .cshtml file we 
will have an associated .cshtml.cs. file and Visual Studio will automatically group these together.
This is not required but I find it easier to reason about.

> [!NOTE]
> _Currently seperation requires the page inherting from the associated `Model`.  
> But I belive this will become a normal feature of Blazor, similar to "Code behind."_


## Create the `CounterModel` for the `Counter` Page.
1. Create a new file named `Counter.cshtml.cs` in the same `Pages` folder as `Counter.cshtml`
CounterModel should:
 * inherit from BlazorComponent
 * implement IBlazorStateComponent

 optionally:
* add property to access State from the store.
as follows:
[!code-csharp[CounterState](../BlazorStateSample/BlazorStateSample.Client/Pages/Counter.cshtml.cs?range=11-15)]

## Displaying state in the user interface.
1. Edit `Pages\Counter.cshtml` and add the following `inherits` clause.
```
@inherits CounterModel
```
2. Change the HTML that displays the value of the counter to `@CounterState.Count`.
3. Remove the @functions sections.

[!code-csharp[CounterState](../BlazorStateSample/BlazorStateSample.Client/Pages/Counter.cshtml)]

## Sending requests that will mutate the state
Changes to state are done by sending a `Request` through the mediator pipeline.
The `Request` is then handled by a `Handler` which can freely mutate the state.

>[!Warning]
>State should NOT be mutated by the presentation layer (.cshtml) or the associated `Model`.

## Create the `IncrementCounterRequest`
1. In the Client project's `Featuers\Counter` folder add a new folder named `IncrementCount`.
2. In this folder create a class named `IncrementCountRequest.cs`.
* The class should inherit from IRequest
as follows:

[!code-csharp[CounterState](../BlazorStateSample/BlazorStateSample.Client/Features/Counter/IncrementCount/IncrementCountRequest.cs)]

## Sending the request to the mediator pipeline

To Send the request to the Pipeline when the user clicks the `Click me` button. 

Add the following method to `Pages\Counter.cshtml.cs`.

[!code-csharp[CounterState](../BlazorStateSample/BlazorStateSample.Client/Pages/Counter.cshtml.cs?name=IncrementCount)]

## Handling the request 

The Handler is where we actually mutate the state to fulfill the `Request`.  

1. In the `Features\Counter\IncrementCount` folder create a new class file named
 `IncrementCountHandler.cs` 

The Handler should:
  * Inherit from `BlazorState.Handlers.RequestHandler`.
  * The generic parameters are the Request Type `IncrementCountRequest` and the state type `CounterState`.
  * Override the `Handle` method
as follows:

[!code-csharp[CounterState](../BlazorStateSample/BlazorStateSample.Client/Features/Counter/IncrementCount/IncrementCountHandler.cs)]

## Validate

Execute the app and confirm that the "Click me" button properly increments the value.

Next we will convert the FetchData page to use Blazor-State to demonstrate making http requests.

[Run completed sample online]: <https://blazor-state.azurewebsites.net/>
[Getting Started Guide]: (https://blazor.net/docs/get-started.html)