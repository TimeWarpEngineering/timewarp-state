---
uid: BlazorState:Release.8.0.0.md
title: Release 8.0.0
---

## Release 8.0.0

### Breaking Changes

* Blazor-State now requires MediatR version 12

See [Migrations](xref:BlazorState:Migration7-8.md) for instructions on how to migrate from version 7.0.0 to 8.0.0.

### Other Changes

* Updates to the Sample app and test app to use MediatR version 12
* Updates to Scrutor and MediatR.Extensions.Microsoft.DependencyInjection
* Updates to Microsoft.AspNetCore packages
* Updates to Fixie, Fixie.TestAdaptor, and TimeWarp.Fixe
* Removed TestCafe and added a PowerShell script to run tests
* JavaScript increment issue has been fixed and all tests are passing
* Updated version number to 8.0.0-alpha.0+7.0.103

## Release 8.0.1

* [Issue 332](https://github.com/TimeWarpEngineering/blazor-state/issues/332) Upgrading to 8.0.0 breaks viewer components updating 

## Release 8.1.0

* [Issue 334](https://github.com/TimeWarpEngineering/blazor-state/issues/334) Add Go Back

* Added Go Back to Route state which allows accessing the navigation history.
* Updated the version of the Microsoft.TypeScript.MSBuild package to 5.0.3.

## Release 8.2.0

* Included BlazorStateAnalyzer, which replaces runtime checking of the IActions nesting with compile-time checking for better performance.
* [Issue 342](https://github.com/TimeWarpEngineering/blazor-state/issues/342) Doubly nested IActions throw an error.


## Release 8.2.1

* move the Analyzer from `analyzers/cs` to `analyzers/dotnet/cs` so it will be automatically found.

## Release 8.2.2

* Fix issue #347  The analyzer will now work with class record or structs
* Added some testing and scipts to locally deploy to nuget etc.

## Release 8.2.3

* Change BlazorStateAnalyzerDebug DiagnosticSeverity from warning to info

## Release 8.2.4
* Really fix the Analyzer this time.  And remove BlazorStateAnalyzerDebug (don't need it now)

## Release 8.2.5
* Drop the dependency down one to the LTS level

## Release 8.2.6
* Analyzer won't check abstract classes of IAction are nested in IState.

## Release 8.2.7
* BlazorStateAnalyzer needs to target netstandard2.0 Fixes 355 