---
uid: BlazorState:Migration5-6.md
title: Migrate From 5.X to 6.X
---

# Migration

## From 5.x to 6.x

### MediatR 10 to 11 migration

In this release we migrate from MediatR 10 to 11 see the migration notes here https://github.com/jbogard/MediatR/wiki/Migration-Guide-10.x-to-11.0

### AddBlazorState and UseReduxDevTools

We have changed how the user can configure ReduxDevTools

from being a simple boolean to a method using the options pattern.

locate `AddBlazorState` in you code and change

```cs
  .UseReduxDevToolsBehavior = true;
```

to

```cs
  .UseReduxDevTools()
```

or if you want to change the options from the default you can use something similar to below.

```cs
  .UseReduxDevTools
  (
    aReduxDevToolsOptions => 
      {
        aReduxDevToolsOptions.Name = "Test App";
        aReduxDevToolsOptions.Trace = true; 
      }
  );
```

See the ReduxDevToolsOptions class for more details.
