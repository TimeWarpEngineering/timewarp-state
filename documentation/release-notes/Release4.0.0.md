---
uid: BlazorState:Release.4.0.0.md
title: Release 4.0.0
---

## Release 4.0.0

### Breaking Changes
* Blazor-State has moved to netstandard 2.1
* The BlazorState Javascript location has changed.

See [Migrations](xref:BlazorState:Migration1-2.md) for how to migrate existing projects from 1.0 to 2.0.

## Release 4.0.1

* Updated Mediator to 9.0 https://jimmybogard.com/mediatr-9-0-released/
* Update Nuget Dev Dependencies: SourceLink, Fixie
* Update Nuget Production Dependency:  TypeSupport now supports HashTables.

## Release 4.1.0

* Added support for dotnet 6.
* Updated MediatR to 10.0 https://jimmybogard.com/mediatr-10-0-released/
* Include debug symbols in main nuget package vs separate.
* Switch from Shouldly to FluentValidations
* Move to `Directory.Packagaes.props`
* Start the migration to move the TypeScript code into the BlazorState.csproj file and remove webpack.
* In dotnet 6 there are new capabilities to initialize JS modules. See blazor-state.lib.module.ts this only sets up the structure to use this capability. It is NOT actually used yet.
* Move to file based namespaces;
* Run dotnet format on all.
* 

