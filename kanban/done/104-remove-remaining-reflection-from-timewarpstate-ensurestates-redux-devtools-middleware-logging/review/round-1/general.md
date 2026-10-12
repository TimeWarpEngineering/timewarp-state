# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** origin/master...HEAD (b9288f80)

## Summary
The change meets the task's acceptance criteria. The Release build of `TimeWarp.State` has zero IL2xxx/IL3xxx warnings, the `rg` check finds only `configureTimeWarpStateOptionsAction?.Invoke`, all tests pass, and `ganda repo audit` exits 0. On CoreCLR, generated registration covers the same concrete states the old scan found in test-app-client (8/8) and in TimeWarp.State.Plus (4/4), and that holds across assemblies. I found no blocking bug. The issues below are about how the registrar gets hooked up (it relies on module-initializer timing and does nothing, silently, when an assembly has no registrar), a few behavior changes the release notes don't mention, and generator cost.

## Verification
- `dotnet build source/timewarp-state/timewarp-state.csproj -c Release --nologo` (also with `--no-incremental`): Build succeeded, no IL2xxx/IL3xxx (only existing TW0007/NU1510 warnings).
- `rg -n 'UnconditionalSuppressMessage|GetTypes\(|GetAssemblies\(|GetMethod\(|GetInterfaces\(|\.Invoke\(' source/timewarp-state`: one hit, `configureTimeWarpStateOptionsAction?.Invoke` (`service-collection-extensions.add-timewarp-state.cs:36`). As expected.
- `dotnet test tests/timewarp-state-tests`: 120 passed, 1 skipped. `dotnet test tests/timewarp-state-source-generator-tests`: 90 passed.
- `dotnet test tests/client-integration-tests` (test-app consumer with `Assemblies = [Test.App.Client, TimeWarp.State.Plus]`): 64 passed, 1 skipped. `action-tracking-tests.cs` resolves the Plus `ActionTrackingState` through DI, so cross-assembly registration works on CoreCLR.
- `dotnet test tests/timewarp-state-plus-tests`: 40 passed. `tests/timewarp-state-devtools-inspected-app` builds.
- `ganda repo audit`: exit 0 (29 pass, 1 existing advisory kebab-path warning).
- Generated output: TimeWarp.State itself now runs the generator, and its only output is `TimeWarpPipelineBehaviors.g.cs` (StateTransactionBehavior, StateInitializationPreProcessor). The test-app-client registrar lists all 8 of its states.
- Scratch probe in /tmp (now deleted), a console app referencing timewarp-state, timewarp-state-plus and the generator:
  - `AddTimeWarpState(o => o.Assemblies = [typeof(Plus.AssemblyMarker).Assembly])` registered ThemeState, ActionTrackingState, RouteState and TimerState before any Plus method had run (CoreCLR).
  - Behaviors nested in a generic outer type (`Outer<>.Inner<,>`, `Outer<>.Plain`) were emitted and matched by `GetComponentOrder`.
  - A `private protected` nested state fails to compile with CS0122 (see Issue 6).
- Samples use `PackageReference TimeWarp.State`, so the generator flows to them from the package. Every in-repo project that calls `AddTimeWarpState` and declares states references the generator.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-state/extensions/state-registration-registry.cs:39
- Description: Registration only works if each selected assembly's `[ModuleInitializer]` has already run when `Apply` is called. ECMA-335 only promises the initializer runs at or before the first static field access or method call in that module. `typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly`, which is how test-app, sample-02 and the integration tests select Plus, does neither. CoreCLR runs the initializer early (probe and integration tests confirm). Nothing tests this on Mono/Blazor WASM, which is the main host for this library, or on NativeAOT. If the initializer runs later there, `Apply` skips the assembly without a sign, and `GetState<RouteState>()` fails later with "Unable to resolve service". The old reflection scan did not depend on initializer timing.
- Suggestion: Make it deterministic. For example, in `Apply` call `RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle)` before the lookup (it is idempotent; confirm it is trim/AOT clean). Or confirm the Mono/WASM behavior with a test. In both cases, add a runtime test that registers states from a referenced assembly the test has not otherwise touched. Today's tests only use the executing test assembly and TimeWarp.State.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state/extensions/state-registration-registry.cs:46
- Description: If an assembly in `TimeWarpStateOptions.Assemblies` has no registrar, it contributes nothing and nothing reports it. That happens when it was compiled without the generator, for example a `ProjectReference` to `timewarp-state.csproj` without `OutputItemType="Analyzer"`, which is the gap this diff had to fix in `timewarp-state-telemetry.csproj`. The old scan registered states either way. The failure now shows up only when a state is resolved.
- Suggestion: Make the miss visible, for example with a debug-level log or a `Debug.Assert`, or document it. The release notes and cloning.md should say that the states' assembly must be compiled with the TimeWarp.State generator. `PackageReference` consumers get it automatically; `ProjectReference` consumers must add it as an Analyzer.
- Status: open

