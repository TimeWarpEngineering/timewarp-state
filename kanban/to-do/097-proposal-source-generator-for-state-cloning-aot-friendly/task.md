# Source generator for state cloning (AOT-friendly)

## Description

**Decision (Steven, 2026-10-10, by voice): pursue this. Approved for implementation.** The goal is to get rid of
reflection in state cloning wherever possible. The AnyClone problem is already gone (replaced by the in-house
`DeepCloner`, and `RouteState` implements `ICloneable`), so this task is about AOT/trim safety, first-use cost
and throughput, not correctness of a third-party cloner.

Deliverable: a Roslyn **incremental** source generator that emits compile-time clone code for `State<T>` types
and replaces the reflection `DeepCloner` (`source/timewarp-state/features/cloning/deep-cloner.cs`,
`clone-extensions.cs`) entirely.

**Design correction (Steven, 2026-10-10, 2:08 AM): no reflection fallback.** No reflection code may ship in
TimeWarp.State at all; even unused reflection breaks AOT trimming. `DeepCloner` (`deep-cloner.cs`) and the
reflection path in `clone-extensions.cs` are **deleted**, not kept as a fallback. Clone order is: hand-written
`ICloneable` first, else the generated clone. A state type the generator cannot handle is a **build-time error**
telling the consumer to implement `ICloneable` on that type (or fix the unsupported member), so nothing fails at
runtime. This is a breaking change, acceptable in the 12.0 beta; it goes in the release notes with migration
guidance.

Work order: **design first, then implement.** Write the design in `## Results` (answering or explicitly
deferring each open question in Notes) before changing product code, then implement against it.

The original proposal analysis is kept below for context.

### Background (from the proposal)

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

- Deliverable: a Roslyn incremental source generator (`IIncrementalGenerator`) that emits compile-time clone
  code for `State<T>` types. `StateTransactionBehavior` uses it in place of the reflection `DeepCloner`, which is
  deleted along with the reflection path in `clone-extensions.cs`.
- **No reflection in TimeWarp.State (Steven, 2026-10-10).** No reflection code may ship in TimeWarp.State at
  all, used or unused: no `System.Reflection` member access, `Activator.CreateInstance`,
  `GetUninitializedObject`, `MemberwiseClone` via reflection or similar. There is no runtime fallback. This task
  removes the cloning reflection; any other reflection found elsewhere in TimeWarp.State is listed in Results with
  its trim/AOT warnings (and fixed here if small, otherwise filed as a follow-up task).
- Clone order in `StateTransactionBehavior`: hand-written `ICloneable` first, else the generated clone. Nothing
  else.
- **Placement (Steven, 2026-10-10): build it into TimeWarp.State.** Use the existing
  `source/timewarp-state-source-generator` / analyzer projects, or a new generator project that ships inside the
  TimeWarp.State package (`analyzers/dotnet/cs`). Do **not** extract a separate generic cloning library or
  package.
- Design first: write the design in `## Results` before changing product code. It must cover generator
  placement, the emitted code shape, registration/dispatch in `StateTransactionBehavior`, handling of nested,
  collection and foreign (other-assembly) types (which are either cloned by generated code, shared as
  immutable, or produce the build error below), and the test plan (port the `deep-cloner-tests.cs` cases to the
  generated clone; benchmarks optional). Answer each open question in Notes or explicitly defer it. Then implement.
- The design must keep current clone semantics so existing states behave the same:
  - Members marked IgnoreDataMember, NonSerialized or JsonIgnore (matched by name, any namespace, including
    backing fields) are not copied, and keep their constructor values. Each `State` clone therefore keeps a
    fresh `Guid`, and `Sender` / `CancellationTokenSource` are not shared.
  - `ICloneable` wins over everything else and is the supported path for any state type the generator can't
    handle.
  - Shared/immutable types (string, primitives, dates, Guid, Uri, Version, delegates, `MemberInfo`, `Type`,
    comparers, `IServiceProvider`, threading types, …) stay copied by reference, matching today's
    `DeepCloner.IsShared` list (decided at compile time now).
  - Cycles and shared references inside one clone are preserved.
  - Cloning never blocks (no `SemaphoreSlim.Wait`, locks or other waits), so it stays safe on single-threaded
    browser WASM.
