# 01-CounterSample
This sample shows how to take the standard Visual Studio Blazor template and add Blazor-State.

[Run this sample online]

### Creating the project
1. Create a new Blazor website using the `Hosted Blazor` template in Visual Studio 
   (See the [Getting Started Guide] for details on how to use Blazor).
2. Name the project `CounterSample`.
3. Add the Blazor-State NuGet package to `CounterSample.Client` project. 
  Note that you might have to tick the checkbox `Include prerelease`.
 
### Configure the services
1. In the `CounterSample.Client` project in the `Program.cs` file. 
2. Add `using BlazorState;`
3. Change the serviceProvider initialization code to add Blazor-State
```
  var serviceProvider = new BrowserServiceProvider(services =>
  {
    services.AddBlazorState(options =>
    {
      options.UseReduxDevToolsBehavior = false; // See other demo on using this
      options.UseRouting = false; // See other demo on routing.
      options.UseCloneStateBehavior = true; // The basics.
    });
  });
```
### File Strucutre of: Feature, State, Request, Handler 
With the Mediator Pattern for each Request there is an associated Handler 
and possibly other items like a Validator, Mapper etc.. 
The Request, Handler and associated items are what I call a `Feature`.  
I organize the Features by the State they act upon.

1. In the Client project add a folder named `Features`.

### Adding state
1. In the `Features` folder add a folder named `Counter`.
1. Within the `Counter` folder create a class file named `CounterState.cs`.
2. Add `using BlazorState;`

Your class should:
* descend from `State<CounterState>`:
* override the `Clone()` method. 
  _Blazor-State will `Clone` the `State` prior to the sending of any `Request`.
  Thus the `Handler` of the request can modify state as desired._
* override the `Initialize()` method. To set the initial values of the state.

4. Enter the code as follows:
```
public class CounterState : State<CounterState>
  {
    /// <summary>
    /// Parameterless constructor needed for deserialization.
    /// </summary>
    public CounterState() { } // needed for serialization

    /// <summary>
    /// Utilize a constructor for cloning
    /// </summary>
    /// <param name="aState"></param>
    protected CounterState(CounterState aState) : this()
    {
      Count = aState.Count;
    }

    public int Count { get; set; }

    public override object Clone() => new CounterState(this);

    /// <summary>
    /// Set the initial state
    /// </summary>
    protected override void Initialize()
    {
      Count = 3;
    }
  }
```
### Seperation of HTML and C#.
I like to keep my C# code separated from the HTML and yet collocated.
So instead of a @functions sections for each .cshtml file we 
will have an associated .cshtml.cs. file and Visual Studio will automatically group these together.
This is not required but I find it easier to reason about.

_Currently this requires the page inherting from the associated Model.  
But I belive this will become a normal feature of Blazor, similar to "Code behind."_


### Create the `CounterModel` for the `Counter` Page.
1. Create a new file named `Counter.cshtml.cs`
CounterModel should:
 * inherit from BlazorComponent
 * implement IBlazorStateComponent
 optionally:
* add property to access State from the store.
as follows:
```
  public class CounterModel : BlazorComponent, IBlazorStateComponent
  {
    public CounterState CounterState => Store.GetState<CounterState>();

    [Inject] public IMediator Mediator { get; set; }
    [Inject] public IStore Store { get; set; }
  }
```

### Displaying state in the user interface without using a base component.
1. Edit `Pages\Counter.cshtml` and add the following `inherits` clause.
```
@inherits CounterModel
```
2. Change the HTML that displays the value of the counter to `@CounterState.Count`.
3. Remove the @functions sections.


### Sending requests that will mutate the state
Changes to state are done by sending a Request through the mediator pipeline.
The Request is then Handled by a Handler which can freely mutate the state.
State should NOT be mutated by the presentation layer (.cshtml) or the associated Model.

### Create Request
1. In the Client project's `Featuers\Counter` folder add a new folder named `IncrementCount`.
2. In this folder create a class named `IncrementCountRequest.cs`.
* The class should inherit from IRequest
as follows:
```
namespace CounterSample.Client.Features.Counter.IncrementCount
{
  using MediatR;

  public class IncrementCountRequest : IRequest
  {
    public int Amount { get; set; }
  }
}
```
### Sending the request to the mediator pipeline

To Send the request to the Pipeline when the user clicks the `Click me` button. 

Add the following method `Pages\Counter.cshtml.cs` file.
```
public void IncrementCount()
    {
      var incrementCountRequest = new IncrementCountRequest { Amount = 3 };
      Mediator.Send(incrementCountRequest);
    }
```

### Handling the request 

The Handler is where we actually mutate the state to fulfill the Request.  

1. In the `Features\Counter\IncrementCounter` folder create a new file named
 `IncrementCountHandler.cs` 

The Handler should:
  * Inherit from BlazorState.Handlers.RequestHandler.
  * The generic parameters are the RequestType and State.
  * Override the Handle method
as follows:
```
  public class IncrementCountHandler : Handlers.RequestHandler<IncrementCountRequest, CounterState>
  {
    public IncrementCountHandler(IStore aStore) : base(aStore) { }

    public CounterState CounterState => Store.GetState<CounterState>();

    public override Task<CounterState> Handle(IncrementCountRequest request, CancellationToken cancellationToken)
    {
      CounterState.Count += request.Amount;
      return Task.FromResult(CounterState);
    }
  }
```

[Run this sample online]: <http://blazor-state-sample-01.azurewebsites.net/>
[Getting Started Guide]: (https://blazor.net/docs/get-started.html)