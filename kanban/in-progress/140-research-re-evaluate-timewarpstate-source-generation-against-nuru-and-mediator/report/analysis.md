# Analysis: re-evaluate TimeWarp.State source generation against Nuru and Mediator

Task 140. This is research only and changes no product code. The report stops at the options. Steve decides.

Path abbreviations used below:

- `SG/` is `source/timewarp-state-source-generator/` in this repository.
- `MG/` is `timewarp-mediator/master/source/timewarp-mediator-generators/`.
- `MA/` is `timewarp-mediator/master/source/timewarp-mediator-analyzers/`.
- `NA/` is `timewarp-nuru/master/source/timewarp-nuru-analyzers/`.
- `464/` is `timewarp-architecture/master/kanban/in-progress/464-research-generate-front-end-ux-pieces-from-contracts-counterpart-to-461/`.

The sibling repositories are under `/home/steve/worktrees/github.com/TimeWarpEngineering/`.

## 1. Executive summary

State has four generators, and each one finds its own inputs. None of them shares a model with the others. Two of them are not incremental in practice:

- The ActionSet method generator's model has no value equality, so its output reruns on every edit. The spike measured this.
- The clone generator's only input is `CompilationProvider`. Its own design note says it reruns on every compilation change.

The ActionSet method generator also has correctness gaps, confirmed by the spike:

- A positional `record Action(...)` gets no method.
- A primary-constructor `Action` gets a method that does not compile (CS7036).
- Init-only properties are ignored.
- Any nested `*ActionSet` class in any class produces broken code.

It has no tests of its own.

Nuru and Mediator each build one equatable model and run several emitters from it. Each pins its caching with an incrementality test.

The constraint that limits what State can offer consumers is not how the generators are written. Roslyn generators cannot see each other's output. That is why State today cannot generate an action, a handler, or a JSON context that Mediator or System.Text.Json would then pick up. Task 075 and the architecture proposal (464 §4a) both ran into it.

TimeWarp owns Mediator, so that constraint can be moved, but only by a Mediator change. That is the real fork for Steve: Option 3b below.

## 2. Scope

The analysis covered:

- **State:** every file in `SG/` (5,037 LOC, 31 files), the analyzers in `source/timewarp-state-analyzer/`, the generator tests, and the runtime registries the generated code feeds.
- **Mediator:** the generator, the analyzer it links in, its design docs and its kanban history.
- **Nuru:** the analyzers project, which holds the generator, plus its `.agent/workspace/` notes and kanban history.
- **Architecture task 464:** `proposal.md` and `task.md` as one consumer input. There is no `question.md`; the question is §7 of the proposal.

## 3. Methodology

- **Reading:** I read the State sources directly and cite them below.
- **Delegated inventories:** the Nuru, Mediator and 464 inventories were done by read-only helper agents. I spot-checked their citations, for example `MG/mediator-generator.cs:5-93` and the Nuru LOC count (128 files, 33,412 LOC, recounted).
- **Spike:** I ran the ActionSet method generator against five action shapes plus an unrelated edit, using `CSharpGeneratorDriver` with `trackIncrementalGeneratorSteps: true`. The script is `report/spike/action-set-generator-spike.cs` and its output is `report/spike/action-set-generator-spike.output.txt`. The generator was the Debug build from this worktree (`dotnet build source/timewarp-state-source-generator`).

## 4. Findings

### 4.1 State generator inventory

