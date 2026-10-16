---
uid: TimeWarpState:Release.12.0.0-beta.16.md
title: Release 12.0.0-beta.16
---

## Release 12.0.0-beta.16

### TimeWarp DevTools

- The inspector product is **TimeWarp DevTools**. State is the label on the existing action log. The browser tab title is **TimeWarp DevTools**.
- **Breaking:** `AddTimeWarpStateDevTools()` and `AddTimeWarpStateDevToolsProject()` default the Aspire resource name to `timewarp-devtools`. The previous default was `timewarp-state-devtools`. A caller that passes `name` still uses that name.
- **Breaking:** the dashboard link text is **TimeWarp DevTools**. Endpoint name `http` and `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` are unchanged.
- Package ids, namespaces, and the `AddTimeWarpStateDevTools` API are unchanged.

timewarp-architecture must update its tests when it takes this package. The IngressSmoke `StateDevTools` test, `Publish_Should_HaveNoStateDevTools`, and any match on the previous resource name or the previous dashboard label need the new values.
