# Round 1 — merged findings
**Date:** 2026-10-10
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 11 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1485
- Description: `EmitAccessor` emits a non-generic `[UnsafeAccessor(Field)] static extern ref <closed field type> F(<closed generic target>)`. The runtime matches the field by its declared signature (`T`, `List<TState>`, `T2`), not the closed type, so any field whose type involves a type parameter throws `MissingFieldException` on the first clone. No build diagnostic is reported. I confirmed each of these at runtime:
  - `[GenerateClone] class UsesWrapper { Wrapper<Item> W }` with `Wrapper<T> { T? Value {get;set;} }` throws: Field not found: 'Wrapper`1.<Value>k__BackingField'.
  - A state `HistState : ListState<HistState>`, where the base has `List<TState> History`, throws from `StateCloneRegistry.Clone`, so every action on that state fails in `StateTransactionBehavior`.
  - A `(string, Item)` tuple member throws: 'System.ValueTuple`2.Item2'.
  - `List<KeyValuePair<string, Item>>` throws: 'KeyValuePair`2.value'.
  - `Box?`, where `Box` is a struct with a `List<int>` field, throws: 'System.Nullable`1.value'.

  Paged or result wrappers like `PagedResult<T>` inside states are a very common shape.
- Suggestion: For fields declared on a generic type, emit the accessor inside a generic holder class whose type parameters match the declaring type's definition, and use `field.OriginalDefinition.Type` for the signature (the .NET 9+ generic UnsafeAccessor rules). Then call it as `Holder<Closed>.F(target)`. Special-case `Nullable<T>` (`HasValue` / `Value` / `new T?(...)`), `KeyValuePair` (`new(key, value)`) and `ValueTuple` (public fields, assign directly) so no accessor is needed. Add generator plus runtime tests for each of these shapes.
- Source: general
- Disposition notes: Accessors moved into one `file static class` holder per declaring type definition. A generic declaring type gets a generic holder (type parameters and constraints mirror the definition, flattened across containing types) and the signature uses the definition's field type (`T`, `List<TState>`), per the .NET 9+ generic UnsafeAccessor rules; consumers target net11.0 only, so no net8 path. `Nullable<T>`, `KeyValuePair` and `ValueTuple` are special-cased through public members. Generator shape tests (GenericWrapper, GenericStateBase, NestedGeneric, Tuple, KeyValuePair, NullableStruct) and runtime tests (including `HistState` through `StateCloneRegistry`) cover it; probes p1/p4/p5 now pass.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:623
- Description: `AppendEnumerableInterfaceCases` always adds `T[]`, `List<T>`, `HashSet<T>`, `ObservableCollection<T>` and `Collection<T>` cases, whatever the target interface is. It returns each clone as the interface type, so the generated code does not compile for most interfaces. I checked each one:
  - `IList<string>` and `IReadOnlyList<string>` fail with CS0266, because a `HashSet` is not an `IList` or `IReadOnlyList`.
  - `ISet<int>` fails with CS8121 and CS0029 (an `int[]` pattern and `List`/`Collection` returns).
  - `IEnumerable<T>` fails separately: `TryElementType` (line 1169) only searches `type.AllInterfaces`, which does not include the interface itself, so the member gets TWSG002 "the collection element type could not be resolved".

  Only `ICollection<T>`, `IReadOnlyCollection<T>`, `IDictionary` and `IReadOnlyDictionary` compile, and those are the only ones the test-app fixtures use. `IReadOnlyList<T>` and `IEnumerable<T>` are probably the most common collection member types in states. Design, cloning.md:24 and the Results table all say interface collections are supported.
- Suggestion: Add a case only when the concrete type is assignable to the interface, using `Compilation.ClassifyConversion` or `HasImplicitConversion`. In `TryElementType`, check `type.OriginalDefinition` itself when `type` is an interface. Add generator compile tests for every interface listed in `MatchInterface`.
- Source: general
- Disposition notes: Every interface case is added only when `Compilation.HasImplicitConversion(case, interface)`; `TryElementType` / `TryDictionaryArguments` check the interface itself. Shape test `CollectionInterfaces` compiles all nine interfaces from `MatchInterface`; probe p3 compiles.