| Generator | Discovers | How | Emits | Model and caching | Tests |
|---|---|---|---|---|---|
| `ActionSetMethodSourceGenerator` (`SG/action-set-method-generator.cs`) | a class whose name ends in `ActionSet` and whose parent is a class (`:29-35`), containing a nested `ClassDeclarationSyntax` named `Action` (`:46-48`) | `CreateSyntaxProvider`: a name match on syntax, then the semantic model for constructor parameter types (`SG/action-set-constructor-parser.cs:33-55`) | `partial class <Parent> { public async Task <Name>(ctorParams…, CancellationToken? externalCancellationToken = null) }`, which sends `new XActionSet.Action(args)` (`:107-132`) | `ClassModel` is a plain class holding a `List<>` (`:158-179`), so there is no equality. The spike showed an unrelated edit gives output reason **Modified**. No tracking names. | None direct. It is only a co-generator in `tests/timewarp-state-source-generator-tests/action-catalog-generator-test-driver.cs:36-40`. |
| `ActionCatalogSourceGenerator` (`SG/action-catalog-source-generator.cs`) | `[CatalogAction]` on a nested `*ActionSet.Action` (`:49-60`) | `ForAttributeWithMetadataName` (`:29-37`), combined with `CompilationProvider.Select(AssemblyName)` (`:39-46`). Parameters come from the same constructor parser. | `GeneratedActionCatalog.All`, with descriptors, a JSON input schema, and executors that call the generated `State.Method` (`:235-255`). Also an assembly attribute deriving from `ActionCatalogProviderAttribute`, so `AddActionCatalog(assembly)` needs no scan (`:180-200`). | `sealed record CatalogEntryModel` with `EquatableArray` (`:277-288`). Equatable. No tracking names. | `action-catalog-source-generator-tests.cs` |
| `PersistenceStateSourceGenerator` (`SG/persistence-state-source-generator.cs`) | a class with an attribute whose text is `PersistentState` or ends in `.PersistentState` (`:44-52`) | `CreateSyntaxProvider`, a text match only. `[PersistentStateAttribute]` written out in full would not match. | `Load(CancellationToken?)`, which sends the hand-written `LoadPersistentStateRequest` (`:79-110`). TWSG001 is reported for nested states (`:20-30`). | `sealed record ClassModel` with an equatable location (`:143-175`). Equatable. | `persistence-state-source-generator-tests.cs` |
| `StateCloneSourceGenerator` and `StateClonePlanner` (`SG/state-clone-source-generator.cs`, `SG/state-clone-planner*.cs`, about 4,000 LOC) | every `State<T>` and `[GenerateClone]` type in the compilation, their member graphs, and implementations of interfaces they use, including in referenced assemblies (`SG/state-clone-planner.subtypes.cs:335-357`). Implementation assemblies are re-imported with `MetadataImportOptions.All` (`SG/state-clone-planner.metadata.cs:167`). | The **only** input is `CompilationProvider` (`SG/state-clone-source-generator.cs:37-39`). The design note says it "reruns on every compilation change" (`:11`). A full symbol walk (`SG/state-clone-planner.cs:53-77`). | `TimeWarpStateClones.g.cs`, containing clone methods with `UnsafeAccessor`, a `[ModuleInitializer]` that registers clones, trimming roots for ICloneable states, and the DI registration of every state through `StateRegistrationRegistry` (`SG/state-clone-planner.emit.cs:227-293`). TWSG002 is reported. | No model. It emits a string from the compilation each time. | 20 test files (shape, correctness, metadata, guard and more). Fixie and Shouldly, using `CSharpGeneratorDriver`. |

Analyzers (TWS0001-TWS0012) live in a separate `DiagnosticAnalyzer` project (`source/timewarp-state-analyzer/`). Release tracking covers only TWSG001 and TWSG002 (`SG/AnalyzerReleases.Unshipped.md`). SG002, an Info diagnostic per generated file, is created inline and is not tracked (`SG/action-set-method-generator.cs:78-92`).

**What State's generators cannot see:**

- **Each other's output.** The catalog's executors call `State.Method`, which is another generator's output. That works only because both run in the same compilation and the catalog generator emits code that *names* the method; it never needs to *discover* it.
- **Mediator's output, and Mediator cannot see State's.** Task 075 found Mediator registered none of the generated `LoadActionSet.Handler`. The fix was to stop generating handlers (`kanban/done/075-fix-persistence-generated-handler-registration-under-mediator.md`, sections "Description" and "Resolution").
- **Razor-generated component members.** `[Parameter]` properties declared in `@code` exist only in Razor generator output.

**Spike results** (`report/spike/action-set-generator-spike.output.txt`):

