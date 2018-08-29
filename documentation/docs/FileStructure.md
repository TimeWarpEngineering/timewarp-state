---
uid: FileStructure:FileStructure.md
title: File Structure
---

## File Structure

## Features
With the mediator pattern for each `Request` there is an associated `Handler` 
and possibly other items like a `Validator`, `Mapper` etc.. 
The `Request`, `Handler` and associated items are what we call a `Feature`.  
We organize the Features by the `State` they act upon.

Ideally we would colocate all of these related items in the same folder.
But with Blazor application we have 3 projects Server Client and Shared and thus they have to be split.
(Someday maybe we could extract the Client from a project by providing a single entry point to the IlLinker but not currently an option).

Please see Sample application for example of file structure.

We follow the same folder structure under each project.
And split the feature accordingly.

```
-Server
  - Features
    - <State>
      - <Feature>
        - <Feature><State>Controller.cs
        - <Feature><State>Handler.cs
        - <Feature><State>Validator.cs
        - <Feature><State>Mapper.cs
- Client
  - Features
    - <State>
      - <Feature>
        - <Feature><State>Request.cs
        - <Feature><State>Response.cs
        - <Feature><State>Validator.cs
    - <State>.cs
- Shared
  - Features
    - <State>
      - <Feature>
        - <Feature><State>Request.cs
        - <Feature><State>Response.cs
        - <Feature><State>Validator.cs
      - <Entity>Dto.cs
      - <Entity>DtoValidator.cs
```