---
uid: blazor-state.Terminology.md
title: Terminology
---

## Terminology

The pattern used by Blazor-State and MediatR has been around for many years and goes by different names.
We list various related terms here and **Bold** indicates the term used in Blazor-State.

### Signals/**Actions**/**Requests**/Commands/

In Redux they call them "Action".  
In UML they are "Signal".  
In Command Pattern they are "Command"  
In MediatR they are `Request`
In Blazor-State we call them `Actions` when they are handled on the Client and `Requests` if they handled on the Server.

### Reducer/**Handler**/Executor

This is the code that processes the `Request/Action` and returns the `Response`.

In Redux they call them "Reducer".
In Command Pattern we call them "Executor".  
In MediatR they are `Handler`.  
In Blazor-State we call them `Handler`.

### Feature

A Feature is the collection of the code needed to implement a
particular [Vertical Slice](https://jimmybogard.com/vertical-slice-architecture/)
of the application.  

On the server side we use the same architecture, where the Features contain
`Endpoint`, `Handler`, `Request`, `Response`, etc...
Each endpoint maps the HTTP Request to the `Request` object and then sends the `Request`
on to the mediator pipeline.
The `Handler` acts on the `Request` and returns a `Response`.

Server side follows the `Request` in `Response` out pattern.

A Feature Folder on the client side will contain an `Action` and the `Handler`
and any corresponding files needed for this feature.
The `Handler` acts on the `Action` and updates the corresponding `State`.

Client side follows the `Action` in new `State` out pattern.