| Action shape | Generated | Result |
|---|---|---|
| class with an explicit constructor `(int amount = 1)` | `Add(int amount = 1, CancellationToken?)` | correct, but **Modified** after an unrelated edit |
| class with `int Amount { get; init; }` and no constructor | `IncrementCount(CancellationToken?)` | compiles; the amount cannot be passed. This is the shape of the test app's own `IncrementCountActionSet` (`tests/test-app/test-app-client/features/counter/actions/counter-state.increment-counter.cs:14-17`). |
| `sealed record Action(int Amount)` | nothing | silent: a record is not a `ClassDeclarationSyntax` |
| `sealed class Action(int amount)` (primary constructor) | `Add(CancellationToken?)` | **CS7036** in generated code |
| a nested `SampleActionSet` in a non-state static class | `partial class Docs { … }` | CS0260, CS0708 and CS0119 in generated code |

**Runtime reflection a generator could replace, and where a generator cannot help:**

- **Component parameter change detection** (`source/timewarp-state-blazor/components/timewarp-state-component.check-complex-parameter-changed.cs:24-35`). It uses `GetProperties` and `GetMethod` with four trim suppressions, whose note says "Follow-up: a generated parameter map".
  - A generator cannot see `@code` members, so a generated map is not feasible for `.razor` files.
  - A runtime fix is: keep the last `ParameterView` values per component and compare against them. That needs no reflection and no generator.
- **`[PersistentState]` checked by `GetCustomAttribute`** (`source/timewarp-state-plus/features/persistence/state-initialized-notification-handler.cs:34`) and **`[TrackAction]` checked by `IsDefined`** (`source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs:26`). Both are trim-safe today. A generated table, or a marker interface plus a Mediator behavior constraint, would move the decision to compile time.
- **JSON for persistence and telemetry.** Persistence calls `JsonSerializer.Deserialize(string, Type, options)` (`source/timewarp-state-plus/features/persistence/services/persistence-service.cs:83`). Telemetry skips a state that has no user-supplied `JsonTypeInfo` (`source/timewarp-state-telemetry/state-snapshot-recorder.cs:36-37,120-126`). A generated `JsonSerializerContext` would be **invisible to the System.Text.Json generator**, which is the same wall again.

### 4.2 Mediator generator inventory

- **One generator** (`MG/mediator-generator.cs:22-93`) with named steps:
  - `Options` (`AnalyzerConfigOptionsProvider`).
  - `HostCandidates`: three narrow `CreateSyntaxProvider`s. These are `TypeDeclarationSyntax { BaseList: not null }` (`MG/candidate-extractor.cs:29-32`), assembly attributes, and runtime declarations. Each extracts equatable `TypeFact` and `BehaviorFact` values (`MG/candidate-facts.cs:18-60`).
  - `ReferencedAssemblies`: `MetadataReferencesProvider.Collect()` plus a `HostIdentity` selected from `CompilationProvider`. It walks only assemblies marked `[assembly: MediatorAssembly]` (`MG/referenced-assembly-candidates.cs:41-101,167-178`).
  - `Model` (`CandidateGraph.Create`), then `Sources` and `Diagnostics`.
- **`ForAttributeWithMetadataName` is avoided on purpose.** The design note says it "combines CompilationProvider, which re-runs the transform on every edit (Unchanged, not Cached)" (`MG/mediator-generator.cs:11-13`).
- **One graph, several emitters.** `CandidateGraph.Create` builds a symbol-free `EmissionGraph` once (`MG/candidate-graph.cs:324`, `MG/emission-graph.cs:5-37`). `MediatorEmitter` and `ManifestEmitter` both read it (`MG/candidate-graph.cs:332-333`).
- **The analyzer holds a second graph builder,** `MessageGraphBuilder` (`MA/message-graph-builder.cs`, 922 LOC). The generator links the analyzer sources in (CS8784). So there are two graph builders to keep in step (`MG/candidate-graph.cs:6-12`). That is a maintenance cost worth not copying.
- **Diagnostics** are carried as equatable `DiagnosticInfo` and `LocationInfo` (`MG/mediator-model.cs:208-275`). They are re-joined to `Compilation` only to place each span (`MG/mediator-generator.cs:87-93`). When the generator is present, the analyzer skips TWM001-TWM003 so each is reported once (`MG/mediator-generator.cs:16-17`).
- **Output** is `Mediator.g.cs` (a type switch with `Dispatch_*` and per-request `Chain_*`), scoped `Sender_*` and `Publisher_*`, `AddGeneratedMediator<TScope>()`, and `MediatorManifest` (`Version = 2`, a JSON const) for tooling. State already consumes that manifest (`source/timewarp-state/features/pipeline/pipeline-behavior-registry.cs:1-47`).
- **Tests:**
  - `tests/timewarp-mediator-generators-compilation-tests/incremental-generator-tests.cs:16-55` asserts that an unrelated edit leaves every named step `Cached`, and that a new handler modifies only `HostCandidates`, `Model` and `Sources`.
  - Compile-and-assert shape tests, end-to-end generated tests, and a packed-nupkg smoke test.
  - No snapshot tests.
