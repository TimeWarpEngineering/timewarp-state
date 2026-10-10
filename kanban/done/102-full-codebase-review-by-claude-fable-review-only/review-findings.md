# TimeWarp.State full codebase review

Reviewer: Claude Fable (claude-fable-5-1), ganda profile `implementer-claude-fable`
Date: 2026-10-10
Commit reviewed: `b255a623` on branch `task/102-full-codebase-review-by-claude-fable-review-only` (origin/master at `60041165`)
Scope: whole repository at 12.0.0-beta.11. Review only; no product files were changed.

Method: every C# and TypeScript file under `source/` was read in full, plus the build props, csproj files, CI workflow, dev CLI, scripts, test inventory and representative tests, docs, samples, and the Results of tasks 037 and 097. A Release build of Plus, Telemetry, Policies and the test app server was run to inventory warnings, and `ganda repo audit` was run (passes, one kebab-path advisory). Package dependencies were verified against the nuspec files in the local NuGet cache for 12.0.0-beta.10 and 12.0.0-beta.11.

## Overall assessment

The codebase is in good shape for a beta and noticeably better than the 2026-06 review snapshot. The architecture is clean and the layering is real: `TimeWarp.State` has no Blazor reference, pipeline behaviors are woven at compile time by TimeWarp.Mediator 14 with explicit cross-assembly ordering (100 DevTools, 200 initialization, 300 transaction, 350 telemetry, 400 render subscriptions), and the Blazor, Plus and Telemetry packages sit on top without back references. Every source file carries Purpose and Design regions, and those regions are accurate; several of them record the exact reasoning for non-obvious choices (UnsafeAccessor holders, per-dispatch render suppression, reference-equality action keys). The source-generated cloner from task 097 is a serious piece of engineering: it handles cycles, polymorphic dispatch by runtime type, generic subtype closure, reference-assembly visibility, required members and a long list of collection shapes, and it fails the build instead of falling back to reflection. The analyzers are symbol-based rather than name-based, the action catalog and JavaScript dispatch are both opt-in and reflection-free, and the test suite covers the hard parts (concurrent first access to the store, transaction rollback, clone shapes, analyzer rules, generator output).

The weak spots are at the seams. Three things stand out above the rest. First, the published `TimeWarp.State` and `TimeWarp.State.Blazor` packages declare a runtime dependency on `Microsoft.CodeAnalysis.CSharp` because the root `Directory.Build.props` references it without `PrivateAssets`; every consumer, including Blazor WebAssembly apps, downloads the Roslyn compiler for a state library. Second, the clone-per-action transaction model has no per-state serialization, so when two actions on the same state overlap, a failure in one silently rolls back the other's committed changes. Third, the generated cloner has no "share by reference" opt-in for injected services, and the documented workaround (`[IgnoreDataMember]` on the service field) produces a clone whose service field is null, which then becomes the live state. Each of these is fixable in a small, contained change, and none of them undermines the direction of the design.

Beyond those, the review found a cluster of medium issues that are mostly about finishing or deleting half-built paths: Redux DevTools time travel is unreachable from the shipped JavaScript yet keeps the only remaining trim-unsafe reflection in the core package; `PersistentStateMethod.Server` and `PreRender` are public enum values that do nothing; `IStore.GetSemaphore` survives for a single caller that uses JavaScript `eval` to read the document title; the "test" assembly-name sniff that beta.5 promised to remove is still the only guard on some public members. The AOT story is real for `TimeWarp.State` and `TimeWarp.State.Telemetry` but not asserted for `TimeWarp.State.Blazor` or `TimeWarp.State.Plus`, which contain the reflection that a trimmed WebAssembly app will actually hit. Docs and readmes have accumulated stale badges, paths and claims (the Plus readme advertises a component that lives in the test app). None of this is large, but together it is the difference between a solid beta and a 12.0 release.

Recommended order of attack: the package dependency (one line), the transaction rollback guard (one `ReferenceEquals` check), a `[CloneShared]`-style attribute or equivalent for injected members, then the DevTools and persistence dead paths, then `IsAotCompatible` on the two remaining runtime packages.

## Critical

None.

## High

### H1. `TimeWarp.State` and `TimeWarp.State.Blazor` ship a runtime dependency on `Microsoft.CodeAnalysis.CSharp`

