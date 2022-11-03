---
uid: BlazorState:Release.6.0.0.md
title: Release 6.0.0
---

## Release 6.0.0

### Breaking Changes

* Blazor-State has moved from MediatR 10 to 11
* UseReduxDevToolsBehavior changes from bool to a function that uses the options pattern.
  This now gives us the ability to enable stack traces in ReduxDevTools

See [Migrations](xref:BlazorState:Migration5-6.md) for how to migrate existing projects from 5.0 to 6.0.

### Other Changes

* Remove esproj and add move typescript directly into BlazorState.csproj utilizing `Microsoft.TypeScript.MSBuild` package.
* Fix the tags on the project they should be separated by semicolons.
* Migrate to Fixie 3.2.0
* Use the TimeWarp.Fixie testing convention.
* Update All Nuget packages to the latest
* Add dotnet-cleanup to dotnet-tools.json
* Update Nuget.config to include packageSourceMapping