- **History:** the first design was `CompilationProvider.Combine(options).Select(...)`, which rebuilt the graph on every keystroke (`documentation/design/2026-10-13-15.0-breaking-redesign-review.md:285-300`, F9). Tasks 013-011 and 021-006 fixed it. This is exactly where State's clone generator is today.
- **Size:** 17 files and 3,866 LOC, plus 1,702 LOC of linked analyzer sources.

### 4.3 Nuru generator inventory

- **One generator** (`NA/generators/nuru-generator.cs:26-27`), with no separate `DiagnosticAnalyzer`. Validation moved into the generator (`:13-14`, `NA/validation/`).
- **Inputs:**
  - `CreateSyntaxProvider` for `Build()` call sites.
  - `ForAttributeWithMetadataName` for `[NuruRoute]`, `[Operation]` and `[assembly: NuruEndpointAssembly]`.
  - Further syntax providers for `Map<T>()`, `RunAsync` and `AddConfiguration`.
  - `CompilationProvider` for assembly metadata (`:39-216`).
- **Cross-generator awareness:** `GeneratedMediatorDetector` predicts what Mediator's generator will emit. It reads Mediator's compiler-visible MSBuild properties, because "a generator cannot see another generator's output" (`NA/generators/extractors/generated-mediator-detector.cs:6-15`). This is the cheap cooperation precedent.
- **Pipeline:** Locate, Extract, Interpret (a semantic DSL interpreter, `NA/generators/interpreter/dsl-interpreter.cs`, 2,447 LOC), Build IR, then Emit (`.agent/workspace/2025-12-31T12-00-00_source-generator-architecture.md:3-28`).
  - The interpreter replaced a syntax-based chain extractor that broke on real code styles (`kanban/done/277-epic-semantic-dsl-interpreter-with-mirrored-ir-builders.md:1-30`).
- **Caching:**
  - The emit model is `GeneratorModel`, a record built from `EquatableArray` (`NA/generators/models/generator-model.cs`, `equatable-array.cs`).
  - Diagnostics and validation stay uncached (stage A). Emission is cached (stage B, tracking names `NuruGeneratorModel` and `NuruEnumInfo`, `nuru-generator.cs:176-241`).
  - Incrementality was restored after three cache killers (raw `Compilation`, `ImmutableArray` reference equality, live `Location`) per `kanban/done/454-010-restore-generator-incrementality.md`.
- **Output:** one file, `NuruGenerated.g.cs`. It holds `[InterceptsLocation]` replacements of `RunAsync`, plus generated routing, binding, DI, help, completion and REPL code.
- **Tests:** Jaribu runfiles `tests/timewarp-nuru-tests/generator/generator-01…72`. Most build a real app and assert behavior. `generator-37-incrementality-caching.cs` asserts the named steps stay `Cached` or `Unchanged`.
- **Size:** 128 files and 33,412 LOC. Nuru also packs ICSharpCode.Decompiler to lower referenced `AddX` bodies, which is a heavy dependency.

### 4.4 Comparison