- `Directory.Build.props:39` adds `<PackageReference Include="Microsoft.CodeAnalysis.CSharp" />` to every project without `PrivateAssets="all"`. The comment above it explains the version floor for the analyzer and generator, but the reference is applied to the runtime libraries too.
- Verified in the local NuGet cache: `timewarp.state/12.0.0-beta.11/timewarp.state.nuspec` lists `Microsoft.CodeAnalysis.CSharp 4.14.0` and `JetBrains.Annotations 2026.2.0` as dependencies; `timewarp.state.blazor/12.0.0-beta.10` lists `Microsoft.CodeAnalysis.CSharp 4.14.0` as well. Task 037's Results record the same list.
- Why it matters: every consumer restores and deploys the Roslyn compiler (plus `Microsoft.CodeAnalysis` and `System.Collections.Immutable` pins) for a state-management library. For Blazor WebAssembly that is download size and trimming work for nothing. It also pins consumers to a Roslyn version band and can cause `NU1605`/`NU1608` conflicts in apps that reference newer Roslyn packages.
- `JetBrains.Annotations` (`source/timewarp-state/timewarp-state.csproj:39`) is a compile-time-only package and should also be `PrivateAssets="all"`.
- Suggestion: move the `Microsoft.CodeAnalysis.CSharp` reference into the analyzer and generator csproj files (they are the only ones that need it), or keep it in `Directory.Build.props` with `PrivateAssets="all"` and a `Condition` on the two netstandard2.0 projects. Mark `JetBrains.Annotations` `PrivateAssets="all"`. Add a packaging test (or a `dev pack` step) that fails when a nuspec dependency list contains anything outside an allow-list, so this cannot regress silently.

### H2. Overlapping actions on one state: a failure in one rolls back the other's committed work

- `source/timewarp-state/features/pipeline/state-transaction-behavior.cs:101` installs the clone as the live state before `next`, and `:131` restores `originalState` on failure unconditionally. There is no per-state serialization: `IStore.GetSemaphore` exists but the behavior does not use it.
- Scenario (Blazor Server or WASM, both single-scope): action A on `CounterState` starts, clone `c1` becomes live, A awaits an HTTP call. Action B on the same state starts, clones `c1` into `c2`, increments, completes, renders. A then throws. The catch restores `originalState`, discarding B's completed increment. The reverse also holds: if B fails, `c1` is restored and A's later writes land on the orphaned `c2` and are lost. In both cases nothing is logged about the second action.
- Handlers already re-read `Store.GetState<T>()` per access, so the ordinary interleaving works. It is only the rollback that is wrong, and it is wrong exactly when the user most expects the store to be consistent.
- Suggestion (minimal): roll back only if the store still holds this action's clone: `if (ReferenceEquals(Store.GetState(enclosingStateType), newState)) Store.SetState(originalState); else log that a concurrent action advanced the state`. Suggestion (full): serialize actions per state with a non-blocking `SemaphoreSlim.WaitAsync` in the transaction behavior (safe on single-threaded WASM because it never blocks), and delete `IStore.GetSemaphore`. Add a unit test with two interleaved `next` delegates to pin the behavior either way.

### H3. Generated cloner has no "share by reference" opt-in, and the documented workaround nulls injected services on the live state

- `source/timewarp-state-source-generator/state-clone-planner.cs:1082` fails the build (TWSG002) for a member whose type is an interface with no visible implementation, which is every injected service (`ILogger<T>`, `HttpClient` fails the metadata check instead, `NavigationManager`, `IJSRuntime`). The only escape hatches are `ICloneable` on the whole state or `[IgnoreDataMember]` on the member.
- `documentation/topics/cloning.md` ("Constructors") and `documentation/migrations/migration12.0.0-beta.11.md` both tell authors to mark service fields `[IgnoreDataMember]` or `[JsonIgnore]` and make the constructor accept `null`. The clone is constructed with `default!` arguments (`state-clone-planner.cs:1237`, `TryEmitConstruction`), ignored members "keep the value the constructor set", which is null, and `StateTransactionBehavior` makes that clone the live state. The next handler on that state dereferences a null service.
- Why it matters: states that hold an injected `HttpClient` or `NavigationManager` are the common pre-beta.11 pattern (the cacheable weather state in the test app would be one if it stored the client). Following the migration guide moves the failure from compile time to the first action after the first action, with a `NullReferenceException` whose stack does not mention cloning. The `InvalidCloneException` message (`invalid-clone-exception.cs`) points authors at the same attributes.
- Suggestion: add an explicit per-member opt-in to copy by reference, for example `[CloneShared]` in `TimeWarp.State` (or treat `[IgnoreDataMember]` as "copy by reference" for reference-typed members, which is closer to what AnyClone users expect). Alternatively have `StateTransactionBehavior` copy every `[IgnoreDataMember]` reference member from the original after cloning, the way it already copies `Sender`. Update the two docs and the exception message to the chosen rule, and add a shape test with an `ILogger` field.

