---
uid: BlazorState:Release.9.0.0.md
title: Release 9.0.0
---

## Release 9.0.0

### Breaking Changes

* Blazor-State now requires MediatR version 12.1.1 which actually should have been a breaking change on MediatR.

See [Migrations](xref:BlazorState:Migration8-9.md) for instructions on how to migrate from version 7.0.0 to 8.0.0.

### Other Changes

* Updates to the Sample app and test app to use MediatR version 12.1
* Update Microsoft.CodeAnalysis.Common 4.6.0 -> 4.7.0
* Update Microsoft.CodeAnalysis.CSharp.Workspaces 4.6.0 -> 4.7.0
* Updates other development dependencies


## Release 9.0.1

* Added InvalidCloneException to assist in detecting when the clone did not work as expected.