### Issue 3 — Severity: suggestion
- File: documentation/release-notes/release12.0.0-beta.12.md:18
- Description: Registration semantics changed in ways the release notes don't mention (cloning.md covers part of it):
  - (a) Open generic `ICloneable` states: the old scan registered the open definition (`TryAddTransient(typeof(Box<>))`), which DI resolves for any `T`. Now only closed constructions that appear in syntax in the same compilation are registered, so a construction made only at run time or in a generic method (`GetState<Box<T>>`) no longer resolves.
  - (b) A closed construction is registered under the assembly that names it, not the one that declares the state. The host must therefore list the constructing assembly.
  - (c) Private, protected and file-local states that the scan used to register are now skipped.
- Suggestion: Add a short "Behavior changes" bullet listing (a) to (c) to the release notes.
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-state/features/pipeline/pipeline-behavior-registry.cs:51
- Description: `GetComponentOrder(this IServiceCollection, Type componentType)` is public and used to match any generic interface definition, because it walked `GetInterfaces()`. The generator now records only `IPipelineBehavior<,>`, so any other `componentType` returns an empty list. The release notes say the API keeps its signature, which is true, but its meaning is now narrower. The only callers in the repo pass `IPipelineBehavior<,>`, so nothing breaks today.
- Suggestion: Document on the method that only `IPipelineBehavior<,>` is supported, or throw `ArgumentException` for any other `componentType`.
- Status: open

### Issue 5 — Severity: suggestion
- File: source/timewarp-state-source-generator/pipeline-behavior-registry-source-generator.cs:23
- Description: The new generator uses only `context.CompilationProvider` with no equatable intermediate model, and it walks every namespace and nested type of the assembly on each compilation change, i.e. every keystroke in the IDE. Every consumer of the TimeWarp.State package runs it. The clone generator has the same shape but documents it as required by its whole-compilation plan. This generator does not need the whole compilation.
- Suggestion: Use `SyntaxProvider.CreateSyntaxProvider` on generic `ClassDeclarationSyntax` with a base list, map each to an equatable record (the display string), then `Collect()`. That lets the incremental cache skip unchanged edits.
- Status: open

### Issue 6 — Severity: nit
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1874
- Description: The planner's `CanName` treats `Accessibility.ProtectedAndInternal` (`private protected`) as nameable within the same assembly, but a namespace-level file class cannot access such a type. Probe: a `private protected sealed class PpState : State<PpState>` nested in a public class fails with CS0122. The error is in the existing clone registry lines and also in the new `TryAddTransient<global::Probe.Host.PpState>` line. The `CanName` problem is older than this diff, but `ServiceTypes` (line 3190) reuses it. The new pipeline generator's `CanName` and the planner's `CanNameFromGeneratedCode` both exclude `ProtectedAndInternal` correctly, so the code is inconsistent. cloning.md's "A private, protected, or file-local state is not registered" does not cover this case.
- Suggestion: Exclude `ProtectedAndInternal` in the planner's `CanName`, or at least in `ServiceTypes`, and add a generator test. This could be a follow-up because the clone path already breaks.
- Status: open

### Issue 7 — Severity: nit
- File: source/timewarp-state-source-generator/pipeline-behavior-registry-source-generator.cs:30
- Description: The generator checks only that `TimeWarp.Mediator.IPipelineBehavior`2` resolves. It then emits a call to `global::TimeWarp.State.PipelineBehaviorRegistry.Register` without checking that this type exists in the compilation. If a compilation sees the analyzer and Mediator but not this version of TimeWarp.State (for example a mismatched or older TimeWarp.State reference), the generated file fails to compile with a confusing error.
- Suggestion: Also return early when `compilation.GetTypeByMetadataName("TimeWarp.State.PipelineBehaviorRegistry")` is null.
- Status: open