### H4. `TimeWarpStateComponent` collection parameter check suppresses renders when any check hook is overridden

- `source/timewarp-state-blazor/components/timewarp-state-component.check-complex-parameter-changed.cs:189` compares collections by `Count()` only. `timewarp-state-component.cs:123` onward returns `false` from `ShouldRender` when `SetParametersAsync` ran, no parameter was judged changed, and no subscription or `ReRender` flag is set.
- Scenario: a component that overrides `HandleUnregisteredParameter` or any `Check*` method (which `documentation/topics/render-control.md` recommends) receives a `List<Item>` parameter. The parent replaces the list with a different list of the same length. `CheckForOverriddenMethods` is true, the collection check says "unchanged", `ParameterTriggered` stays false, `ShouldRender` returns false, and the child shows stale items. The e2e `should-render-test-page` has a `ChildComponentWithCollection`, but the test app overrides are on a base component, so only the overriding case is exercised.
- Related: `:132` treats only `IsPrimitive` and `string` as primitives, so `decimal`, `DateTime`, `Guid`, `enum` and user structs fall through to `:222`, where boxing makes `ReferenceEquals` always false. That direction is safe (over-render) but makes `RenderReasonDetail` claim a change that did not happen, which defeats the diagnostic purpose.
- Suggestion: for collections, fall back to element-wise `SequenceEqual` on materialized snapshots, or treat "same count" as "unknown" and return true; never enumerate an `IQueryable`. Route non-primitive value types through `Equals`. Add a unit test in `tests/timewarp-state-tests/timewarp-state-component/parameter-change-tests.cs` for same-count-different-items.

## Medium

### M1. Redux DevTools time travel is unreachable from the shipped JavaScript, but keeps the core package's only trim-unsafe reflection

- `source/timewarp-state-blazor/wwwroot/typescript/redux-dev-tools.ts:61` onward maps every `DISPATCH` payload (`COMMIT`, `JUMP_TO_STATE`, `IMPORT_STATE`, ...) to `undefined`, and `MessageHandler` ignores unmapped types. Only `START` reaches .NET. `CommitHandler` and `IReduxDevToolsStore.LoadStatesFromJson` are therefore dead at runtime.
- `source/timewarp-state/store/store.redux-dev-tools.cs:43-71` carries seven `UnconditionalSuppressMessage` attributes (IL2026, IL2070, IL2072, IL2075, IL3050) to scan `AppDomain.CurrentDomain.GetAssemblies()` and invoke `Hydrate` through `MethodInfo.Invoke`. Together with `EnsureStates` (`service-collection-extensions.add-timewarp-state.cs:85`) these are the only reflection sites left after task 097, and 097's own requirement was "no reflection in TimeWarp.State at all, used or unused".
- `State<T>.Hydrate` (`state/state.cs:62`) throws `NotImplementedException` by default; only five states in the repo override it.
- Suggestion: pick one. Either finish time travel (map `JUMP_TO_STATE`/`JUMP_TO_ACTION`, have the clone generator emit `Hydrate` so the reflection goes away, and test it), or delete `LoadStatesFromJson`, `Hydrate`, `IState<TState>`, `CommitHandler` and the seven suppressions now. The scratch backlog already lists splitting DevTools into its own package; deleting is consistent with that.

### M2. `IsAotCompatible` is set on two of four runtime packages; the reflection that trimmed WASM apps will hit is in the other two

