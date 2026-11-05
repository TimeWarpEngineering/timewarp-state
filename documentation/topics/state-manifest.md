---
uid: TimeWarpState:StateManifest.md
title: State manifest
---

# State manifest

The source generator writes a manifest into every assembly that declares a state. The manifest lists the states and the actions nested in each state. Tools can read it before any action runs. DevTools uses it to show states and actions up front. Startup checks and analyzers in other projects can read it too.

You write nothing to get it. It is generated next to the dispatch methods, the action catalog and the clones.

## What is generated

`TimeWarp.State.StateManifest.g.cs` holds two things:

- An internal `TimeWarpStateManifest` class with `Version` and `Json` constants. It is in the namespace named after the assembly, the same one the action catalog uses.
- `[assembly: TimeWarp.State.StateManifestAttribute(Version, Json)]`.

An assembly with no state gets no manifest.

The document, version 1:

```json
{"version":1,"assembly":"Test.App.Client","states":[
  {"type":"Test.App.Client.Features.Counter.CounterState","name":"CounterState",
   "persistent":null,"clone":"generated",
   "actions":[{"type":"Test.App.Client.Features.Counter.CounterState+IncrementCountActionSet+Action",
               "name":"IncrementCountActionSet.Action","method":"IncrementCount",
               "parameters":[],"catalog":null,"tracked":false}]}]}
```

| Field | Meaning |
|---|---|
| state `type` | `Type.FullName`, with nested types joined by `+` |
| state `name` | `Type.Name`. Telemetry puts the same value in the `timewarp.state.state_type` span tag. |
| `persistent` | `SessionStorage` or `LocalStorage` for a `[PersistentState]` state, otherwise `null` |
| `clone` | `generated` when the generator clones the state, `cloneable` when the state implements `ICloneable` |
| action `name` | the nested name below the state. Telemetry puts the same value in the `timewarp.state.action` span tag. |
| `method` | the state method that sends the action, generated or hand-written, or `null` when there is none |
| `parameters` | the method's parameters: `name`, `type`, `default` (source text or `null`), `required` |
| `catalog` | the action catalog name for a `[CatalogAction]` action, otherwise `null` |
| `tracked` | `true` for a `[TrackAction]` action |

States and actions are sorted by type name. Readers ignore fields they do not know, so later versions can add facts.

## Read it at runtime

```csharp
StateManifest? manifest = StateManifest.FromAssembly(typeof(CounterState).Assembly);
foreach (StateManifestState state in manifest?.States ?? [])
{
  Console.WriteLine($"{state.Name}: {string.Join(", ", state.Actions.Select(action => action.Method))}");
}
```

`FromAssembly` reads the attribute instance and parses the document with `JsonDocument`. It uses no member reflection, so it is safe under trimming and Native AOT. `StateManifest.Parse(json)` parses a document from anywhere else.

## Read it from an analyzer

An analyzer in a project that references the assembly reads the attribute from metadata:

```csharp
IAssemblySymbol referenced = (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(reference)!;
AttributeData? manifest = referenced.GetAttributes()
  .FirstOrDefault(data => data.AttributeClass?.ToDisplayString() == "TimeWarp.State.StateManifestAttribute");
string? json = manifest?.ConstructorArguments[1].Value as string;
```

An architecture coverage check can compare its contracts to this list. It does not need to re-derive State's rules.

## DevTools

An inspected app posts its manifests to DevTools once at startup:

```csharp
builder.Services.AddTimeWarpStateDevToolsManifest(typeof(CounterState).Assembly);
```

The call is in `TimeWarp.State.DevTools.Export`. It does nothing when `TIMEWARP_STATE_DEVTOOLS_ENDPOINT` is unset, and a stopped inspector never fails the app. DevTools keeps the newest manifest for each assembly. Its **Manifest** tab lists every state and its actions before any span arrives.

Dispatching an action from DevTools is not built yet. The manifest already carries what it needs: the method, the parameters and the catalog name. See [DevTools](xref:TimeWarpState:DevTools.md).
