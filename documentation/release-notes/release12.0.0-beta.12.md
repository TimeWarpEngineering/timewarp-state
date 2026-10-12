---
uid: TimeWarpState:Release.12.0.0-beta.12.md
title: Release 12.0.0-beta.12
---

## Release 12.0.0-beta.12

### DevTools store identity

- **Each span names its store.** Init and action spans carry `timewarp.state.store`, the `Guid` created with that `Store`. When `AddTimeWarpStateBlazor` is registered, spans also carry `timewarp.state.render` (`prerender` or `interactive`). A host that cannot tell omits the render tag.
- **The inspector shows both init rows.** A Blazor Server page with prerendering builds two stores. Both initializations stay in the log. Each row has a badge: **pre**, **live**, or the first four characters of the store id. The colour stays with the store id. The Action tab shows the full id. The filter matches a store id or badge.
- **The State panel folds one store.** Current state is the store that first appeared latest in the log. A selector lists stores newest first, so a prerender snapshot does not replace the live store.

See [DevTools](xref:TimeWarpState:DevTools.md).