- `source/timewarp-state/timewarp-state.csproj` and `source/timewarp-state-telemetry/timewarp-state-telemetry.csproj` set `IsAotCompatible`. `source/timewarp-state-blazor/timewarp-state-blazor.csproj` and `source/timewarp-state-plus/timewarp-state-plus.csproj` do not, so no IL warnings are produced for them (the Release build log confirms zero IL warnings overall).
- Reflection in those two packages: `TimeWarpStateComponent` parameter discovery via `GetProperties`/`GetMethod` (`check-complex-parameter-changed.cs`), `Expression.Compile` in `RegisterRenderTrigger` (`register-render-trigger.cs:153`), `JsonSerializer.Deserialize(string, Type, options)` and `Activator.CreateInstance` in `JsonRequestHandler` (`json-request-handler.cs:122`), reflection-based `JsonSerializer.Serialize(object, Type)` in `PersistentStatePostProcessor` and `PersistenceService`, and `GetCustomAttribute` in several Plus behaviors. Blazor WebAssembly publishes with trimming on by default, and the trimmer only warns about assemblies that opt into analysis.
- No trimmed or AOT publish is in the CI pipeline (`tools/dev-cli/endpoints/workflow-command.cs`: build, test, e2e, pack, verify-samples). `samples/04-telemetry` has `EnableTrimAnalyzer` and the dev CLI itself is `PublishAot`, which is not the same thing.
- Suggestion: set `IsAotCompatible` (or at least `IsTrimmable` + `EnableTrimAnalyzer`) on Blazor and Plus, annotate or suppress with justification, and add a `PublishTrimmed` smoke of `samples/05-persistence` to `dev verify-samples` with `TrimmerSingleWarn=false`.

### M3. Transaction behavior swallows handler failures and cancellation, so callers cannot observe either

- `state-transaction-behavior.cs:144` returns `default!` after rollback, for both exceptions and `OperationCanceledException`. `await counterState.IncrementCount(ct)` completes normally when the handler threw or when `ct` was cancelled. Cancellation is the worse case: cooperative cancellation contracts in .NET expect `OperationCanceledException` to propagate, and callers chaining actions will continue as if the first succeeded.
- `ExceptionNotification` covers the error case for apps that subscribe; nothing covers cancellation.
- Suggestion: rethrow `OperationCanceledException` after rollback (it is not a failure, as the Design region says, so publishing is right to skip, but swallowing is not). Consider a `TimeWarpStateOptions` switch to rethrow handler exceptions after publishing, for hosts that want `Send` to fault. Document the current semantics in `documentation/topics` either way; today they are only in a code comment.

### M4. `Store.Reset` is a partial reset

- `source/timewarp-state/store/store.cs:105` clears `States` only. `PreviousStates`, `Semaphores`, `StateInitializationLocks` and `StateInitializationTasks` keep their entries, no `CancelOperations` or `Dispose` runs, and a later `GetState` creates a new instance while `StateInitializationPreProcessor` may still await the old initialization task. `RemoveState` (`:74-100`) does cancel and dispose the semaphore, so the two paths disagree.
- Suggestion: implement `Reset` as `RemoveState` for every key under the per-type lock, or document `Reset` as test-only and move it off `IStore`.

### M5. `IStore` exposes implementation details: `GetSemaphore`, `StateInitializationTasks`, `GetState(Type)` returning `object`

- `source/timewarp-state/store/i-store.cs:27` (`GetState(Type)` returns `object` although every implementation returns `IState`), `:29` (`GetSemaphore`), and `:37` (`StateInitializationTasks`). `GetSemaphore` returns null until the state exists, is used by exactly one caller (`route-state.push-route-info.cs:34`), and is no longer used by the transaction behavior. `StateInitializationTasks` is a mutable `ConcurrentDictionary<string, Task>` keyed by type full name, which `documentation/topics/persistence.md` tells users to await directly.
- Consequence of the `GetSemaphore` null contract: when a host sets `UseStateTransactionBehavior = false`, nothing creates `RouteState` before the handler runs, `GetSemaphore` returns null and `PushRouteInfo` silently does nothing (`push-route-info.cs:35`).
- Suggestion: remove `GetSemaphore` once H2 lands; replace the dictionary with `Task WaitForInitializationAsync<TState>()`; return `IState` from `GetState(Type)`. These are breaking changes that fit the 12.0 beta window.

### M6. `PushRouteInfo` reads the page title with JavaScript `eval`

- `source/timewarp-state-plus/features/routing/route-state/route-state.push-route-info.cs:41`: `JsRuntime.InvokeAsync<string>("eval", cancellationToken, "document.title")`. Any host with a Content Security Policy that omits `unsafe-eval` (the normal production setting) throws here, and the interop call is not guarded (BL0016 in the build log).
- Suggestion: add a `getDocumentTitle` export to the Plus or Blazor script module (`download-file.js` shows Plus already ships JS), or pass the title from `TwPageTitle`, which already knows it.

### M7. `ActiveActionBehavior` completes tracking with the request token, so a cancelled action leaves the busy indicator on