| Dimension | State | Mediator | Nuru |
|---|---|---|---|
| Discovery | four separate scans: two by name or text match, one by attribute, one whole-compilation symbol walk | narrow syntax candidates into equatable facts, plus a metadata scan of opted-in referenced assemblies | syntax and attribute locators, then a semantic interpreter of the user's DSL |
| Pipeline shape | four independent generators; DI registration is a side effect of the clone generator | one generator, one graph, several emitters | one generator, one model, many emitters into one file |
| Incremental caching | catalog and persistence equatable; ActionSet not (spike: Modified); clone reruns per compilation by design | every step named and asserted `Cached` | emit stage named and asserted; diagnostics stage uncached by design |
| Diagnostics | separate analyzer project (TWS*), generator-only TWSG001 and TWSG002, untracked SG002 noise | equatable `DiagnosticInfo`; analyzer and generator split by a property so nothing is reported twice | everything in the generator |
| Cross-assembly | the clone planner walks referenced symbols; the catalog aggregates through a generated assembly attribute | `[assembly: MediatorAssembly]` opt-in plus metadata scan | `[assembly: NuruEndpointAssembly]` plus semantic model |
| Testing | Fixie with `CSharpGeneratorDriver`; strong for clone, none for ActionSet, no incrementality test | compile tests, end-to-end tests, an incrementality test, a nupkg smoke test | runfile behavior tests plus an incrementality test |
| AOT and trimming | generated clones, registration and catalog are reflection-free; remaining reflection is listed in 4.1 | reflection-free dispatch, `PublishAot` sample | interceptors replace reflection routing |
| Maintenance cost | about 5k LOC, 80% of it the clone planner; the small generators are cheap but under-tested | two graph builders to keep in step | large (33k LOC); interpreter and decompiler are the cost centers |

**Lessons that transfer:**

1. Build one equatable model per concern and name every step.
2. Assert caching in a test. CI cannot see a caching regression any other way (the Nuru generator-37 header says so).
3. Carry diagnostics as equatable data.
4. Put cross-assembly membership behind an assembly attribute.
5. Emit a JSON manifest for tooling.

**Lessons not to copy:**

- Two graph builders (Mediator).
- A DSL interpreter and a decompiler (Nuru). State has no fluent DSL to interpret.

**Open question to settle by measurement.** Mediator's note says `ForAttributeWithMetadataName` reruns transforms on every edit. Nuru and State's catalog both use it. Neither repository has a test that compares the two styles, so a State tracking test should settle it for State rather than doctrine.

### 4.5 The generator wall and who owns which side

| Output a State generator might want to emit | The generator that would have to see it | Owner | Can it be moved? |
|---|---|---|---|
| an `IAction` or a handler | TimeWarp.Mediator | TimeWarp | yes, with a Mediator feature (Option 3b) |
| a `JsonSerializerContext` or `[JsonSerializable]` | System.Text.Json | Microsoft | no; use a code fix that writes into user source (Option 2) |
| a component `[Parameter]` map | Razor (the input side) | Microsoft | no; use the runtime `ParameterView` fix |
| a `[Page]` route or an architecture roster entry | architecture generators | TimeWarp | yes, with the same "declared fact" idea as 3b |

The architecture proposal's Option C needs "the TimeWarp.State and TimeWarp.Mediator rules copied, or the work moved into TimeWarp.State" (`464/proposal.md:65-83,317-342,513-521`). Moving the work into State does not by itself remove the Mediator wall. Only 3a or 3b does.

## 5. Recommendations: options for Steve

The options are not exclusive. Option 0 is cheap whatever else is picked. Options 1 and 3 are independent forks. Options 4 and 5 depend on 3b.

### Option 0: fix the current generators in place (hygiene)

**What changes:**

- **ActionSet generator:**
  - Move to an equatable record model.
  - Discover by a semantic check: the parent derives from `State<T>`, and the Action implements `IAction` or is a nested `Action` class or record.
  - Read primary constructors (`TypeDeclarationSyntax.ParameterList`) and positional records.
  - Optionally use `required`/`init` properties as parameters.
  - Drop SG002.
  - Report a TWS diagnostic instead of emitting broken code.
  - Add direct tests.
- **Persistence generator:** match `[PersistentStateAttribute]` too, or use `ForAttributeWithMetadataName`.
- **Every generator:** add `WithTrackingName` to every step, plus one incrementality test in the Mediator style.

**What the developer writes after this fix** (today: no method, or CS7036):

```csharp
public partial class CounterState
{
  public static class AddActionSet
  {
    public sealed record Action(int Amount, bool Animate = false) : IAction;
    internal sealed class Handler(IStore store) : StateActionHandler<Action>(store) { /* … */ }
  }
}
```

**Generated:**

```csharp
partial class CounterState
{
  public async Task Add(int Amount, bool Animate = false, CancellationToken? externalCancellationToken = null)
  { /* linked token, then Sender.Send(new AddActionSet.Action(Amount, Animate), token) */ }
}
```

