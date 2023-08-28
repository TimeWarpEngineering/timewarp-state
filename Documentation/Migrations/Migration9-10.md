---
uid: BlazorState:Migration9-10.md
title: Migrate From 9.X to 10.X
---

# Migration

## From 9.x to 10.x

If you were implementing IBlazorStateComponent in your own component.  You no longer need to implement IMediator or IStore.

If you were accessing either of these properties via the IBlazorStateComponent interface you will need to create your own interface to use that implements IBlazorStateComponent plus the properties you need.
