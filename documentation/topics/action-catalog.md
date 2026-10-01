---
uid: TimeWarpState:ActionCatalog.md
title: Action catalog
---

# Action catalog

The action catalog lets a host enumerate an app's user-facing actions at runtime and execute them through the store — for example to fill a Ctrl-K command palette or to expose agent tools. It is opt-in: an action without `[CatalogAction]` is never cataloged, so `Fetch*`, `Clear*`, debug and inbound hub actions stay out by default.

## Mark an action

Put `[CatalogAction]` on the nested `Action` class of an `*ActionSet`:

```csharp
public partial class CredentialsState
{
  public static class AddPasskeyActionSet
  {
    [CatalogAction
    (
      Description = "Add a passkey to the signed-in account.",
      Permissions = ["credentials.write"],
      Visibility = ActionVisibility.Both
    )]
    public sealed class Action : IAction
    {
      public Action(string label, bool makeDefault = false) { … }
    }

    internal sealed class Handler(IStore store) : StateActionHandler<Action>(store) { … }
  }
}
```

| Property | Required | Meaning |
|----------|----------|---------|
| `Description` | yes | One plain sentence shown to people and agents. |
| `Name` | no | Defaults to `<StateWithoutSuffix>.<ActionSetWithoutSuffix>`, here `Credentials.AddPasskey`. Unique per assembly. The stable identifier. |
| `DisplayName` | no | Human-facing label for command palettes and menus, for example `"Link Microsoft 365"`. Authored copy, never derived: `ActionCatalogEntry.DisplayName` is `null` when it is not set, and the consumer chooses the fallback (for example `entry.DisplayName ?? FormatLabel(entry.Name)`). |
| `Permissions` | no | Consumer-defined permission or policy ids. Opaque to TimeWarp.State. |
| `Visibility` | no | `Human` (default), `Agent`, or `Both`. |

## What is generated

The source generator emits one internal `GeneratedActionCatalog` per assembly, in a namespace equal to the assembly name. `GeneratedActionCatalog.All` is an `IReadOnlyList<ActionCatalogEntry>` ordered by name. Each entry carries:

- `Name`, `DisplayName` (or `null`), `Description`, `Permissions`, `Visibility`
- `StateType` and `ActionType`
- `Parameters`: name, CLR type, required flag, default value text and a JSON schema fragment — taken from the action's first explicit constructor, the same parse that generates `State.AddPasskey(label, makeDefault, ct)`
- `InputSchema`: a JSON schema object for the parameters
- `Execute(IStore store, object?[]? arguments, CancellationToken ct)`: resolves the state from the store and calls the generated `State.Method(...)`. Arguments are positional in constructor order; trailing optional arguments may be omitted. No reflection is involved, so it is AOT and trim safe.

Arguments already of the parameter type are passed through; otherwise the values the schema advertises are converted with the invariant culture (enum from name or integer, `Guid`/date/time types from string, numbers and `bool` via `IConvertible`, for example `"5"` or `5L` for an `int`). A failed conversion throws `ArgumentException`.

Input schemas cover `bool`, integers, floating point, `string`, `char`, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeSpan` and enums (by member name). Complex parameter types such as a `Command` record are described by CLR type name only in this version (`{"x-clr-type":"App.Command"}`, and `ActionCatalogParameter.JsonSchema` is `null`); converting JSON into CLR arguments is the host's job.

An assembly with no cataloged actions gets no generated catalog.

## Register and aggregate assemblies

```csharp
builder.Services.AddActionCatalog
(
  typeof(Web.Spa.AssemblyMarker).Assembly,
  typeof(Features.Credentials.AssemblyMarker).Assembly
);
```

`AddActionCatalog` reads each named assembly's generated `[assembly: GeneratedActionCatalogProvider]` attribute and registers a singleton `IActionCatalog` that concatenates the entries. Only the assemblies you name are read; there is no global scanning. Call it again to add more assemblies. The same name declared in two assemblies throws `InvalidOperationException` when the catalog is resolved.

```csharp
@inject IActionCatalog ActionCatalog
@inject IStore Store

ActionCatalogEntry? entry = ActionCatalog.Find("Credentials.AddPasskey");
if (entry is not null && permissionService.IsAllowed(entry.Permissions))
{
  await entry.Execute(Store, ["Laptop"]);
}
```

## What the catalog does not do

- **Permissions are consumer-enforced.** `Execute` never checks `Permissions`; filter and authorize before you show or run an entry.
- **Ranking is the consumer's.** The catalog is an unordered set of descriptors (sorted by name for determinism); a palette or agent host decides relevance.
- **`[TrackAction]` is unrelated.** It only drives the busy indicator in TimeWarp.State.Plus and does not catalog anything; `[CatalogAction]` does not affect tracking.

## Analyzer rules

| Id | Severity | Rule |
|----|----------|------|
| TWS0004 | Error | `[CatalogAction]` is only allowed on the nested `Action` class of an `*ActionSet`; no generic types in the chain and no private/protected nesting. |
| TWS0005 | Error | `Description` is missing or empty. |
| TWS0006 | Error | Two cataloged actions in one assembly share a `Name`. |
| TWS0007 | Warning | `Description` is not one plain sentence (line break or more than one sentence). |
| TWS0008 | Error | `DisplayName` is set but empty or whitespace. (Attribute arguments are always compile-time constants; the compiler reports CS0182 otherwise.) |
