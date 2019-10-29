---
uid: BlazorState:Migration1-2.md
title: Migrate From 1.X to 2.X
---

# Migration

## From 1.X to 2.X

> [!WARNING]
> ReduxDevTools should NOT be enabled in production.

In 1.0 ReduxDevTools were enabled by default, in 2.0 they are disabled by default.

If your code previously used the default to enabled ReduxDevTools you will now need to explicitly enable it as in the following example `ConfigureServices` method inside `Startup.cs`:

```csharp
aServiceCollection.AddBlazorState
(
  (aOptions) =>
  {
    aOptions.UseReduxDevToolsBehavior = true; // Add This Line
    aOptions.Assemblies =
      new Assembly[]
      {
          typeof(Startup).GetTypeInfo().Assembly,
      };
  }
);
```