- `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs:72`: the `finally` sends `CompleteProcessing` with `cancellationToken`. If the token is already cancelled (which is exactly when `next` threw `OperationCanceledException`), the completion send is itself cancelled, the action stays in `ActiveActionList`, and `IsActive` is stuck true for the scope.
- Suggestion: send the completion with `CancellationToken.None`, mirroring what `StateTransactionBehavior` does for `ExceptionNotification`. Add a test with a pre-cancelled token to `active-action-behavior-tests.cs`.

### M8. `TimerState` publishes into a scoped pipeline from timer threads, through `async void`

- `source/timewarp-state-plus/features/timers/timer-state/timer-state.cs:82,102`: `System.Timers.Timer.Elapsed` fires on a thread-pool thread (Blazor Server) and `OnTimerElapsed` is `async void`. An exception from any notification handler is unobserved and tears down the process; the publish runs outside the circuit's synchronization context and races with user actions on the same scoped `Store`. On WASM the timer runs on the single thread, so only the `async void` part applies there.
- Suggestion: make the callback `async Task` wrapped in a try/catch that logs, and dispatch through the circuit (expose a hook, or document that `TimerElapsedNotification` handlers must marshal with `InvokeAsync`). Consider `PeriodicTimer` or `ITimer` from `TimeProvider` for testability.

### M9. Persistence: two public enum values do nothing, and the service's dependencies disagree with the behavior's

- `source/timewarp-state/features/persistence/persistent-state-method.cs` exposes `PreRender` and `Server`; `persistent-state-post-processor.cs:92` and `persistence-service.cs:107-108` are `TODO` no-ops for both. A `[PersistentState(PersistentStateMethod.Server)]` state compiles, passes the policies, and silently never persists.
- `PersistenceService` (`persistence-service.cs:33`) requires both `ISessionStorageService` and `ILocalStorageService` through its constructor, while `PersistentStatePostProcessor` treats each as optional and logs when missing. A host that registers only `AddBlazoredLocalStorage` fails DI resolution of the load path with an unrelated-looking error.
- `IPersistenceService` has no registration helper; `AddTimeWarpStateRouting` exists for routing but persistence requires the host to write `AddScoped<IPersistenceService, PersistenceService>()` plus an assembly attribute (readme and `documentation/topics/persistence.md`).
- Suggestion: remove `PreRender` and `Server` until implemented (beta window), make the storage services optional in `PersistenceService` with the same warning, and add `AddTimeWarpStatePersistence()` that registers the service.

### M10. The "test" assembly-name sniff is still the guard on public and internal members

- `source/timewarp-state/state/state.cs:86`: `ThrowIfNotTestAssembly` passes any caller whose assembly full name contains "test" (ordinal, ignore case), unless `StateTestOptions.Enable()` was called. The Design region and `documentation/release-notes/release12.0.0-beta.5.md` say the sniff is removed "in the following release"; it is still present six betas later.
- Guarded members include `ThemeState.Initialize(Theme)` (`theme-state.debug.cs:41`), which is public. An assembly named `Contoso.Latest` or `Acme.Testimonials` passes the check in production. `Assembly.GetCallingAssembly()` is also unreliable under JIT inlining without `[MethodImpl(MethodImplOptions.NoInlining)]`.
- Suggestion: drop the sniff and require `StateTestOptions.Enable()`, or make the guarded `Initialize` overloads `internal` with `InternalsVisibleTo` for the test assemblies and delete the guard altogether.

### M11. `AddTimeWarpStateBlazor` registers an `HttpClient` on Blazor Server

- `source/timewarp-state-blazor/extensions/service-collection-extensions.add-timewarp-state-blazor.cs:47`: a scoped `new HttpClient { BaseAddress = NavigationManager.BaseUri }` is `TryAdd`ed when not running in the browser. A state library registering the app's `HttpClient` is a surprising side effect: it hides a missing host registration, bypasses `IHttpClientFactory`, and creates one `HttpClient` per circuit. The 12.0.0-beta.10 migration guide documents it, which at least makes it discoverable.
- Suggestion: remove it in the beta window and document the one-line host registration, or move it to an explicit `AddTimeWarpStateServerHttpClient()`.

### M12. `JsonRequestHandler` serializes `JsonSerializerOptions` itself on every scope construction