### M3 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:772
- Description: An interface-typed member whose runtime value is not one of the five hard-coded collections, a concrete type in the compilation, or `ICloneable` falls into `default: throw InvalidOperationException("... Implement ICloneable on that type.")`. I confirmed these at runtime:
  - `IReadOnlyCollection<Item>` holding `ImmutableList.Create(...)` throws.
  - `IReadOnlyDictionary<string,int>` holding `ImmutableDictionary` throws, even though all of its elements are shared.

  `ReadOnlyCollection` from `.AsReadOnly()`, `FrozenDictionary`, `SortedDictionary` and LINQ iterators reach the same path. Design and Results say "A field typed `IEnumerable<T>` that holds a lazy iterator is materialized", but no such case is emitted. The old `DeepCloner` cloned all of these by runtime type. The failure surfaces as an exception thrown by `StateTransactionBehavior`, and the message tells the user to implement `ICloneable` on a BCL type, which they cannot do.
- Suggestion:
  - Add a case for immutable and frozen types: share them when the elements are shared, otherwise rebuild.
  - Add a final fallback for enumerable and dictionary interfaces that materializes into the generator's default concrete type (`List<T>` or `Dictionary<,>`) when that type is assignable to the interface.
  - Fix the Design and Results text to match what is actually emitted.
- Source: general
- Disposition notes: Interface dispatch now also switches on sorted, linked, read-only, immutable (incl. sorted) and frozen BCL collections (non-sealed ones by exact runtime type), then source implementations, then `ICloneable`, and finally materializes into `List<T>` / `HashSet<T>` (set interfaces) / `Dictionary<,>` when assignable, so BCL values never throw. Frozen collections of mutable elements are rebuilt. Design/Results and cloning.md describe what is emitted. Runtime test `Clone_Bcl_Values_Behind_Collection_Interfaces`.

### M4 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:779
- Description: `ClassifyObject` treats any non-collection class from a referenced assembly as cloneable when it has a public constructor. It copies only the fields Roslyn exposes, and for metadata types that excludes private and internal fields. These types come back silently empty, with no TWSG002:
  - A `SortedDictionary<string,int>` with 2 entries clones to 0 entries.
  - A `StringBuilder` containing "hello" clones to "".

  `LinkedList<T>`, `SortedSet<T>`, `ConcurrentDictionary<,>`, `BitArray` and third-party classes with private state behave the same way. This contradicts the requirement that anything the generator cannot clone is a build error, and it contradicts cloning.md:32 and the migration guide, which say "a type in another assembly whose shape is not an auto-property, a known collection, or ICloneable" produces TWSG002.
- Suggestion: For metadata (non-source) classes that are not in the shared or known-collection lists, report TWSG002 rather than guess from visible members. The one exception is a type whose only instance state is public or protected auto-properties, which needs a positive check. Alternatively, extend the known-collection list with `SortedDictionary`, `SortedList`, `SortedSet` and `LinkedList`.
- Source: general
- Disposition notes: Metadata types (member types and bases) are cloned field by field only when all state is provably visible: TimeWarp.State's own assemblies; implementation assemblies re-imported with `MetadataImportOptions.All` (every private field must back a visible auto-property, be an event backing field, or be ignored); reference assemblies only for auto-property/record-only types. Otherwise TWSG002 (message suggests `ProduceReferenceAssembly=false`). Known collections added: `SortedDictionary`, `SortedList`, `SortedSet`, `LinkedList`, `ConcurrentDictionary`, `ReadOnlyCollection`, `ReadOnlyDictionary`, immutable sorted, plus `StringBuilder`. test-app-contracts sets `ProduceReferenceAssembly=false` for its DTO. Shape tests `OtherBclCollections`, `BclClassWithPrivateState`, and the `Should_Check_Metadata_Types` cases.

