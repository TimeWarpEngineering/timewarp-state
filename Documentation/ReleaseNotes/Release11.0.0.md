---
uid: TimeWarpState:Release.11.0.0.md
title: Release 11.0.0
---

## Release 11.0.0

### Major Changes

**Rebranding to TimeWarp.State**
We're excited to announce that Blazor-State has been rebranded to TimeWarp.State. 
This change reflects our evolving vision and commitment to providing a robust state management solution for Blazor applications[1].

### Breaking Changes

See [Migrations](xref:BlazorState:Migration10-11.md) for instructions on how to migrate from version 10.0 to 11.0.

- `TimeWarp.State` now requires .NET 8.0 or later. 
- All references to `BlazorState` should be updated to `TimeWarp.State`.
- The package name has changed from `Blazor-State` to `TimeWarp.State`.
  You'll need to update your package references and namespaces accordingly.

### New Features
- Analyzers
  -  
- Source Generators
  - ActionSet
- Introducing `TimeWarp.State.Plus` NuGet package:
  - **PersistentState**:
    - **Key Feature**: Automates the persistence of state in browser storage.
    - **Usage**: Annotate state classes with `[PersistentState]` attribute to enable.
    - **Storage Options**: Supports both `LocalStorage` and `SessionStorage` for storing state data.

> Note: If you are not ready to update to .NET 8.0, you should continue using BlazorState version 10.0.

See [Migrations](xref:BlazorState:Migration10-11.md) for instructions on how to migrate from version 10.0 to 11.0.



### Breaking Changes


* [List any API changes, removed features, or other breaking changes here]

### New Features

* [List any new features introduced in this version]

### Improvements

* [List any significant improvements or optimizations]

### Bug Fixes

* [List any notable bug fixes]

### Documentation

* Updated documentation to reflect the new branding and any changes in usage.
* [Mention any significant additions or changes to the documentation]

### Migration Guide

For detailed instructions on how to migrate from Blazor-State to TimeWarp.State, please refer to our [Migration Guide](link-to-migration-guide).

### Other Changes

* [List any other relevant changes, dependency updates, etc.]
