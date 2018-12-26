# Client Project (C#)

**Action**: Actions are the client version of Requests.  
Named differently solely to clarify where they are expected to run.  
Actions are on the Client and Requests are on the server.

**ActionHandler**: The Client version of RequestHandler. 
ActionHandler's are implemented as child classes of the State they will act upon, 
(so they have access to the private members of the State) and inherit from the `BaseHandler` as in :
```csharp
public partial class CounterState
{
  public class IncrementCounterHandler : BaseHandler<IncrementCounterAction, CounterState>
  ...
```

**Behavior**:  
New in MediatR 3.0 are [behaviors](https://github.com/jbogard/MediatR/wiki/Behaviors), 
which allow you to build your own pipeline. 
A pipeline behavior is an implementation of `IPipelineBehavior<TRequest, TResponse>`. 
It represents a similar pattern to filters in ASP.NET MVC/Web API or pipeline behaviors in NServiceBus. 

**Component**: [BlazorComponents](https://blazor.net/docs/components/index.html). 
A component is a self-contained chunk of user interface (UI), such as a page, dialog, or form.
A component includes both the HTML markup to render along with the processing logic, `Model`, needed to inject data or respond to UI events. 
Components are flexible and lightweight, and they can be nested, reused, and shared between projects.

**Feature**: Abstract concept that does not have an actual implementation but is the name for the grouping.

**InteropDto**: 
This is the C# object used to define the data transfered between the TypeScript Client 
and C# Client.

**Page**: This is a special `Component` in that a `Page` defines a `Route` and a `Layout`.
Pages are thus potential entry points into the application.

**State**: a Class that defines a portion of State to be managed.
Each class will need to inherit from `State<>` as in below:
```csharp
public partial class CounterState : State<CounterState>
```
State may be made up collections of other objects that also need to be defined.


# Client TypeScript Project

**Action**: This is a TypeScript Interface that corresponds with the C# Action.  
Facilitating the ability for the TypeScript Client to Dispatch Actions to the C# Client Pipeline.

**Interop**: 
TypeScript Class that controls the interoperability between C# and JavaScript. 
Typically for using npm packages but also for direct Javascript functionality that is not yet available to Blazor/WASM.

**InteropDto**: Objects that are used to transfer data between the C# client and the TypeScript client.


# Server Project (C#)
**Controller**: This is a ASP.Net Controller that inherits from `BaseController`.  
The Controller is used to bind the HttpRequest Data to the Request Object 
and to transfer back the Response.  
Each Controller has only a single endpoint and has a one to one mapping with a `RequestHandler`

**Entity**: This is the C# object representing the persisted Entities.  
The DbContext will contain collections of these.

**RequestHandler**: This is a class that implements the [MediatR](https://github.com/jbogard/MediatR/) `IRequestHandler<TRequest, TResponse>`.

**MappingProfile**:  AutoMapper Profile for configuring the Mapping of Entities to Responses and DTOs.

**DbContext**: Entity Framework DbContext That communicates with the Persistence System.

**Persistence**:  The Database to persist data.  The default starting point is SQL Server.  
Using LocalDB for localDevelopment. 
If NoSQL is needed I will first ask "Why?"  
The NOSQL Dbs get a bunch of hype and I am no fan of the SQL language, yet 

# Shared Project (C#)

**Request**: This is a class that implements the [MediatR](https://github.com/jbogard/MediatR/) `IRequest`.
The Mediator pipeline processes `Requests` and outputs `Response`

**Response**: This is a class that implements the [MediatR](https://github.com/jbogard/MediatR/) `IRequestHandler<TRequest, TResponse>`.

**DTO**: Data Transfer Object. A Request or a Response can contain reference to objects.  
The sole purpose of these is to define objects that are to be transfered between the server and the client projects.
Do NOT reuse an Entity Object or Service Objects as DTOs. 
It may not appear to be "DRY" but in this case the Single Responsibility Principle far outweighs DRY.
It is ok to reuse DTOs in various locations as long as they are just DTOS.
But be aware that changes to these DTOs then will affect all of the Requests/Responses that use them.


**Validator**: This is a [FluentValidation](https://fluentvalidation.net/) Class that implements `AbstractValidator<SomeRequest>`.
Typically a Request object will have a corresponding validator. And any of the DTOs can also have validators.


# General Definitions

DRY: Don't Repeat Yourself. https://en.wikipedia.org/wiki/Don%27t_repeat_yourself

SOLID: https://en.wikipedia.org/wiki/SOLID 

Single Responsibility Principle: https://en.wikipedia.org/wiki/Single_responsibility_principle