- `source/timewarp-state-blazor/features/javascript-interop/json-request-handler.cs:50`: the constructor logs `JsonSerializer.Serialize(JsonSerializerOptions)` at Debug level, unconditionally. That reflects over the options object (including converter instances) on every circuit or WASM start, can throw for options that hold non-serializable converters, and the string is discarded when Debug logging is off.
- Suggestion: delete the call, or guard with `Logger.IsEnabled(LogLevel.Debug)` and log a hand-picked subset of properties.

### M13. Public diagnostic ids are inconsistent and three are not prefixed

- `TWS001` (`state-implementation-analyzer.cs:17`) sits next to `TWS0001`..`TWS0008`; the difference is one zero and is easy to misconfigure in `.editorconfig`. `StateInheritanceTypeArgumentRule`, `StateSealedClassRule` and `StateReadOnlyPublicPropertiesRule` (`state-inheritance-analyzer.cs:21`, `state-read-only-public-properties-analyzer.cs`) are prose ids without a prefix, so they cannot be grouped or suppressed by family. All are listed as unshipped in `AnalyzerReleases.Unshipped.md`, so renaming now is cheap; after 12.0 it is a breaking change.
- `StateReadOnlyPublicPropertiesAnalyzer` is syntax-only: it flags `internal set`, ignores `init`, and does not see positional record properties.
- Suggestion: renumber to `TWS0009`..`TWS0012` before 12.0 GA, move the entries to `AnalyzerReleases.Shipped.md` at release, and make the read-only rule symbol-based.

### M14. Test coverage gaps worth closing before 12.0

- No test for interleaved actions on one state (H2), for `StateInitializationPreProcessor`, for `ReduxDevToolsBehavior`/`ReduxDevToolsInterop`, for `RouteState.ChangeRoute`, for `TimerState` elapsed and restart, for `TwPageTitle`/`TimeWarpPageRenderNotifier`, or for `JsonRequestHandler.InitAsync`/dispose lifecycle (task 063 fixed a leak there without a regression test).
- `tests/test-app-end-to-end-tests/persistence-test-page-tests.cs:48` is `[Ignore]`d; the only browser proof of persistence is gone. `sample-test.cs` is a Playwright demo against playwright.dev and accounts for two permanent skips.
- `tests/timewarp-state-tests/cloning/deep-cloner-tests.cs` is named and documented after the deleted `DeepCloner`; its Purpose region still says "AnyClone replacement".
- No packaging test asserts package contents or dependencies (H1 would have been caught).
- Suggestion: add the missing unit tests listed above, un-ignore the persistence e2e once Chromium installs in CI (it does on `ubuntu-latest`), delete `sample-test.cs`, rename the cloner test file, and add a nuspec allow-list check to `dev pack`.

### M15. Docs, readmes and repo metadata have drifted from the code

- `claude.md:127` says Nullable is disabled project-wide; `Directory.Build.props` enables it. `claude.md:55` references `./BuildAndPackageAnalyzer.ps1` and `claude.md:132` says lock files are enabled; neither file nor any `packages.lock.json` exists, and `./LocalNugetFeed` is now `artifacts/packages`.
- `readme.md:2` badge points at `workflows/release-build.yml` (only `workflow.yml` exists); `readme.md:13` says dotnet 10.0 while the target is net11.0; `readme.md:37` uses a docfx `xref:` link that does not resolve on GitHub; the `Blazor-State` download badge is for the retired package id.
- `source/timewarp-state-plus/readme.md:1` says dotnet 8.0, `:17` loads `Assets/Logo.svg` (the folder is `assets/logo.svg`, case-sensitive on GitHub), `:47-49` advertise `InputColor`, which lives in `tests/test-app/test-app-client/features/color/`, not in Plus. This readme is the NuGet package readme.
- `documentation/partials/getting-started.md:8` links to `tree/master/Samples` (folder is `samples`).
- `timewarp-state.slnx:69` lists `Build\documentation.yml`, which does not exist, and its solution folders are misnamed (`03-ServerSide` holds sample 00 server; `/source/` holds only the Blazor project while the other source projects are at root).
- `source/timewarp-state/event-ids.cs:19,23` assign `104` to both `Store_SetState` and `LoadStatesFromJson`.
- Suggestion: one docs pass with the list above; add `ai-context.md` as the single place that summarizes packages and point `claude.md` at it.

## Low

### L1. `StateTransactionBehavior` is bypassed for `UseStateTransactionBehavior = false` but still clones nothing and still gates render; document or remove the option