### M5 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:255
- Description: A member whose static type is a concrete, non-sealed class is cloned as exactly that type. `ClassifyObject` emits `new Base()` and copies only the base fields, so derived instances are sliced. In the probe, `Shape S` holding a `Circle { Radius = 5 }` cloned as `Shape`, losing its type and `Radius`. The old `DeepCloner` cloned by runtime type, and its `IsCopyDirect` note says "any other declared type can hold a mutable subtype", so this is a silent semantic regression for polymorphic members such as `List<Shape>` or a `Notification` base class.
- Suggestion: For non-sealed classes, emit a dispatch the same way `ClassifyInterface` does: a switch over the derived types in the compilation, deepest first, falling back to the base cloner only when `source.GetType() == typeof(Base)`, and throwing otherwise. Report TWSG002 when unseen subtypes are possible from other assemblies, or document that limitation.
- Source: general
- Disposition notes: Non-sealed classes dispatch on runtime type: derived source types (deepest first), the `_Exact` cloner when `GetType()` matches, then `ICloneable`, else `InvalidOperationException` naming the unseen type (a subclass from another assembly; documented). Known BCL collections fall back to the base collection. A derived source type that cannot be cloned fails the build. Shape tests `Polymorphic` / `UncloneableDerivedType`, runtime test `Keep_Runtime_Type_Of_Polymorphic_Members`.

### M6 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:910
- Description: `TryEmitConstruction` emits untyped `new(default!, default)`, which fails in two ways:
  1. **Compile error:** when the chosen constructor has an overload of the same arity, the call is ambiguous. `Money(decimal)` plus `Money(string)` produces CS0121 in the generated file.
  2. **Runtime throw:** constructors that validate their arguments throw on every clone. `ReadOnlyCollection<int>` throws `ArgumentNullException ('list')`, confirmed. A DI state constructor with `ArgumentNullException.ThrowIfNull(service)` would throw on every action. The old `DeepCloner` caught this case through `InvokeOrUninitialized`.
- Suggestion: Emit typed arguments such as `new((global::System.Decimal)default)`. For constructor-throws safety, consider `[UnsafeAccessor(UnsafeAccessorKind.Constructor)]` on a parameterless or private constructor, or `RuntimeHelpers.GetUninitializedObject(typeof(T))`, which is AOT-safe for a statically known `T`, followed by explicit re-initialization of the ignored members. At a minimum, document that state constructors must tolerate `default` arguments, and add a test.
- Source: general
- Disposition notes: Arguments are typed: declared default value cast to the parameter type, else `default(T)`. Parameterless constructors of any accessibility and inaccessible constructors go through `[UnsafeAccessor(UnsafeAccessorKind.Constructor)]`. cloning.md and the migration guide document that constructors must tolerate default arguments and that stored services should be ignored. Shape tests `AmbiguousConstructor`, `PrivateConstructor`, `DependencyConstructorState`; runtime tests `Construct_With_Typed_Default_Arguments` and `Clone_State_Whose_Constructor_Takes_A_Service`.

### M7 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:947
- Description: `RequiredInitializer` only scans `type.GetMembers()`. It misses `required` members declared on base types and `required` fields. A `[GenerateClone] class RequiredHolder : RequiredBase {}`, where `RequiredBase` has `public required string Name { get; set; }`, makes the generated code fail with CS9035. The same applies to `SetsRequiredMembers` checks across the hierarchy.
- Suggestion: Walk `EnumerateHierarchy(type, null)` and include `IFieldSymbol.IsRequired`. Initialize them with `default!` as is done today, since the accessor copy overwrites them anyway.
- Source: general
- Disposition notes: `RequiredInitializer` walks the whole hierarchy and includes required fields (deduplicated by name); an unsettable required member falls back to an UnsafeAccessor constructor. Shape test `RequiredMembers`, runtime test with a required base property and required field.