- **Cost:** changes stay inside `SG/` and the generator tests. Not breaking, except that code which used to fail now compiles, and `[CatalogAction]` on a record action starts producing executors.
- **Unblocks:** records and primary constructors as actions everywhere, including the template's `DefaultApiHandler` actions. A caching baseline for later options.
- **Does not unblock:** any new consumer capability.

### Option 1: one State graph, several emitters, and a State manifest

Replace the per-generator scans with one `TimeWarpStateGenerator` in the Mediator style:

- Equatable facts per state, action set, action, handler and attribute: `[CatalogAction]`, `[PersistentState]`, `[TrackAction]` and `[GenerateClone]`.
- A `StateGraph` built once.
- Emitters for dispatch methods, catalog, persistence `Load`, DI registration (moved out of the clone emitter), and a new `TimeWarpStateManifest`.
- The clone planner stays a semantic stage. Wrap it as `CompilationProvider.Select(plan)`, returning an equatable `ClonePlan(text, diagnostics)`, so at least the output and downstream are `Cached` when the plan is equal. Planning itself still reruns.
- Analyzers stay a separate project, but read the same fact extractors, linked as source. Keep one builder, not Mediator's two.

**What the developer writes:** nothing new.

**Generated, new:**

```csharp
// TimeWarpStateManifest.g.cs (illustrative; one per assembly, in the sanitized assembly namespace, like the catalog)
[assembly: global::Test.App.Client.GeneratedStateManifestProviderAttribute]
internal static class TimeWarpStateManifest
{
  public const int Version = 1;
  public const string Json = """
  {"version":1,"assembly":"Test.App.Client","states":[
    {"type":"Test.App.Client.Features.Counter.CounterState","persistent":null,"clone":"generated",
     "actions":[{"type":"Test.App.Client.Features.Counter.CounterState+AddActionSet+Action",
                 "method":"Add","parameters":[{"name":"amount","type":"int","default":"1"}],
                 "catalog":null,"tracked":false}]}]}
  """;
}
```

**What consumers get:**