`timewarp-state-options.cs:29`. The option is honored, but with it off the store never creates the state before the handler (see M5) and `GetPreviousState` is never populated, so `RegisterRenderTrigger` always renders. Either document those consequences or drop the option.

### L2. `TimeWarpStateOptions` surface

`timewarp-state-options.cs:31` `UseRouting` is unused anywhere in the repo. `:49` exposes the `IServiceCollection` as a public readonly field (convention and encapsulation). `Assemblies` defaults to `Assembly.GetCallingAssembly()` (`service-collection-extensions.add-timewarp-state.cs:41`), which is inlining-sensitive; mark `AddTimeWarpState` `[MethodImpl(NoInlining)]` or require the assembly explicitly. `EnsureStates` (`:85`) uses `assembly.GetTypes()`, which throws `ReflectionTypeLoadException` on a partially loadable assembly; catch and use `ex.Types`.

### L3. `ReRender` and the hidden `StateHasChanged` are fire-and-forget

`timewarp-state-component.register-render-trigger.cs:42,64` discard the `InvokeAsync` task, so an `ObjectDisposedException` after dispose or a render exception is unobserved. `StateHasChanged` is hidden with `new` and made asynchronous, which changes semantics for callers that expect the base synchronous behavior. Observe the task (`_ = InvokeAsync(...).ContinueWith(log, OnlyOnFaulted)`) or return it.

### L4. `TimeWarpStateComponent` is declared `abstract` in one partial file only

`check-complex-parameter-changed.cs:16` says `public abstract partial class`; `timewarp-state-component.cs` says `public partial class` and its XML doc calls it "a non required base class". Partial modifiers merge, so it is abstract; make the declarations agree. `TimeWarpStateDevComponent` (`timewarp-state-dev-component.cs:21`) has a protected constructor but is not abstract.

### L5. Blazor interop warnings

BL0010 and BL0016 in the Release build: `redux-dev-tools-interop.cs:52,61,71` and `json-request-handler.cs:162` use `InvokeAsync<object>` where `InvokeVoidAsync` is meant, and no interop call is guarded for `JSDisconnectedException` during circuit teardown (the dispose path is, the init path is not). `ReduxDevToolsBehavior` constructs a `Regex` per scoped instance per closed generic (`redux-dev-tools-behavior.cs:47`); make it `static readonly` and compiled. `TraceFilterExpression` (`redux-dev-tools-options.cs:25`) does not escape the dots in `TimeWarp.Mediator`.

### L6. Generated ActionSet method API

`action-set-method-generator.cs:118` emits `CancellationToken? externalCancellationToken = null` (a nullable struct) instead of the idiomatic `CancellationToken cancellationToken = default`. `:148` returns the literal namespace `Global` for types in the global namespace, which emits `namespace Global;` and breaks compilation for such a state. `:82` reports an `SG002` Info diagnostic per generated file ("Unique hint name"), which is build noise and collides with the `SG` prefix other generators use. `IsCandidateClass` matches on the identifier suffix only, so a class named `MyActionSet` nested in any class, state or not, gets a method generated for it. The catalog generator's `GetDefaultName` uses culture-sensitive `EndsWith("State")` while the analyzer uses `Ordinal`.

### L7. csproj and build props leftovers

`source/timewarp-state/timewarp-state.csproj:29` `TargetsForTfmSpecificBuildOutput` self-assignment is a no-op; `:34` conditions on configuration `DefaultReduxEnabled`, which is not in `<Configurations>`; `:70` `InternalsVisibleTo Test.App.Client.Integration.Tests` names an assembly that does not exist; `:86-88` include `build\**` folders that do not exist. `source/Directory.Build.props:9` hard-codes `12.0.0-beta.11` next to `msbuild/repository.props:17`; `<Version>$(TimeWarpStateVersion)</Version>` removes the need for the `AssertVersionSsot` pipeline step. `NU1510` for `Microsoft.Extensions.Logging.Abstractions` in `timewarp-state.csproj` and `timewarp-state-telemetry.csproj` and `System.Net.Http.Json` in `test-app-contracts.csproj`: remove the references.

### L8. `InvalidRequestTypeException` carries the obsolete serialization constructor

`invalid-request-type-exception.cs:37`: `[Serializable]` plus a `SerializationInfo` constructor that does not call the base constructor. Binary serialization of exceptions is obsolete (SYSLIB0051) and the constructor as written loses `Message`. Delete both.

### L9. Small correctness nits in Plus

