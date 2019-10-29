---
uid: BlazorState:Release.2.0.0.md
title: Release 2.0.0
---

## Release 2.0.0

### Breaking Changes
Since have moved from preview builds to release, and ReduxDevTools should be disabled in production, we are changing the default option setting to be disabled.

`BlazorStateOptions.UseReduxDevToolsBehavior` now defaults to false; 

See [Migrations](xref:BlazorState:Migration1-2.md) for how to migrate existing projects from 1.0 to 2.0.

### Improvements

`Action`s and their `Handler`s should be nested classes of the `State` they act on. Previously the developer could forget this and it would take some time to figure out the problem (I know I did it.).  Now RenderSubscriptionsPostProcessor with throw an exception if the IAction is not nested within an IState.
