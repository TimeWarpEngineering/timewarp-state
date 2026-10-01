---
uid: TimeWarpState:Release.12.0.0-beta.7.md
title: Release 12.0.0-beta.7
---

## Release 12.0.0-beta.7

### Features

- `[CatalogAction(DisplayName = "…")]`: an optional, authored human-facing label for command palettes and menus (for example `"Link Microsoft 365"`). `Name` stays the stable identifier. `ActionCatalogEntry.DisplayName` carries the generator-copied literal and is `null` when not set; TimeWarp.State never derives a fallback. See [Action catalog](../topics/action-catalog.md).
- New analyzer rule TWS0008 (Error): `DisplayName`, when given, must not be empty or whitespace.

### Breaking changes

- `ActionCatalogEntry` gains a trailing optional `displayName` constructor parameter. Source compatible; code compiled against beta.6 that calls the constructor directly must be rebuilt.
