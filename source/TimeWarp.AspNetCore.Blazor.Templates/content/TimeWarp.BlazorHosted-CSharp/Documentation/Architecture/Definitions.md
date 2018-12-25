# Client Project (C#)

**Action**: 

**ActionHandler**:

**Behavior**: 

**Component**:

**Feature**: Abstract concept that does not have an actual implementation but is the name for the grouping.

**Interop**:

**InteropDto**:

**Page**:

**State**:

# Client TypeScript Project

**Action**:

**Interop**:

**InteropDto**:


# Server Project (C#)
**Controller**:

**Entity**:

**RequestHandler**:

**MappingProfile**:

**DbContext**:

**Persistence**:  The Database to persist data.  The default starting point is SQL Server.  
Using LocalDB for localDevelopment. 
If NoSQL is needed I will first ask "Why?"  
The NOSQL Dbs get a bunch of hype and I am no fan of the SQL language, yet 

# Shared Project (C#)

**Request**: This is a class that implements the [MediatR](https://github.com/jbogard/MediatR/) `IRequest`.

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