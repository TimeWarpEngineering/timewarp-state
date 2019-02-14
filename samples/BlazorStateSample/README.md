---
uid: BlazorStateSample:README.md
title: Blazor-State Sample Application
---

# Blazor-State Sample Application
This sample shows how to add Blazor-State to the Visual Studio `Blazor (ASP.NET Core hosted)` template.

## Creating the project

1. Create a new project using the `ASP.NET Core Web Application` solution template in Visual Studio 2019.
2. Name the project `BlazorStateSample`.
3. Create a new `ASP.NETCore web Application` using the `Blazor (ASP.NET Core hosted)` project template.
   (See [Get started with Blazor] for more details).
4. Run the default application and confirm it works.  
   > [!NOTE]
   > Notice that the counter resets on page changes.
   > There is currently no client side state maintained.
   > When the counter components is no longer rendered it is freed by the `GarbageCollector`.
   > When returning to the counter page a new component is created 
   > and therefore the count is back to zero.
5. Add the Blazor-State NuGet package to `BlazorStateSample.Client` project. 
   > [!NOTE]
   > Check the `Include prerelease` checkbox.
 
## Configure the services
1. In the `BlazorStateSample.Client` project in the `Startup.cs` file. 
2. Add `using BlazorState;`
3. Change `ConfigureServices` to add Blazor-State

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBlazorState();
}
```


## Feature File Structure
With the mediator pattern for each `Request/Action` there is an associated `Handler` 
and possibly other items like a `Validator`, `Mapper` etc.. 
These associated items are what we call a `Feature`.
Let's organize the Features by the State they act upon.

1. In the Client project add a folder named `Features`.

## Adding state
1. In the `Features` folder add a folder named `Counter`.
1. Within the `Counter` folder create a class file named `CounterState.cs`.
2. in `CounterState.cs` add `using BlazorState;`

Your class should:
* be a partial class
* descend from `State<CounterState>`
* override the `Clone()` method. 
* override the `Initialize()` method. To set the initial values of the state.

> [!NOTE]
>  _Blazor-State will `Clone` the `State` prior to the sending of any `Request`.
>  Thus the `Handler` of the request can modify state as desired.
>  Without worrying about mutating the original state.

4. Enter the code as follows:

[!code-csharp[CounterState](../BlazorStateSample/BlazorStateSample.Client/Features/Counter/CounterState.cs]

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
  
  * inherit from ComponentBase
  * implement IBlazorStateComponent
  > [!NOTE]
  > Optionally one could inherit form BlazorStateComponent to accomplish the above two items.

optionally:
  * add property to access State from the store.
as follows:
[!code-csharp[CounterState](../BlazorStateSample/BlazorStateSample.Client/Pages/Counter.cshtml.cs?name=DocsCounterModel)]

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

> [!Warning]
> State should NOT be mutated by the presentation layer (.cshtml) or the associated `Model`.
> All state changes should be done in handlers.

## Create the `IncrementCounterRequest`
1. In the Client project's `Featuers\Counter` folder add a new folder named `IncrementCount`.
2. In this folder create a class named `IncrementCountAction.cs`.
* The class should inherit from `IRequest`
as follows:

[!code-csharp[CounterState](../BlazorStateSample/BlazorStateSample.Client/Features/Counter/IncrementCount/IncrementCountAction.cs)]

## Sending the request/action through the mediator pipeline

To Send the action to the pipeline when the user clicks the `Click me` button. 

Add the following method to `Pages\Counter.cshtml.cs`.

[!code-csharp[CounterState](../BlazorStateSample/BlazorStateSample.Client/Pages/Counter.cshtml.cs?name=IncrementCount)]

## Handling the request/action 

The `Handler` is where we actually mutate the state to fulfill the `Request`.  

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

[get started with Blazor]: <https://docs.microsoft.com/en-us/aspnet/core/client-side/spa/blazor/get-started)>