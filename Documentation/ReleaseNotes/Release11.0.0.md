---
uid: BlazorState:Release.11.0.0.md
title: Release 11.0.0
---

## Release 11.0.0

### Breaking Changes

- `BlazorState` now requires .NET 8.0 or later. See [Migrations](xref:BlazorState:Migration10-11.md) for instructions on how to migrate from version 10.0 to 11.0.

### New Features

- Introducing `TimeWarp.State.Middleware` NuGet package:
  - **PersistentState**:
    - **Key Feature**: Automates the persistence of state in browser storage.
    - **Usage**: Annotate state classes with `[PersistentState]` attribute to enable.
    - **Storage Options**: Supports both `LocalStorage` and `SessionStorage` for storing state data.

> Note: If you are not ready to update to .NET 8.0, you should continue using BlazorState version 10.0.

See [Migrations](xref:BlazorState:Migration10-11.md) for instructions on how to migrate from version 10.0 to 11.0.
