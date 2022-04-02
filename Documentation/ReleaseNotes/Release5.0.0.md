---
uid: BlazorState:Release.5.0.0.md
title: Release 5.0.0
---

## Release 5.0.0

### Breaking Changes
* Blazor-State has moved to net6.0
* `<script src="_content/Blazor-State/blazorstate.js"></script>` is no longer needed and should be removed.
* Updated Mediator to 10.0.1 https://jimmybogard.com/mediatr-10-0-released/
* Requires Browsers that support module import.  [CanIUse](https://caniuse.com/?search=module)

See [Migrations](xref:BlazorState:Migration4-5.md) for how to migrate existing projects from 4.0 to 5.0.

### Other Changes

* Include debug symbols in main nuget package vs separate.
* Switch from Shouldly to FluentValidations
* Move to `Directory.Packagaes.props`
* Move to esproj and remove webpack.
* In dotnet 6 there are new capabilities to initialize JS modules.
* Move to file based namespaces;
* Run dotnet format on all.

## Release 5.0.1

* Fixed the bug that the javascript wasn't actually being included in the dll.
* Updated Logging to utilize ILogger Structured Logging.
* Use EventIds for all LogMessages this makes filtering better and also requires developer to think about logging vs just throwing them everywhere or nowhere.
* Moved from Azure Pipelines to Github Actions.

## Release 5.0.2

* Fixed duplicate EventIds