### M8 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:599
- Description: An `ImmutableStack<T>` of non-shared elements is rebuilt with `ImmutableStack.CreateRange(source.Select(...))`. Enumeration is top to bottom and `CreateRange` pushes in order, so the clone is reversed. Confirmed: original top=2, clone top=1. The mutable `Stack<T>` path handles this correctly.
- Suggestion: Reverse the sequence before `CreateRange`, for example by materializing into an array and iterating backwards. Add a test.
- Source: general
- Disposition notes: `ImmutableStack` copies into a buffer and pushes in reverse. Runtime test `Keep_ImmutableStack_Order`.

### M9 — Severity: bug — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:596
- Description: The immutable rebuild paths call the `source.Select(...)` extension, but the generated file has no `using System.Linq`. Consumers with `<ImplicitUsings>disable</ImplicitUsings>` get CS1061 ("'ImmutableList<Item>' does not contain a definition for 'Select'"), confirmed in probe p2. Every other emitted name is `global::`-qualified.
- Suggestion: Emit `global::System.Linq.Enumerable.Select(source, item => ...)`, or a plain loop into a builder, which also avoids the allocation of the lambda capturing `map`.
- Source: general
- Disposition notes: Immutable and frozen rebuilds use builders and loops; generated code has no `System.Linq`. Shape test asserts no `.Select(`; probe p2 (ImplicitUsings disabled) compiles.

### M10 — Severity: bug — Status: fixed
- File: source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs:78
- Description: Requirements say any other reflection in TimeWarp.State must be "listed in Results with its trim/AOT warnings", and the goal is zero IL2xxx/IL3xxx. Building TimeWarp.State with `-p:IsAotCompatible=true` still reports:
  - IL2026 and IL2072 for `EnsureStates` (`assembly.GetTypes()` plus `TryAddTransient(Type)`)
  - IL2026 and IL3050 at store.redux-dev-tools.cs:56 (`JsonSerializer.Deserialize<TValue>(string, options)`, outside the suppressed `LoadStatesFromJson`)

  `GetInterfaces()` in service-collection-extensions.log-timewarp-state-middleware.cs:40 is also reflection. None of these appear in Results or the release notes, and the trimmed console smoke test passed only because that code was unreachable. No check was wired in: `IsAotCompatible` / `EnableTrimAnalyzer` is not set anywhere, although the checklist item "Wire it in as a check if practical" is ticked.
- Suggestion: List these sites in Results with their IL ids and file a follow-up task. Set `<IsAotCompatible>true</IsAotCompatible>` on timewarp-state.csproj, with targeted suppressions or `[RequiresUnreferencedCode]` on the remaining sites, so regressions fail the build.
- Source: general
- Disposition notes: `timewarp-state.csproj` sets `IsAotCompatible=true` and lists IL2xxx/IL3xxx ids in `WarningsAsErrors` (global TreatWarningsAsErrors is false). `EnsureStates` (IL2026, IL2072) and `LoadStatesFromJson` (IL2026, IL3050) carry `UnconditionalSuppressMessage` with justifications; `GetInterfaces` in middleware logging reports nothing. All sites are listed in task.md Results and release notes. Follow-up not filed with `ganda kanban create` because it claims and creates a worktree; it is listed in Results instead.

### M11 — Severity: bug — Status: fixed
- File: tests/timewarp-state-source-generator-tests/state-clone-source-generator-tests.cs:12
- Description: The checklist requires "generator/diagnostic tests for unsupported shapes" and parity coverage. The generator tests cover only four cases: `object` → TWSG002, `ICloneable` skipped, a simple state, and a metadata base auto-property. The runtime fixtures do not cover:
  - generic member types
  - collection interfaces other than `ICollection` and `IDictionary`
  - immutable or frozen collections
  - records, `required`, or `init`
  - structs inside `Nullable`
  - tuples
  - polymorphic members
  - pointer or ref-struct TWSG002
  - open generic states
  - private nested states

  Every bug in Issues 1–9 would have been caught by a compile-and-run test of that shape.