`assembly-extensions.cs` `ShortHash => CommitHash?[^6..]` throws for a hash shorter than six characters. `CompleteProcessingActionSet.Handler` throws `InvalidOperationException` when the action is missing, which the transaction behavior turns into an `ExceptionNotification` to the app for what is internal bookkeeping. `ActionTrackingState.ActiveActions` allocates a new `ReadOnlyCollection` wrapper per read; cache it. `MultiTimerPostProcessor` sends `ResetTimersOnActivity` after every request, including requests whose host never registered `TimerState` in `Assemblies` (the first send then fails DI resolution).

### L10. CI trigger paths omit files that change the build

`.github/workflows/workflow.yml` path filters do not include `.editorconfig`, `BannedSymbols.txt`, `msbuild/external-sources.globalconfig` (covered by `msbuild/**`) or `bin/dev*`. A severity change in `.editorconfig` can turn a warning into an error without CI running. Add `.editorconfig` and `BannedSymbols.txt`.

### L11. TypeScript hygiene

`tsconfig.json` has `strict: false` and all strict flags off; `redux-dev-tools.ts` types everything as `any`, writes globals through `window[...]`, and `MessageHandler` fires `DispatchRequest(...).then()` without a catch. `TimeWarp.State.Blazor.lib.module.ts` logs at `info` on every start. Turn on `strict`, add a `.catch` that logs, and lower the startup logs to debug.

## Nit

- `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs` declares `ActiveActionBehavior`; the file, the readme and the Plus assembly marker call it `ActionTrackingBehavior`.
- `persistent-state-post-processor.cs` names the `ILocalStorageService` field `LocalSessionStorageService`.
- `source/timewarp-state/subscriptions.cs:65` overrides `Equals`/`GetHashCode` with reference-based logic that is identical to the default; delete both.
- `source/timewarp-state/store/store.cs:14-16` has an empty `<summary>`; `i-state.cs` keeps four commented-out members.
- `StartHandler_RequestHandled` event id is declared and never used; `CommitHandler` logs under `JumpToStateHandler_RequestReceived`.
- `ReduxDevToolsOptions.TFeatures` and the "serialize is not implemented" comment block; `BaseJsonRequest`/`JsonRequest<T>` are shipped public types the library does not use (Design region says so).
- 62 `TW0007` warnings (local `using` directives) in the Release build, 20 of them in `source/` (`timer-state.cs`, `assembly-extensions.cs`, `service-collection-extensions.cs` in Plus, `handler-must-not-send-action-analyzer.cs`, `action-catalog-arguments.cs`, `policies.state-policy.cs`); 12 `RS0030` `Console` uses in the test app; 42 nullable warnings in `tests/test-app/test-app-client/tests/clone-provider-tests.cs`.
- `kanban/backlog/scratch/todo.md` still lists "Convert js to ts" as the only done item and "Review TODOs in source" as open. Five `TODO` comments remain under `source/`. Four are the persistence no-ops in M9 (`persistent-state-post-processor.cs:93` and `:130`, `persistence-service.cs:107-108`). The fifth is a file-layout note at `source/timewarp-state-policies/policies.action-policy.cs:15`.
- `documentation/toc.yml` lists the topics folder and then each topic again at top level.

## Strengths worth keeping

- Compile-time pipeline weaving with explicit cross-assembly order numbers in `assembly-marker.cs` files, and the `ClientPipeline`/`ServerPipeline` split with TWM004 enforcing it.
- The clone planner's refusal to fall back to reflection, its reference-assembly visibility rules, cycle handling through `CloneMap`, and the diagnostic that names the member and the root state.
- Opt-in, name-indexed JavaScript dispatch that never calls `Type.GetType`, with fail-closed logging that caps attacker-controlled strings.
- `TelemetryBehavior`: `HasListeners` gate, `JsonTypeInfo` from caller options, no `new JsonSerializerOptions()`, snapshot failures never fail the action.
- `Subscriptions`: two indexes under one lock, snapshot outside the lock, dead `WeakReference` cleanup after the loop.
- `Store.GetState` double-checked per-type lock with insertion only after `Initialize` succeeds, and the concurrent first-access tests that pin it.
- Purpose and Design regions on every file, enforced by TWA0004, and kanban Results sections that record measured package sizes, dependency lists and test counts.
- The dev CLI pipeline with version SSOT assertion, trusted publishing probe mode, and promote-on-release rather than rebuild.
