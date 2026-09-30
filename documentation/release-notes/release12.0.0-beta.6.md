---
uid: TimeWarpState:Release.12.0.0-beta.6.md
title: Release 12.0.0-beta.6
---

## Release 12.0.0-beta.6

### Features

- Opt-in action catalog. `[CatalogAction(Description = "…")]` on an `*ActionSet.Action` puts the action in a generated per-assembly registry (`GeneratedActionCatalog.All`) with its name, description, permissions, visibility, parameters, input schema, and a reflection-free `Execute(IStore, object?[], CancellationToken)`. `services.AddActionCatalog(typeof(Marker).Assembly, …)` aggregates assemblies into `IActionCatalog`. See [Action catalog](../topics/action-catalog.md).
- New analyzer rules: TWS0004 (placement), TWS0005 (Description required), TWS0006 (duplicate name), TWS0007 (Description is one plain sentence).