- Unsupported types are a build error, not a runtime fallback: a state type (or a member type reachable from
  it) that the generator cannot clone, and that has no hand-written `ICloneable`, produces an **error**
  diagnostic that names the state type and the offending member, and tells the author to implement
  `ICloneable` on the state (or fix the unsupported member). New TW id, documented in
  `AnalyzerReleases.Unshipped.md` and the analyzer readme.
- Reconcile the existing **TWS001** rule (`StateImplementationAnalyzer`: a `State<T>` must implement
  `ICloneable` or have a parameterless ctor) with the new rule. Either fold it into the new diagnostic or
  redefine it so the two never contradict each other (for example, the ctor requirement only applies where the
  generated clone needs it). Update its tests (`state-implementation-analyzer-tests.cs`), readme and release
  tracking.
- Goal: **zero IL2xxx/IL3xxx trim/AOT warnings from TimeWarp.State.** Add a check for that if practical, for
  example an AOT/trim-analysis build (`IsAotCompatible` / `EnableTrimAnalyzer`, or a trimmed publish of a sample
  with `TrimmerSingleWarn=false`) that fails on any such warning from TimeWarp.State.
- Breaking change (12.0 beta): `DeepCloner` and the reflection `CloneExtensions.Clone<T>()` path are removed,
  and unsupported states now fail the build. Record it in the release notes with migration steps
  (implement `ICloneable`, or fix the unsupported member).

## Checklist

Approved for implementation (Steven, 2026-10-10). Design first, then implement.

- [x] Write the design in Results: generator placement, emitted shape, registration/dispatch in
      `StateTransactionBehavior`, nested/collection/foreign types, tests (parity old vs generated), benchmarks
      optional; answer or explicitly defer each open question in Notes
- [x] Where the generator lives: inside TimeWarp.State (existing `source/timewarp-state-source-generator` /
      analyzer projects, or a new generator project shipped in the TimeWarp.State package). No separate
      generic cloning library (Steven, 2026-10-10)
- [x] Generator: for each opted-in state type, emit a clone method (for example a `partial` member or a
      generated `IStateCloner<TState>` registered in a static lookup) plus helpers for the reachable member
      types it can see
- [x] Generated code honors IgnoreDataMember / NonSerialized / JsonIgnore exactly as `DeepCloner.IsIgnored`
      does, including attributes on properties that apply to their backing fields
- [x] Generated code uses a reference map equivalent to `CloneContext.Visited` for cycles and shared references
- [x] Error diagnostic (new TW id, documented in AnalyzerReleases.Unshipped.md and the readme) for any state
      type the generator can't clone and that has no `ICloneable`, naming the type and member and telling the
      author to implement `ICloneable`
- [x] Reconcile TWS001 with the new diagnostic; update its tests and docs
- [x] `StateTransactionBehavior` dispatch: `ICloneable`, else generated clone; nothing else
- [x] Delete `DeepCloner` (`deep-cloner.cs`) and the reflection path in `clone-extensions.cs`; no reflection
      left in TimeWarp.State
- [x] Port the `deep-cloner-tests.cs` cases (private fields, ignored members, nested collections, cycles,
      multi-dimensional arrays, structs, shared delegates and types, null) to the generated clone, plus every
      test-app state; add generator/diagnostic tests for unsupported shapes
- [x] Keep the WASM E2E clone suite green (`CloneTestPageTests.CloneSuitePassesInServerAndWasm`,
      `CounterTests`)
- [x] Trim/AOT smoke test: publish a sample WASM app with trimming (and Native AOT for a console host if
      practical) and `TrimmerSingleWarn=false`; there must be zero IL2xxx/IL3xxx warnings from TimeWarp.State,
      and clones must be correct at runtime. Wire it in as a check if practical
- [x] Optional: benchmark first clone and steady-state clone (generated vs the old reflection cloner, measured
      before it is deleted) and record the numbers
- [x] Docs: update the cloning topic and claude.md; release notes with the breaking change and migration steps

## Notes

### 2026-10-10: walk interrupted for a design correction

- The first `ganda task work 097 --yes` run (PID 2503312) was stopped at about 2:08 AM (UTC+7) during the
  `implement` step (implementer-grok had just started; claim and folderize had completed). No product code had
  been changed, so nothing was discarded.
