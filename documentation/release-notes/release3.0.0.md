---
uid: BlazorState:Release.3.0.0.md
title: Release 3.0.0
---

## Release 3.0.0

### Breaking Changes
Blazor-State has moved to netstandard 2.1
The BlazorState Javascript location has changed.

See [Migrations](xref:BlazorState:Migration1-2.md) for how to migrate existing projects from 1.0 to 2.0.

## Release 3.0.1

Update Nuget Dev Dependencies: SourceLink, Fixie
Update Nuget Production Dependency:  TypeSupport now supports HashTables.

## Release 3.1.0

AddBlazorState Extention method has been updated to automatically register all classes that inherit from `State<>`.
   Thus we no longer need to explicitly register States.

Updated Mediator to 8.0 https://jimmybogard.com/mediatr-8-0-released/