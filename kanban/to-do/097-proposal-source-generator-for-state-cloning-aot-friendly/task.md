# Proposal: source generator for state cloning (AOT-friendly)

## Description

**Proposal only. Not approved for implementation.** This task is for analysis and a design proposal. It
makes no product code changes. Steven decides whether, and how, to implement it.

As of 12.0.0-beta.9 (PR #617, task 095), `StateTransactionBehavior` snapshots state before every action using
the reflection-based `TimeWarp.Features.Cloning` deep cloner:

- `source/timewarp-state/features/cloning/clone-extensions.cs` is the public `CloneExtensions.Clone<T>()` /
  `Clone<T>(CloneErrorHandler)` API, and it forwards to `DeepCloner`.
- `source/timewarp-state/features/cloning/deep-cloner.cs` is `internal static class DeepCloner`. It builds a
  per-type `TypePlan` (`PlanKind` plus `FieldPlan(FieldInfo, copyDirect)` list) through
  `GetPlan(type) => Plans.GetOrAdd(type, CreatePlan)` on a
  `ConcurrentDictionary<Type, TypePlan>`. It copies with `field.Info.GetValue(source)` /
  `field.Info.SetValue(clone, …)`, and copies arrays with `Array.GetValue/SetValue`. It creates instances with
  `CreateFactory`, which tries a parameterless ctor, then the fewest-parameter ctor with default arguments, then
  `RuntimeHelpers.GetUninitializedObject`. Cycles and shared references go through a per-call
  `CloneContext.Visited` (`Dictionary<object, object>` with `ReferenceEqualityComparer`). Fields are skipped by
  attribute name (`IgnoredAttributeNames`: IgnoreDataMember, NonSerialized, JsonIgnore), including on
  auto-property backing fields, and by the `FieldInfo.IsNotSerialized` flag.
- `source/timewarp-state/features/pipeline/state-transaction-behavior.cs` does the dispatch:
  `originalState is ICloneable cloneable ? (IState)cloneable.Clone() : originalState.Clone((ex, path) => …)`.
- `source/timewarp-state/state/state.cs` keeps per-instance members out of the clone: `[IgnoreDataMember] Guid`,
  `[IgnoreDataMember] CancellationTokenSource`, and `[JsonIgnore] ISender<ClientPipeline> Sender`.

### Motivation

- **AOT and trimming.** `FieldInfo.GetValue/SetValue`, ctor discovery through reflection,
  `GetUninitializedObject` and walking `Type.GetFields` over arbitrary runtime types do not work well with Native
  AOT or the IL trimmer. Trimmed WASM apps can lose the fields or ctors the cloner looks for, and the code would
  need `[DynamicallyAccessedMembers]` / `[RequiresUnreferencedCode]` annotations that flow into consumers.
- **First-use cost.** Each state type pays to build its `TypePlan` (reflection over fields and attributes, ctor
  selection) the first time it is cloned. That is noticeable on browser WASM startup and on the first action
  per state.
- **Throughput.** Reflection get/set per field on every action is slower than direct member assignment.

A source generator could emit the copy plan at build time. Clones would then be AOT-compatible, safe under
trimming, and faster, while keeping today's semantics.

## Requirements

- Proposal deliverable only: a design write-up in Results, with the open questions answered or explicitly
  deferred. Do not change product code under this task.
- The design must keep current clone semantics so existing states behave the same:
  - Members marked IgnoreDataMember, NonSerialized or JsonIgnore (matched by name, any namespace, including
    backing fields) are not copied, and keep their constructor values. Each `State` clone therefore keeps a
    fresh `Guid`, and `Sender` / `CancellationTokenSource` are not shared.
  - `ICloneable` remains the escape hatch for hand-tuned clones and wins over everything else.
  - Shared/immutable types (string, primitives, dates, Guid, Uri, Version, delegates, `MemberInfo`, `Type`,
    comparers, `IServiceProvider`, threading types, …) stay copied by reference, matching
    `DeepCloner.IsShared`.
  - Cycles and shared references inside one clone are preserved.
  - Cloning never blocks (no `SemaphoreSlim.Wait`, locks or other waits), so it stays safe on single-threaded
    browser WASM.
- Graceful fallback: any type the generator cannot see or handle (defined in another assembly, open generic,
  inaccessible, unsupported shape) must fall back to `DeepCloner`. It must not fail the build or change
  runtime behavior.

## Checklist

Proposed future implementation steps. **Proposal, not approved.** Do not start these until Steven approves.

- [ ] Decide the open questions in Notes and record the decisions in Results
- [ ] Decide where the generator lives: the existing `source/timewarp-state-source-generator` project (ships
      in the TimeWarp.State package as `analyzers/dotnet/cs`), or a separate analyzer package
- [ ] Generator: for each opted-in state type, emit a clone method (for example a `partial` member or a
      generated `IStateCloner<TState>` registered in a static lookup) plus helpers for the reachable member
      types it can see
- [ ] Generated code honors IgnoreDataMember / NonSerialized / JsonIgnore exactly as `DeepCloner.IsIgnored`
      does, including attributes on properties that apply to their backing fields
- [ ] Generated code uses a reference map equivalent to `CloneContext.Visited` for cycles and shared references
- [ ] Fallback to `DeepCloner` for unsupported members or types, and an analyzer diagnostic (new TW id,
      Info/Warning, documented in AnalyzerReleases.Unshipped.md) naming each member or type that falls back
- [ ] `StateTransactionBehavior` dispatch order: `ICloneable`, then generated clone, then `DeepCloner`
- [ ] Parity tests: generated clone vs `DeepCloner` on the existing `deep-cloner-tests.cs` cases (private
      fields, ignored members, nested collections, cycles, multi-dimensional arrays, structs, shared
      delegates and types, null) plus every test-app state
- [ ] Keep the WASM E2E clone suite green (`CloneTestPageTests.CloneSuitePassesInServerAndWasm`,
      `CounterTests`)
- [ ] Trim/AOT smoke test: publish a sample WASM app with trimming (and Native AOT for a console host if
      practical) and `TrimmerSingleWarn=false`; there should be no IL2xxx/IL3xxx warnings from the generated
      path, and clones should be correct at runtime
- [ ] Benchmark first clone and steady-state clone (generated vs reflection) and record the numbers
- [ ] Docs: update the cloning topic, claude.md, and the release notes

## Notes

### Proposed design

1. **Emission.** For each state type (and each reachable member type the generator can fully see in the
   current compilation), emit a strongly typed clone, `TState CloneGenerated(TState source, CloneMap map)`.
   It allocates the target and assigns each copied member directly. Reference-type members recurse into
   their generated clone, or into `DeepCloner` when none exists.
2. **Opt-out attributes.** Use the same name-based IgnoreDataMember / NonSerialized / JsonIgnore checks. The
   generator resolves them from symbols (`IFieldSymbol` / `IPropertySymbol` and the property's backing field).
   Ignored members are left as the constructor set them, so `State.Guid` stays unique and `Sender` is not
   shared.
3. **ICloneable escape hatch** stays first. A type that implements `ICloneable` gets no generated clone, or
   the generated one is bypassed.
4. **Fallback.** Types that are external (metadata only), open generic, inaccessible from generated code, or
   unsupported shapes route to `DeepCloner.Clone`. An optional analyzer diagnostic lists each fallback so
   authors can fix them (make the type partial or accessible, or add ICloneable).
5. **Dispatch** in `StateTransactionBehavior`: `ICloneable`, then generated clone (looked up through a
   generated registry or a static abstract / partial member on the state), then reflection `DeepCloner`.

### Open questions

- **Opt-in:** implicit for every `IState` / `State<TState>` in the compilation, or explicit through an
  attribute (for example `[GenerateClone]`)? Does implicit require states to be `partial` (a breaking change),
  or should the generator emit an external cloner class instead?
- **Constructors with parameters only:** today `CreateFactory` uses a parameterless ctor, then the
  fewest-parameter ctor with defaults, then `GetUninitializedObject`. `State<TState>` has a protected DI
  constructor `State(ISender<ClientPipeline> sender)` plus a protected `[JsonConstructor] State()`, but derived
  states often expose only DI constructors. What should generated code do? Options: call a generated or protected copy ctor, require a
  parameterless ctor, call `GetUninitializedObject` (AOT-safe for known types, but it skips field
  initializers, which would break Guid/CancellationTokenSource freshness), or pass the source's injected
  dependencies.
- **Cycles and shared references:** generated code needs a reference map like `CloneContext.Visited`. Should
  it be a pooled `Dictionary<object, object>(ReferenceEqualityComparer)`, or skipped for types proven acyclic
  at compile time (for example records of primitives) as a fast path?
- **Where the generator lives:** the existing `timewarp-state-source-generator` project (already
  `netstandard2.0`, with the Roslyn 4.14.0 floor), or a separate analyzer package so non-state users can
  clone too?
- **Collections and dictionaries:** generate element-wise copies for `List<T>`, `T[]`, multi-dimensional
  arrays, `Dictionary<TKey,TValue>`, `HashSet<T>`, immutable collections (which can be shared by reference),
  and `ObservableCollection<T>`? Preserve comparers (DeepCloner shares comparers by reference)? Note the
  current limitation: hash-based collections are copied field-for-field, so keys that rely on reference
  identity don't survive.
- **Init-only and readonly members:** direct assignment of `init` / `readonly` fields is illegal outside
  ctors. Options are `UnsafeAccessor` (.NET 8+, AOT-friendly) for private/readonly/init fields, a generated
  copy ctor in a partial type, or fallback.
- **Private fields of non-partial types and base classes:** `[UnsafeAccessor]` works for these without
  reflection. Confirm it covers generic base types such as `State<TState>`.
- **Records:** use the compiler's `<Clone>$` / `with` (shallow) plus a deep copy of reference members, or
  treat records like classes?
- **Structs:** copy by value plus deep copy of reference fields (DeepCloner uses `MemberwiseClone`, then fixes
  up the fields).
- **Testing:** parity tests (generated vs `DeepCloner`), the existing WASM E2E clone suite, and a trim/AOT
  smoke test. Also decide whether the generated path is on by default or behind an option during preview.

## Session

- Created: 956920 (2026-10-09)