- Reason: Steven's design correction. Remove the reflection fallback entirely (delete `DeepCloner`); unsupported
  state types become a build-time error telling the author to implement `ICloneable`; reconcile TWS001; zero
  IL2xxx/IL3xxx warnings from TimeWarp.State. Requirements and Checklist above are updated.
- Old log: `/home/steve/logs/task-work-state-097-20261010-020553.log`. The walk was re-run with
  `ganda task work 097 --yes`.

### Proposed design (original proposal; superseded where it mentions a DeepCloner fallback)

1. **Emission.** For each state type (and each reachable member type the generator can fully see in the
   current compilation), emit a strongly typed clone, `TState CloneGenerated(TState source, CloneMap map)`.
   It allocates the target and assigns each copied member directly. Reference-type members recurse into
   their generated clone (or hand-written `ICloneable`); a member type with neither is a build error.
2. **Opt-out attributes.** Use the same name-based IgnoreDataMember / NonSerialized / JsonIgnore checks. The
   generator resolves them from symbols (`IFieldSymbol` / `IPropertySymbol` and the property's backing field).
   Ignored members are left as the constructor set them, so `State.Guid` stays unique and `Sender` is not
   shared.
3. **ICloneable escape hatch** stays first. A type that implements `ICloneable` gets no generated clone, or
   the generated one is bypassed.
4. **No fallback (corrected 2026-10-10).** Types that are external (metadata only) and not known-immutable,
   open generic, inaccessible from generated code, or unsupported shapes produce a build error naming the
   state and member, telling the author to implement `ICloneable` (or make the type partial/accessible).
5. **Dispatch** in `StateTransactionBehavior`: `ICloneable`, else generated clone (looked up through a
   generated registry or a static abstract / partial member on the state). No reflection path.

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
- **Where the generator lives:** *Decided (Steven, 2026-10-10):* inside TimeWarp.State, in the existing
  `timewarp-state-source-generator` project (already `netstandard2.0`, with the Roslyn 4.14.0 floor) or a new
  generator project shipped in the TimeWarp.State package. Not a separate generic cloning library.
- **Collections and dictionaries:** generate element-wise copies for `List<T>`, `T[]`, multi-dimensional
  arrays, `Dictionary<TKey,TValue>`, `HashSet<T>`, immutable collections (which can be shared by reference),
  and `ObservableCollection<T>`? Preserve comparers (DeepCloner shares comparers by reference)? Note the
  current limitation: hash-based collections are copied field-for-field, so keys that rely on reference
  identity don't survive.
- **Init-only and readonly members:** direct assignment of `init` / `readonly` fields is illegal outside
  ctors. Options are `UnsafeAccessor` (.NET 8+, AOT-friendly) for private/readonly/init fields, a generated
  copy ctor in a partial type, or the build error (implement `ICloneable`).
- **Private fields of non-partial types and base classes:** `[UnsafeAccessor]` works for these without
  reflection. Confirm it covers generic base types such as `State<TState>`.
- **Records:** use the compiler's `<Clone>$` / `with` (shallow) plus a deep copy of reference members, or
  treat records like classes?
- **Structs:** copy by value plus deep copy of reference fields (DeepCloner uses `MemberwiseClone`, then fixes
  up the fields).
- **Testing:** the ported `deep-cloner-tests.cs` cases against the generated clone, the existing WASM E2E clone
  suite, and a trim/AOT warning check. The generated path is the only path (no option to switch back to
  reflection).

### Design considerations / open questions

- **Recursion risk (dev-tools/inspector extension).** A proposed dev-tools or inspector extension might use
  TimeWarp.State to manage its own UI state. If it does, the extension's own actions could be captured into the
  same action log it displays. Each displayed update would then produce a new captured action, which is an
  infinite loop. This is a design constraint to solve before building such an extension. Options include
  filtering the extension's own actions out of capture, or using a separate store inside the extension that
  is not part of the app's pipeline.
- **Separate store for the extension.** Give the extension its own entirely separate store, decoupled from
  the app's state, so its UI state never enters the logged stream or the app's transaction/clone pipeline.
- **Relation to this cloning proposal.** The extension's state must not be cloned or logged by the app's
  `StateTransactionBehavior`. The generated clone path should never run for extension state, and any generator opt-in rule, such as "every `IState`", must not pull extension
  states into the app's clone registry.

## Results

Design recorded before product changes. Implementation follows this section.

### Design

**Placement.** The generator is `StateCloneSourceGenerator` (`IIncrementalGenerator`) in the existing
`source/timewarp-state-source-generator` project (`netstandard2.0`, Roslyn 4.14). It ships inside
`TimeWarp.State` at `analyzers/dotnet/cs`. No separate cloning package.

**Opt-in.** Implicit for every concrete, closed, accessible `State<TState>` in the compilation that does not
implement `ICloneable`. States do not need to be `partial`: the generator emits an external cloner, not a
partial member. `[GenerateClone]` (`TimeWarp.State.GenerateCloneAttribute`) is the explicit opt-in for a
non-state class or struct that still needs a `Clone()` extension (the test-app clone suite and the ported
deep-clone fixtures). Open generic states are a build error. `ICloneable` types are skipped: the hand-written
clone stays the only clone.

**Emitted shape.** One `TimeWarpStateClones.g.cs` per compilation:

- `TimeWarp.Features.Cloning.GeneratedCloneExtensions.Clone(this T?)` for each root, so existing
  `using TimeWarp.Features.Cloning` call sites keep compiling.
- An internal cloner per reachable class, struct, array, and supported collection. Reference types take a
  `CloneMap`. The cloner calls a constructor so field initializers run: the parameterless one at any
  accessibility (private/protected through `[UnsafeAccessor(UnsafeAccessorKind.Constructor)]`), else the
  accessible one with the fewest parameters, else any constructor through `[UnsafeAccessor]`. Arguments are the
  declared default or a typed `default(T)`, so same-arity overloads stay unambiguous. `required` members across
  the hierarchy (properties and fields) get `default!` in an object initializer. `Guid` and
  `CancellationTokenSource` therefore stay fresh. There is no `GetUninitializedObject`. A constructor that throws
  on default arguments throws on every clone; such a state needs `ICloneable` (documented).
- Instance fields, including auto-property backing fields, `init` / `readonly` / private fields, and fields
  declared on generic bases such as `State<TState>`, are read and written with
  `[UnsafeAccessor(UnsafeAccessorKind.Field)]`. Accessors live in one `file static class` per declaring type
  definition. A generic declaring type gets a generic holder whose type parameters (and constraints) mirror the
  definition, and the field signature uses the definition's field type (`T`, `List<TState>`): the .NET 9+
  generic UnsafeAccessor rule. All consumers target net11.0, so no net8 path is needed. `Nullable<T>`,
  `KeyValuePair`, and `ValueTuple` are rebuilt through public members. Ignored fields are left at the
  constructor values. Method and holder names end in an FNV-1a hash of the fully qualified name, so they never
  collide.
- A `[ModuleInitializer]` registers each state root with `StateCloneRegistry` (lock-free
  `ImmutableInterlocked` dictionary of `Type` → clone delegate). No assembly scan.

**Ignore rules.** A field is skipped when it, or its associated property or event, carries an attribute whose
type name is `IgnoreDataMemberAttribute`, `NonSerializedAttribute`, or `JsonIgnoreAttribute` (any namespace).
That matches `DeepCloner.IsIgnored`, including `[IgnoreDataMember]` / `[JsonIgnore]` on `State<T>` `Guid`,
`Sender`, and `CancellationTokenSource`.

**Shared values.** Decided from the static type, matching `DeepCloner.IsShared`: primitives, enums, string,
decimal, dates, `Guid`, `Uri`, `Version`, delegates, `MemberInfo` / `Type` / `Assembly`, `CultureInfo`,
`Regex`, `Encoding`, `IServiceProvider`, comparers, and reference types in `System.Threading` or
`System.Threading.Tasks`. A struct whose fields are all shared is assigned by value. `object` is not shared:
a field of type `object` is unsupported.

**Cycles.** Every clone allocates a `CloneMap` (`Dictionary<object, object>` with `ReferenceEqualityComparer`).
The new instance is inserted before members are copied. Acyclic elision is deferred: the map is always used.

**Collections.** Element-wise public construction, not BCL private-field copies:

- arrays of any rank (zero lower bound via `new T[...]`; non-zero lower bounds are not preserved)
- `List<T>`, `Dictionary<TKey,TValue>` (comparer shared), `HashSet<T>` (comparer shared), `Stack<T>`,
  `Queue<T>`, `Collection<T>`, `ObservableCollection<T>`, and subclasses (subclass fields copied, then items
  added)
- `SortedSet<T>`, `LinkedList<T>`, `SortedDictionary<,>`, `SortedList<,>`, `ConcurrentDictionary<,>`, and
  `ReadOnlyCollection<T>` / `ReadOnlyDictionary<,>` (rebuilt around a new `List<T>` / `Dictionary<,>`);
  `StringBuilder` is copied by its text
- immutable and frozen collections (including sorted ones) are shared when their elements are shared;
  otherwise rebuilt with builders or loops (no `System.Linq` in generated code), in order; `ImmutableStack`
  keeps its top on top
- interface collections (`IEnumerable<T>`, `ICollection<T>`, `IList<T>`, `IReadOnlyCollection<T>`,
  `IReadOnlyList<T>`, `ISet<T>`, `IReadOnlySet<T>`, `IDictionary<,>`, `IReadOnlyDictionary<,>`) switch on
  arrays and every BCL collection above that is assignable to the interface (non-sealed ones by exact runtime
  type), then implementations in the compilation, then `ICloneable`, and otherwise materialize into `List<T>`,
  `HashSet<T>` (set interfaces) or `Dictionary<,>`. A lazy iterator, a collection-expression type, or a
  collection from another library therefore clones into one of those types instead of throwing.

`ObservableCollection<T>` does not copy `CollectionChanged` subscribers. Hash collections are rebuilt, so
keys that use reference identity still do not survive, same as the field-copy limitation for value equality.

**Other member types.** Records are classes or structs (field copy, not `with`). A struct with no reference
fields (`IsUnmanagedType`) is copied by value. A member typed as a non-sealed class dispatches on the runtime
type: derived types in the compilation (deepest first), the exact cloner when `GetType()` matches, then
`ICloneable`, else `InvalidOperationException` (a subtype from another assembly the generator never saw; a
subclass of a BCL collection falls back to the base collection). A non-collection interface or abstract type
switches over implementations in the compilation plus `ICloneable`. A derived type or implementation in the
compilation that cannot be cloned fails the build. A private or protected type, a pointer, an open generic, an
`object` field, or an interface with no implementation is **TWSG002** (error). TWSG002 is reported once, at the
source-located member that reaches the failure (or at a root state's declaration for a root-level failure),
with the type's display name and the root state (`reached from 'X'`). Nothing falls back at runtime. A missing
registry entry throws `InvalidOperationException` from `StateCloneRegistry.Clone`.

**Dispatch.** `StateTransactionBehavior`: `ICloneable`, else `StateCloneRegistry.Clone`. `DeepCloner`,
`CloneExtensions`, and `CloneErrorHandler` are deleted. Clone failures propagate; they are not logged and
swallowed.

**TWS001.** Redefined so it does not contradict the generator. Concrete (non-abstract) types that derive
directly from `State<T>` must implement `ICloneable` or have an accessible constructor (public, or internal
in the same assembly). A parameterless constructor is no longer required. Abstract intermediates such as
`TimeWarpCacheableState<TState>` are exempt. Unsupported members stay **TWSG002** only.

**Inspector / extension store.** Deferred. The generator registers only states declared in that compilation.
It does not reflect over other assemblies, so an extension is not pulled in by an "every `IState`" scan.
A loaded extension assembly registers its own states; `StateTransactionBehavior` clones a state only when an
action nested in that state is dispatched. A separate store for a future inspector is unchanged and out of
scope.

**Tests.** Generator diagnostics for an unsupported member and for `ICloneable` skipping, plus a table-driven
shape suite (`state-clone-shape-tests.cs`): 17 supported shapes compile with no generator diagnostic and no
compiler error (generic declaring types, nested generics, tuples, `KeyValuePair`, `Nullable` structs, every
collection interface, immutable/frozen, sorted/linked/concurrent/read-only collections, `StringBuilder`,
collection subclasses, polymorphism, same-arity overloads, private constructors, `required` across the
hierarchy, records, colliding names, DI-constructor state), 7 unsupported shapes report TWSG002 at a source
location, nested failures report once with the root named, and metadata types are checked for implementation
and reference assemblies. Ported `deep-cloner-tests` cases and `generated-clone-shape-tests.cs` execute the
generated `Clone()` (`[GenerateClone]` fixtures and registered states). The test-app clone
suite keeps calling `Clone()` on `[GenerateClone]` objects. WASM E2E stays `CloneTestPageTests` and
`CounterTests`. Trim check: `EnableTrimAnalyzer` / `IsAotCompatible` on `TimeWarp.State` must not report
IL2xxx/IL3xxx from the clone path. Pre-existing Redux DevTools time-travel reflection
(`Store.LoadStatesFromJson`: `GetAssemblies` / `GetTypes` / `GetMethod` / `Invoke`) is not removed here;
it is annotated, suppressed at that method, and listed as a follow-up. `MethodInfoExtensions.InvokeAsync`
is unused reflection and is deleted.

**Benchmarks.** Reflection cloner, measured before deletion (100000 steady iterations, one graph of nested
state): first clone 14457.4 µs, steady 4.730 µs per clone (473.0 ms total). A generated-path microbenchmark
was not repeated after the reflection cloner was removed.

**Types from other assemblies (bases and members).** Roslyn does not return private fields of classes in
referenced assemblies, so a field copy could silently drop state. A metadata type (a member type, or a base in
a source type's hierarchy) is cloned field by field only when the generator proves it sees all state:
TimeWarp.State's own assemblies (`State<T>`, `TimeWarpCacheableState<T>`; their private fields are ignored);
an implementation assembly, re-imported with `MetadataImportOptions.All`, where every private field is the
backing field of a visible auto-property, an event backing field, or ignored; or a reference assembly
(`[ReferenceAssembly]`: a `ProjectReference` or a framework reference pack) whose type has only
auto-properties and compiler-generated methods (records). Anything else is TWSG002, whose message suggests
`ProduceReferenceAssembly=false` on the defining project. `test-app-contracts` sets that because its
`WeatherForecastDto` has a computed `TemperatureF`. Public and protected auto-properties on accepted metadata
types are copied through `<Property>k__BackingField`.

**Remaining reflection in TimeWarp.State (trim/AOT).** `timewarp-state.csproj` now sets `IsAotCompatible=true`
and lists the IL2xxx/IL3xxx ids in `WarningsAsErrors`, so a new trim/AOT warning fails the build. The remaining
sites are suppressed in place with justifications:

| Site | Reflection | Warnings |
|------|------------|----------|
| `service-collection-extensions.add-timewarp-state.cs` `EnsureStates` | `Assembly.GetTypes()`, `TryAddTransient(Type)` | IL2026, IL2072 (suppressed) |
| `store.redux-dev-tools.cs` `LoadStatesFromJson` | `JsonSerializer.Deserialize<Dictionary<string, object>>` | IL2026, IL3050 (suppressed) |
| `store.redux-dev-tools.cs` `LoadStateFromJson` | `GetAssemblies` / `GetTypes` / `GetMethod` / `Invoke` | IL2026, IL2070, IL2072, IL2075, IL3050 (suppressed) |
| `service-collection-extensions.log-timewarp-state-middleware.cs` `GetComponentOrder` | `Type.GetInterfaces()` | none reported |

Follow-up (not filed here: `ganda kanban create` claims and creates a worktree): "Remove remaining reflection
from TimeWarp.State (EnsureStates, redux devtools, middleware logging)" — generated state registration,
generated DevTools hydration, and a non-reflection pipeline listing.

### How to validate

Smoke:

- `dotnet test tests/timewarp-state-tests/timewarp-state-tests.csproj --nologo` — 94 passed, 1 skipped (the Fixie skip sample). The ported deep-clone cases passed, including private fields, ignored members, nested collections, cycles, multi-dimensional arrays, structs, shared delegates, and null. Review round 1 added runtime cases for generic declaring types (including a generic state base through `StateCloneRegistry`), nested generics, tuples, `KeyValuePair`, `Nullable` structs, BCL values behind collection interfaces, polymorphic members, `ImmutableStack` order, sorted/linked/`StringBuilder` members, typed default constructor arguments, `required` members, and a DI-constructor state.
- `dotnet test tests/timewarp-state-source-generator-tests/timewarp-state-source-generator-tests.csproj --nologo` — 51 passed, including the table-driven shape suite (supported shapes compile clean; unsupported shapes are TWSG002 at a source location; metadata implementation and reference assemblies).
- `dotnet test tests/timewarp-state-analyzer-tests/timewarp-state-analyzer-tests.csproj --nologo` — 38 passed (TWS001 accepts an accessible constructor and skips abstract states).
- `dotnet test tests/client-integration-tests/client-integration-tests.csproj --nologo` — 65 passed, 1 skipped. `ReturnCachedData_WhenCacheValid` keeps `CacheKey` and `TimeStamp`.
- `dotnet test tests/timewarp-state-plus-tests/timewarp-state-plus-tests.csproj --nologo` — 32 passed, 1 skipped.
- `./bin/dev e2e` — 11 passed, 3 skipped, 0 failed (re-run after review round 1: same result). Playwright's browser install warned (`sudo` needs a terminal); Chromium was already present and the suite ran.
- Build: the build solution filter compiles with 0 errors; its 149 warnings are pre-existing (TW0007, RS0030, BL0010, NU1510, ...) and identical to the pre-fix build. `TimeWarp.State` builds with `IsAotCompatible=true` and no IL2xxx/IL3xxx warnings.
- `ganda repo audit` — passes (one pre-existing advisory: non-kebab wwwroot module names).
- Trim: `dotnet publish` of a console host referencing `TimeWarp.State` and `TimeWarp.State.Blazor`, `-p:PublishTrimmed=true -p:TrimmerSingleWarn=false`. The linker ran and reported no IL2xxx/IL3xxx. The published app printed `clone-ok` (count copied, Guid fresh and not equal). Native AOT of a host referencing `TimeWarp.State` only (`PublishAot=true`) also printed `clone-ok` with no IL warnings. Publishing the analyzer project itself with `PublishAot` fails NETSDK1207 because that project is `netstandard2.0`. `dotnet publish` of `test-app-client` did not link: the `wasm-tools` workload is not installed (`Publishing without optimizations`).

Expect:

- A concrete `State<T>` clones without reflection. `ICloneable` still wins. Unsupported members fail the build as TWSG002.
- Ignored members keep constructor values, so `Guid` is unique and `Sender` / `CancellationTokenSource` are not shared.
- Cacheable weather keeps its timestamp across a second fetch while the cache is fresh, in integration tests and in the browser suite.

### Open questions — answers

| Question | Answer |
|----------|--------|
| Opt-in | Implicit for concrete `State<T>`; external cloner, no `partial`. `[GenerateClone]` for non-states. |
| Constructors | Accessible parameterless, else fewest-parameter accessible ctor with defaults. No uninitialized objects. No ctor → TWSG002 / TWS001. |
| Cycles | Always a per-call `CloneMap`. Acyclic fast path deferred. |
| Where it lives | Decided earlier: `timewarp-state-source-generator`. |
| Collections | Element-wise, comparers shared. See Design. |
| `init` / `readonly` | `[UnsafeAccessor]` field writes. |
| Private and base fields | `[UnsafeAccessor]`, including `State<TState>`. Inaccessible types → TWSG002. |
| Records | Field copy, same as classes or structs. |
| Structs | Value copy, then deep-copy reference fields. |
| Testing | Generated path only. See Tests. |
| Inspector recursion | Deferred. Not part of this generator. |

## Session

- Created: 956920 (2026-10-09)
- Approved for implementation by Steven (2026-10-10, voice); task file updated from proposal to implementation
- Design correction by Steven (2026-10-10, 2:08 AM): no reflection fallback; walk interrupted and re-run
- Implementer: grok task-work (2026-10-10). Design written in Results before product edits.
- Review round 1 (2026-10-10): M1–M14 fixed by the implementer (Claude); see `review/round-1/merged.md`.