- **DevTools:** `AddStateDevToolsExportAsync` posts the manifest once at startup. The inspector can then list every state and action before any span arrives, and show parameter schemas. With the catalog executors that already exist, it can offer a "dispatch from DevTools" button in Development. None of that is possible today: the inspector only knows what spans tell it (`source/timewarp-state-devtools/readme.md`).
- **Architecture coverage analyzers:** the A1 coverage check in the 464 proposal can compare contracts against this manifest (or against State's facts linked as source) instead of re-deriving State's rules.
- **`LogTimeWarpStateMiddleware`-style startup checks** can cross-check the State manifest against `MediatorManifest`, for example "action X has no handler in ClientPipeline".

**Cost:** a rewrite of the three small generators into one pipeline, with the clone planner untouched inside it. Hint names and generated text can stay identical, so it is not breaking for consumers. A new public `StateManifest` reader type in `TimeWarp.State`, and DevTools ingest and UI work to use it.

**Unblocks:** DevTools state and action discovery and dispatch. One place to add future per-action facts. Measurable caching.

### Option 2: compile-time metadata where another generator is involved, using an analyzer plus a code fix

Where the output must be seen by a generator TimeWarp does not own (System.Text.Json), a generator is the wrong tool. Use an analyzer and code fix that write into user source instead, because user source is visible to every generator.

**What the developer writes:**

```csharp
[JsonSerializable(typeof(CounterState))]   // TWS00xx: CounterState is [PersistentState] / snapshotted but not in a context
internal sealed partial class AppStateJsonContext : JsonSerializerContext;
```

The analyzer reports each state that persistence or telemetry needs but no context lists. The code fix adds the `[JsonSerializable]` line. `AddTimeWarpState(o => o.JsonSerializerOptions.TypeInfoResolver = AppStateJsonContext.Default)` stays the user's one line.

- **Cost:** one analyzer, one code fix, and tests. No generator change.
- **Unblocks:** AOT-clean persistence and telemetry snapshots without guessing. Today telemetry silently skips a state that has no type info (`state-snapshot-recorder.cs:120-126`).
- **Not addressed:** the component parameter map. That one is a runtime change (compare against the last `ParameterView`) and needs no generator. It is listed here so it is not mistaken for generator work.

### Option 3: make generated handlers possible

This is the strategic fork. It decides whether State, or the architecture template, can ever generate a handler or an action.

#### 3a. Partial shell, generated body (works today, no Mediator change)

The developer declares the type and its base list, so Mediator sees it in syntax. State fills in the members. This is 464 Option B, generalized.

**What the developer writes:**

```csharp
public partial class RoleState
{
  public static partial class CreateRoleActionSet
  {
    public sealed record Action(CreateRole.Command Command) : IAction;

    [GenerateHandler]
    internal sealed partial class Handler(IStore store) : StateActionHandler<Action>(store);

    private static partial void Reduce(RoleState state, Action action);   // the only hand-written logic
  }
}
```

**Generated:**

```csharp
internal sealed partial class Handler
{
  public override ValueTask Handle(Action action, CancellationToken cancellationToken)
  {
    Reduce(Store.GetState<RoleState>(), action);
    return default;
  }
}
```

- **Cost:** a State emitter (it fits Option 1's graph) plus an analyzer that catches a missing `Reduce` (the compiler already gives CS8795).
- **Gain:** it saves only the `Handle` boilerplate. The shell still has to be written.

#### 3b. Mediator learns declared handlers (TimeWarp change in Mediator)

Mediator's host candidate extractor accepts a Mediator-owned attribute on a request type in source. The attribute names a handler that another generator will emit. Mediator emits dispatch to that name. Generated code may reference generated code, because the final compilation contains both. Only discovery needed the wall moved.

```csharp
// TimeWarp.Mediator (new, public):
[AttributeUsage(AttributeTargets.Class)]
public abstract class GeneratedHandlerAttribute : Attribute
{
  public abstract string HandlerTypeName { get; }   // relative to the request's containing type
}

// TimeWarp.State (new): derives from it, so Mediator's semantic check finds it by base type.
public sealed class ReducerAttribute : GeneratedHandlerAttribute
{
  public override string HandlerTypeName => "Handler";
}
```

- **Cost:**
  - **Mediator:** a fact kind in `candidate-extractor.cs` and `candidate-graph.cs`, a TWM diagnostic when the named handler does not exist after generation (a State analyzer can check earlier), and a version both packages must move to together.
  - **State:** emitters on the Option 1 graph.
- **Risk:** if the other generator does not emit the handler, the error lands in `Mediator.g.cs` as CS0246. The diagnostics need care.

**Rejected alternatives:**

- A runtime handler table that Mediator consults before `MissingMessageHandlerException`. It loses compile-time behavior weaving.
- Folding Mediator's emitters into State's generator. That is too much coupling, and two generators would race to own `Mediator.g.cs`.

**Unblocks:** Options 4 and 5, a per-state persistence action again if wanted (task 075's original shape), and 464 Option C without copying Mediator's rules.

### Option 4: contract-bound action sets (needs 3b; 3a gives a partial version)

This consumer is the architecture SPA (464). A thin generic hook belongs in State. The contract knowledge belongs in architecture.

**What the developer writes:**

```csharp
[ContractState(typeof(GetRoles), typeof(CreateRole), typeof(DeleteRole))]
public sealed partial class RoleState : State<RoleState>
{
  private partial void OnCreateRoleSucceeded(CreateRole.Response response) => /* update list */;
}
```

**Generated:** `CreateRoleActionSet.Action` and its `Handler : DefaultApiHandler<…>`, marked so Mediator dispatches it (3b), plus `RoleState.CreateRole(CreateRole.Command command, …)`, plus a catalog entry with `Description` and `Permissions` taken from the contract, which fixes the hand-copied `[CatalogAction]` drift (`464/proposal.md:95-109`).

- **Cost:** the architecture repository owns that generator. State owns 3b and the Option 1 graph, so a generated action still gets its dispatch method and catalog entry. That last point holds only if State's dispatch-method and catalog emitters can take facts that another generator declares. The same wall applies one level up, so the architecture generator must itself emit the dispatch method, or State must expose the rule as a reusable emitter library.
- **Unblocks:** 464 Options C and D without copying State's or Mediator's rules.

### Option 5: shorter action authoring (needs 3b)

**What the developer writes today:** about 25 lines per action (`counter-state.increment-counter.cs`).

**What they could write:**

```csharp
public sealed partial class CounterState : State<CounterState>
{
  public int Count { get; private set; }

  [Reducer] public sealed record IncrementCountAction(int Amount = 1) : IAction;
  private void Reduce(IncrementCountAction action) => Count += action.Amount;
}
```

**Generated:**

```csharp
partial class CounterState
{
  internal sealed class IncrementCountActionHandler(IStore store) : StateActionHandler<IncrementCountAction>(store)
  {
    public override ValueTask Handle(IncrementCountAction action, CancellationToken cancellationToken)
    { Store.GetState<CounterState>().Reduce(action); return default; }
  }

  public async Task IncrementCount(int Amount = 1, CancellationToken? externalCancellationToken = null) { /* send */ }
}
```

`[Reducer]` derives from Mediator's `GeneratedHandlerAttribute`, so `Mediator.g.cs` dispatches `IncrementCountAction` to `IncrementCountActionHandler`. Async reducers (`ValueTask ReduceAsync(…)`) and reducers that take services would need a rule for constructor injection. The existing `XActionSet { Action; Handler }` shape keeps working beside it.

- **Cost:** 3b, a new authoring convention in State's documentation and analyzers (the TWS0001 nesting rule needs a second accepted shape), and the clone and registration paths stay as they are.
- **Unblocks:** a much smaller surface for every app and for the template's scaffolding.

### Summary of options

| Option | Repositories | Breaking | Consumer-visible gain | Depends on |
|---|---|---|---|---|
| 0 hygiene | State | no | record and primary-constructor actions work; no broken generated code | — |
| 1 one graph and a manifest | State (and DevTools) | no | DevTools knows states and actions up front; dispatch from DevTools; manifest for analyzers | 0 (folds it in) |
| 2 analyzer and JSON code fix | State | no | AOT-clean persistence and telemetry, guided | — |
| 3a partial shell and body | State | no | less handler boilerplate | 1 (fits best) |
| 3b declared handlers | Mediator and State, lockstep | no (additive) | generated handlers and actions become possible | Mediator release |
| 4 contract-bound action sets | Architecture, State, Mediator | no | 464 Options C and D without copied rules | 3b, 1 |
| 5 reducer authoring | State and Mediator | no (additive) | about 25 lines down to about 3 per action | 3b |

**Ordering, if Steve wants one:**

1. Option 0 first, whatever else.
2. Option 1 next. It is the structural change that pays off independently, through DevTools.
3. Decide 3b explicitly. It is the only change that lifts the wall behind 464 Option C and Option 5, and it is a Mediator decision as much as a State one.
4. Option 2 is orthogonal and can go at any time.

## 6. References

- State:
  - `SG/*.cs` (cited per row above).
  - `source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs:54-63` and `source/timewarp-state/assembly-marker.cs:12-29` (Mediator membership).
  - `kanban/done/075-…md`, `kanban/done/097-proposal-source-generator-for-state-cloning-aot-friendly/`, `kanban/done/134-…`, `kanban/done/135-…`.
- Mediator:
  - `MG/mediator-generator.cs`, `MG/candidate-graph.cs`, `MG/emission-graph.cs`, `MG/referenced-assembly-candidates.cs`, `MA/message-graph-builder.cs`.
  - `documentation/architecture.md`, `documentation/design/2026-06-17-source-gen-aot-rewrite-spec.md:42`, `documentation/design/2026-10-13-15.0-breaking-redesign-review.md:285-300`.
  - `tests/timewarp-mediator-generators-compilation-tests/incremental-generator-tests.cs`.
- Nuru:
  - `NA/generators/nuru-generator.cs`, `NA/generators/extractors/generated-mediator-detector.cs:6-15`, `NA/generators/interpreter/dsl-interpreter.cs`.
  - `.agent/workspace/2025-12-31T12-00-00_source-generator-architecture.md`.
  - `kanban/done/277-…`, `kanban/done/454-010-restore-generator-incrementality.md`.
  - `tests/timewarp-nuru-tests/generator/generator-37-incrementality-caching.cs`.
- Architecture: `464/proposal.md` §3, §4a, §5 (Options B and C) and §7, and `464/task.md:30`.
- Spike: `report/spike/action-set-generator-spike.cs` and `report/spike/action-set-generator-spike.output.txt`.