- Suggestion: Add a table-driven generator test that compiles each shape and asserts no CS errors, plus TWSG002 for the unsupported shapes. Add runtime clone tests in timewarp-state-tests for the shapes in Issues 1, 3, 5 and 8.
- Source: general
- Disposition notes: Added `state-clone-shape-tests.cs` (table-driven: 17 supported shapes compile clean, 7 unsupported shapes report TWSG002 at a source location, single located report naming the root, metadata implementation/reference assembly checks) and `generated-clone-shape-tests.cs` runtime tests for M1, M3, M4, M5, M6, M7, M8 shapes.

### M12 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-source-generator.cs:35
- Description: The pipeline combines `CompilationProvider` with a syntax provider whose values are only checked for `IsDefault`. The type names are never used, and computing them costs a `GetDeclaredSymbol` call for every type declaration. As a result, the whole planner reruns on every compilation change, and so does `CollectClosedGenerateCloneTypes`, which calls `GetSemanticModel` and `GetSymbolInfo` on every `GenericNameSyntax` in every tree. In the IDE that means every keystroke, so the incremental caching does nothing here. The Design comment ("the syntax provider only contributes equatable identity") suggests caching was intended.
- Suggestion: Use `ForAttributeWithMetadataName` for `[GenerateClone]`, plus a syntax predicate on base lists for `State<`. Project each root into an equatable model (strings or records) and plan per root. At minimum, drop the unused syntax provider, so the cost is honest, and scope the closed-generic scan to `[GenerateClone]` generic definitions.
- Source: general
- Disposition notes: Dropped the unused syntax provider; the generator takes `CompilationProvider` only (whole-compilation planning, documented in its Design). The closed-generic scan binds only `GenericNameSyntax` whose identifier matches a `[GenerateClone]` generic definition. A per-root equatable pipeline was not attempted (optional).

### M13 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1563
- Description: `Report` emits one TWSG002 for every reachable error slot, including BCL and metadata types. The location is `Location.None` or a metadata location, so the build shows `CSC : error TWSG002`. The message format then says "Implement ICloneable on 'IEnumerable'", which tells the user to implement `ICloneable` on a BCL interface, and the type is shown by `Name` only, without its type arguments. The same failure is also reported a second time, correctly, at the member, so users see a duplicate, misleading error with no file.
- Suggestion: Report only primary failures that have a source location, which means the member-level ones. Use `ToDisplayString()` for the type in the message, and name the root state as the requirement asks.
- Source: general
- Disposition notes: TWSG002 is reported only for source-located primary failures: member-level failures whose cause chain has no deeper source member failure, or a root's own failure; a failed root with nothing reported gets one diagnostic at its declaration. The type is shown with `ToDisplayString()`, and the message names the root (`reached from 'X'`). Test `Given_Nested_Failure_Reports_Once_At_Member_And_Names_Root`.

### M14 — Severity: nit — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs:172
- Description: Method and accessor names come from `Sanitize(type.ToDisplayString())`, which maps every non-alphanumeric character to `_`. Different types can therefore collide: `N.A.B` (nested) and `N.A_B` both become `Clone_N_A_B`. When both inherit a field from the same base, both emit `Clone_N_A_B_F0(Common target)`, which fails with CS0111. Confirmed in probe p5. It is unlikely in practice but cheap to make impossible.
- Suggestion: Append a per-slot sequence number, or a short stable hash of the fully qualified name, to `MethodName`.
- Source: general
- Disposition notes: Method and holder names end in an FNV-1a hash of the fully qualified name, with a counter fallback on collision; accessor names hash the field name. Shape test `NameCollision` (`N.A.B` vs `N.A_B` over a shared base) compiles.

## Duplicates / conflicts

- Single reviewer; general Issue N maps to MN.
