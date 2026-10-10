# Task 109: Fable review Medium findings M1-M15 (batched)

## Description

Batch of the fifteen Medium findings from Claude Fable's full codebase review, task 102
(`kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md`, PR #629,
`00ade373`). One checklist item per finding. The High findings are separate tasks: 105 (H1), 106 (H2),
107 (H3), 108 (H4).

Filed 2026-10-10 at Steven's request (relayed by Amina). Not launched. This is a batch for tracking;
per the one-PR-per-task rule, split items into child tasks (`ganda kanban reserve --parent 109`) before
implementing rather than doing them all in one PR.

## Order

Fable's recommended order (verbatim, `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md`, Overall assessment):

> Recommended order of attack: the package dependency (one line), the transaction rollback guard (one `ReferenceEquals` check), a `[CloneShared]`-style attribute or equivalent for injected members, then the DevTools and persistence dead paths, then `IsAotCompatible` on the two remaining runtime packages.

Filed tasks in that order: 105 (H1, package dependency) -> 106 (H2, rollback guard) -> 107 (H3, share-by-reference attribute) -> 104 and 109 M1/M9 (dead DevTools and persistence paths) -> 109 M2 (AOT checks on Blazor and Plus). 108 (H4) and the remaining Medium items are not in Fable's ordered list; schedule them after 107.

## Checklist

- [x] M1. Closed here with a recorded reason: task 104 still owns finish-vs-delete for Redux DevTools time travel (Steven decides). Companion inspector remains task 103. `LoadStatesFromJson` was not deleted on this id.
- [x] M2. `IsAotCompatible` on `TimeWarp.State.Blazor` and `TimeWarp.State.Plus`, IL suppressions with rooting justification, `PublishTrimmed` smoke of `samples/05-persistence` in `dev verify-samples`
- [x] M3. Rethrow `OperationCanceledException` after rollback; `TimeWarpStateOptions.RethrowHandlerExceptions` (default false); semantics in `documentation/topics/state-transactions.md`
- [x] M4. `Store.Reset` removes every key through the same cancel/dispose path as `RemoveState`
- [x] M5. `IStore`: `GetSemaphore` and the public initialization dictionary removed; `WaitForInitializationAsync<TState>()` and `FindInitializationTask`; `GetState(Type)` returns `IState`
- [x] M6. `PushRouteInfo` reads `document.title` from `wwwroot/js/document-title.js` (`getDocumentTitle`); disconnect returns an empty title
- [x] M7. Action tracking sends `CompleteProcessing` with `CancellationToken.None`; pre-cancelled token test
- [x] M8. `TimerState` posts an `async Task` through the captured circuit `SynchronizationContext`, with try/catch logging (event 1405). `System.Timers.Timer` kept.
- [x] M9. `PreRender`/`Server` removed; storage services optional on `PersistenceService`; `AddTimeWarpStatePersistence()`
- [x] M10. Assembly-name sniff removed. Guarded members require `StateTestOptions.Enable()`
- [x] M11. `AddTimeWarpStateBlazor` no longer registers `HttpClient`. Hosts register it. No opt-in helper.
- [x] M12. `JsonRequestHandler` logs a hand-picked options subset only when Debug is enabled
- [x] M13. Diagnostics renumbered to `TWS0009`..`TWS0012`. Read-only rule is symbol-based. Entries stay in `AnalyzerReleases.Unshipped.md` until 12.0 ships.
- [x] M14. Unit gaps closed. Persistence e2e un-ignored. `sample-test.cs` deleted. Cloner tests renamed to `clone-graph-tests.cs`. Nuspec allow-list already landed on task 105. Interleaved rollback tests already landed on task 106. Redux DevTools tests stay with task 104.
- [x] M15. Docs, readmes, slnx, and the duplicate event id 104 (`LoadStatesFromJson` is 106, `LoadStateFromJson` is 107)

## Acceptance criteria

- Every checklist item is either done (via a child task PR) or explicitly closed with a recorded reason
  (e.g. M1 resolved by task 104).
- Each child PR: build warning-free for touched projects, tests green, `ganda repo audit` clean.

## Findings (verbatim)

### M1 (see task 104)

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 57-62 (PR #629, merged as `00ade373`):

> ### M1. Redux DevTools time travel is unreachable from the shipped JavaScript, but keeps the core package's only trim-unsafe reflection
>
> - `source/timewarp-state-blazor/wwwroot/typescript/redux-dev-tools.ts:61` onward maps every `DISPATCH` payload (`COMMIT`, `JUMP_TO_STATE`, `IMPORT_STATE`, ...) to `undefined`, and `MessageHandler` ignores unmapped types. Only `START` reaches .NET. `CommitHandler` and `IReduxDevToolsStore.LoadStatesFromJson` are therefore dead at runtime.
> - `source/timewarp-state/store/store.redux-dev-tools.cs:43-71` carries seven `UnconditionalSuppressMessage` attributes (IL2026, IL2070, IL2072, IL2075, IL3050) to scan `AppDomain.CurrentDomain.GetAssemblies()` and invoke `Hydrate` through `MethodInfo.Invoke`. Together with `EnsureStates` (`service-collection-extensions.add-timewarp-state.cs:85`) these are the only reflection sites left after task 097, and 097's own requirement was "no reflection in TimeWarp.State at all, used or unused".
> - `State<T>.Hydrate` (`state/state.cs:62`) throws `NotImplementedException` by default; only five states in the repo override it.
> - Suggestion: pick one. Either finish time travel (map `JUMP_TO_STATE`/`JUMP_TO_ACTION`, have the clone generator emit `Hydrate` so the reflection goes away, and test it), or delete `LoadStatesFromJson`, `Hydrate`, `IState<TState>`, `CommitHandler` and the seven suppressions now. The scratch backlog already lists splitting DevTools into its own package; deleting is consistent with that.

### M2

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 64-69 (PR #629, merged as `00ade373`):

> ### M2. `IsAotCompatible` is set on two of four runtime packages; the reflection that trimmed WASM apps will hit is in the other two
>
> - `source/timewarp-state/timewarp-state.csproj` and `source/timewarp-state-telemetry/timewarp-state-telemetry.csproj` set `IsAotCompatible`. `source/timewarp-state-blazor/timewarp-state-blazor.csproj` and `source/timewarp-state-plus/timewarp-state-plus.csproj` do not, so no IL warnings are produced for them (the Release build log confirms zero IL warnings overall).
> - Reflection in those two packages: `TimeWarpStateComponent` parameter discovery via `GetProperties`/`GetMethod` (`check-complex-parameter-changed.cs`), `Expression.Compile` in `RegisterRenderTrigger` (`register-render-trigger.cs:153`), `JsonSerializer.Deserialize(string, Type, options)` and `Activator.CreateInstance` in `JsonRequestHandler` (`json-request-handler.cs:122`), reflection-based `JsonSerializer.Serialize(object, Type)` in `PersistentStatePostProcessor` and `PersistenceService`, and `GetCustomAttribute` in several Plus behaviors. Blazor WebAssembly publishes with trimming on by default, and the trimmer only warns about assemblies that opt into analysis.
> - No trimmed or AOT publish is in the CI pipeline (`tools/dev-cli/endpoints/workflow-command.cs`: build, test, e2e, pack, verify-samples). `samples/04-telemetry` has `EnableTrimAnalyzer` and the dev CLI itself is `PublishAot`, which is not the same thing.
> - Suggestion: set `IsAotCompatible` (or at least `IsTrimmable` + `EnableTrimAnalyzer`) on Blazor and Plus, annotate or suppress with justification, and add a `PublishTrimmed` smoke of `samples/05-persistence` to `dev verify-samples` with `TrimmerSingleWarn=false`.

### M3

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 71-75 (PR #629, merged as `00ade373`):

> ### M3. Transaction behavior swallows handler failures and cancellation, so callers cannot observe either
>
> - `state-transaction-behavior.cs:144` returns `default!` after rollback, for both exceptions and `OperationCanceledException`. `await counterState.IncrementCount(ct)` completes normally when the handler threw or when `ct` was cancelled. Cancellation is the worse case: cooperative cancellation contracts in .NET expect `OperationCanceledException` to propagate, and callers chaining actions will continue as if the first succeeded.
> - `ExceptionNotification` covers the error case for apps that subscribe; nothing covers cancellation.
> - Suggestion: rethrow `OperationCanceledException` after rollback (it is not a failure, as the Design region says, so publishing is right to skip, but swallowing is not). Consider a `TimeWarpStateOptions` switch to rethrow handler exceptions after publishing, for hosts that want `Send` to fault. Document the current semantics in `documentation/topics` either way; today they are only in a code comment.

### M4

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 77-80 (PR #629, merged as `00ade373`):

> ### M4. `Store.Reset` is a partial reset
>
> - `source/timewarp-state/store/store.cs:105` clears `States` only. `PreviousStates`, `Semaphores`, `StateInitializationLocks` and `StateInitializationTasks` keep their entries, no `CancelOperations` or `Dispose` runs, and a later `GetState` creates a new instance while `StateInitializationPreProcessor` may still await the old initialization task. `RemoveState` (`:74-100`) does cancel and dispose the semaphore, so the two paths disagree.
> - Suggestion: implement `Reset` as `RemoveState` for every key under the per-type lock, or document `Reset` as test-only and move it off `IStore`.

### M5

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 82-86 (PR #629, merged as `00ade373`):

> ### M5. `IStore` exposes implementation details: `GetSemaphore`, `StateInitializationTasks`, `GetState(Type)` returning `object`
>
> - `source/timewarp-state/store/i-store.cs:27` (`GetState(Type)` returns `object` although every implementation returns `IState`), `:29` (`GetSemaphore`), and `:37` (`StateInitializationTasks`). `GetSemaphore` returns null until the state exists, is used by exactly one caller (`route-state.push-route-info.cs:34`), and is no longer used by the transaction behavior. `StateInitializationTasks` is a mutable `ConcurrentDictionary<string, Task>` keyed by type full name, which `documentation/topics/persistence.md` tells users to await directly.
> - Consequence of the `GetSemaphore` null contract: when a host sets `UseStateTransactionBehavior = false`, nothing creates `RouteState` before the handler runs, `GetSemaphore` returns null and `PushRouteInfo` silently does nothing (`push-route-info.cs:35`).
> - Suggestion: remove `GetSemaphore` once H2 lands; replace the dictionary with `Task WaitForInitializationAsync<TState>()`; return `IState` from `GetState(Type)`. These are breaking changes that fit the 12.0 beta window.

### M6

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 88-91 (PR #629, merged as `00ade373`):

> ### M6. `PushRouteInfo` reads the page title with JavaScript `eval`
>
> - `source/timewarp-state-plus/features/routing/route-state/route-state.push-route-info.cs:41`: `JsRuntime.InvokeAsync<string>("eval", cancellationToken, "document.title")`. Any host with a Content Security Policy that omits `unsafe-eval` (the normal production setting) throws here, and the interop call is not guarded (BL0016 in the build log).
> - Suggestion: add a `getDocumentTitle` export to the Plus or Blazor script module (`download-file.js` shows Plus already ships JS), or pass the title from `TwPageTitle`, which already knows it.

### M7

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 93-96 (PR #629, merged as `00ade373`):

> ### M7. `ActiveActionBehavior` completes tracking with the request token, so a cancelled action leaves the busy indicator on
>
> - `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs:72`: the `finally` sends `CompleteProcessing` with `cancellationToken`. If the token is already cancelled (which is exactly when `next` threw `OperationCanceledException`), the completion send is itself cancelled, the action stays in `ActiveActionList`, and `IsActive` is stuck true for the scope.
> - Suggestion: send the completion with `CancellationToken.None`, mirroring what `StateTransactionBehavior` does for `ExceptionNotification`. Add a test with a pre-cancelled token to `active-action-behavior-tests.cs`.

### M8

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 98-101 (PR #629, merged as `00ade373`):

> ### M8. `TimerState` publishes into a scoped pipeline from timer threads, through `async void`
>
> - `source/timewarp-state-plus/features/timers/timer-state/timer-state.cs:82,102`: `System.Timers.Timer.Elapsed` fires on a thread-pool thread (Blazor Server) and `OnTimerElapsed` is `async void`. An exception from any notification handler is unobserved and tears down the process; the publish runs outside the circuit's synchronization context and races with user actions on the same scoped `Store`. On WASM the timer runs on the single thread, so only the `async void` part applies there.
> - Suggestion: make the callback `async Task` wrapped in a try/catch that logs, and dispatch through the circuit (expose a hook, or document that `TimerElapsedNotification` handlers must marshal with `InvokeAsync`). Consider `PeriodicTimer` or `ITimer` from `TimeProvider` for testability.

### M9

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 103-108 (PR #629, merged as `00ade373`):

> ### M9. Persistence: two public enum values do nothing, and the service's dependencies disagree with the behavior's
>
> - `source/timewarp-state/features/persistence/persistent-state-method.cs` exposes `PreRender` and `Server`; `persistent-state-post-processor.cs:92` and `persistence-service.cs:107-108` are `TODO` no-ops for both. A `[PersistentState(PersistentStateMethod.Server)]` state compiles, passes the policies, and silently never persists.
> - `PersistenceService` (`persistence-service.cs:33`) requires both `ISessionStorageService` and `ILocalStorageService` through its constructor, while `PersistentStatePostProcessor` treats each as optional and logs when missing. A host that registers only `AddBlazoredLocalStorage` fails DI resolution of the load path with an unrelated-looking error.
> - `IPersistenceService` has no registration helper; `AddTimeWarpStateRouting` exists for routing but persistence requires the host to write `AddScoped<IPersistenceService, PersistenceService>()` plus an assembly attribute (readme and `documentation/topics/persistence.md`).
> - Suggestion: remove `PreRender` and `Server` until implemented (beta window), make the storage services optional in `PersistenceService` with the same warning, and add `AddTimeWarpStatePersistence()` that registers the service.

### M10

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 110-114 (PR #629, merged as `00ade373`):

> ### M10. The "test" assembly-name sniff is still the guard on public and internal members
>
> - `source/timewarp-state/state/state.cs:86`: `ThrowIfNotTestAssembly` passes any caller whose assembly full name contains "test" (ordinal, ignore case), unless `StateTestOptions.Enable()` was called. The Design region and `documentation/release-notes/release12.0.0-beta.5.md` say the sniff is removed "in the following release"; it is still present six betas later.
> - Guarded members include `ThemeState.Initialize(Theme)` (`theme-state.debug.cs:41`), which is public. An assembly named `Contoso.Latest` or `Acme.Testimonials` passes the check in production. `Assembly.GetCallingAssembly()` is also unreliable under JIT inlining without `[MethodImpl(MethodImplOptions.NoInlining)]`.
> - Suggestion: drop the sniff and require `StateTestOptions.Enable()`, or make the guarded `Initialize` overloads `internal` with `InternalsVisibleTo` for the test assemblies and delete the guard altogether.

### M11

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 116-119 (PR #629, merged as `00ade373`):

> ### M11. `AddTimeWarpStateBlazor` registers an `HttpClient` on Blazor Server
>
> - `source/timewarp-state-blazor/extensions/service-collection-extensions.add-timewarp-state-blazor.cs:47`: a scoped `new HttpClient { BaseAddress = NavigationManager.BaseUri }` is `TryAdd`ed when not running in the browser. A state library registering the app's `HttpClient` is a surprising side effect: it hides a missing host registration, bypasses `IHttpClientFactory`, and creates one `HttpClient` per circuit. The 12.0.0-beta.10 migration guide documents it, which at least makes it discoverable.
> - Suggestion: remove it in the beta window and document the one-line host registration, or move it to an explicit `AddTimeWarpStateServerHttpClient()`.

### M12

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 121-124 (PR #629, merged as `00ade373`):

> ### M12. `JsonRequestHandler` serializes `JsonSerializerOptions` itself on every scope construction
>
> - `source/timewarp-state-blazor/features/javascript-interop/json-request-handler.cs:50`: the constructor logs `JsonSerializer.Serialize(JsonSerializerOptions)` at Debug level, unconditionally. That reflects over the options object (including converter instances) on every circuit or WASM start, can throw for options that hold non-serializable converters, and the string is discarded when Debug logging is off.
> - Suggestion: delete the call, or guard with `Logger.IsEnabled(LogLevel.Debug)` and log a hand-picked subset of properties.

### M13

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 126-130 (PR #629, merged as `00ade373`):

> ### M13. Public diagnostic ids are inconsistent and three are not prefixed
>
> - `TWS001` (`state-implementation-analyzer.cs:17`) sits next to `TWS0001`..`TWS0008`; the difference is one zero and is easy to misconfigure in `.editorconfig`. `StateInheritanceTypeArgumentRule`, `StateSealedClassRule` and `StateReadOnlyPublicPropertiesRule` (`state-inheritance-analyzer.cs:21`, `state-read-only-public-properties-analyzer.cs`) are prose ids without a prefix, so they cannot be grouped or suppressed by family. All are listed as unshipped in `AnalyzerReleases.Unshipped.md`, so renaming now is cheap; after 12.0 it is a breaking change.
> - `StateReadOnlyPublicPropertiesAnalyzer` is syntax-only: it flags `internal set`, ignores `init`, and does not see positional record properties.
> - Suggestion: renumber to `TWS0009`..`TWS0012` before 12.0 GA, move the entries to `AnalyzerReleases.Shipped.md` at release, and make the read-only rule symbol-based.

### M14

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 132-138 (PR #629, merged as `00ade373`):

> ### M14. Test coverage gaps worth closing before 12.0
>
> - No test for interleaved actions on one state (H2), for `StateInitializationPreProcessor`, for `ReduxDevToolsBehavior`/`ReduxDevToolsInterop`, for `RouteState.ChangeRoute`, for `TimerState` elapsed and restart, for `TwPageTitle`/`TimeWarpPageRenderNotifier`, or for `JsonRequestHandler.InitAsync`/dispose lifecycle (task 063 fixed a leak there without a regression test).
> - `tests/test-app-end-to-end-tests/persistence-test-page-tests.cs:48` is `[Ignore]`d; the only browser proof of persistence is gone. `sample-test.cs` is a Playwright demo against playwright.dev and accounts for two permanent skips.
> - `tests/timewarp-state-tests/cloning/deep-cloner-tests.cs` is named and documented after the deleted `DeepCloner`; its Purpose region still says "AnyClone replacement".
> - No packaging test asserts package contents or dependencies (H1 would have been caught).
> - Suggestion: add the missing unit tests listed above, un-ignore the persistence e2e once Chromium installs in CI (it does on `ubuntu-latest`), delete `sample-test.cs`, rename the cloner test file, and add a nuspec allow-list check to `dev pack`.

### M15

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 140-148 (PR #629, merged as `00ade373`):

> ### M15. Docs, readmes and repo metadata have drifted from the code
>
> - `claude.md:127` says Nullable is disabled project-wide; `Directory.Build.props` enables it. `claude.md:55` references `./BuildAndPackageAnalyzer.ps1` and `claude.md:132` says lock files are enabled; neither file nor any `packages.lock.json` exists, and `./LocalNugetFeed` is now `artifacts/packages`.
> - `readme.md:2` badge points at `workflows/release-build.yml` (only `workflow.yml` exists); `readme.md:13` says dotnet 10.0 while the target is net11.0; `readme.md:37` uses a docfx `xref:` link that does not resolve on GitHub; the `Blazor-State` download badge is for the retired package id.
> - `source/timewarp-state-plus/readme.md:1` says dotnet 8.0, `:17` loads `Assets/Logo.svg` (the folder is `assets/logo.svg`, case-sensitive on GitHub), `:47-49` advertise `InputColor`, which lives in `tests/test-app/test-app-client/features/color/`, not in Plus. This readme is the NuGet package readme.
> - `documentation/partials/getting-started.md:8` links to `tree/master/Samples` (folder is `samples`).
> - `timewarp-state.slnx:69` lists `Build\documentation.yml`, which does not exist, and its solution folders are misnamed (`03-ServerSide` holds sample 00 server; `/source/` holds only the Blazor project while the other source projects are at root).
> - `source/timewarp-state/event-ids.cs:19,23` assign `104` to both `Store_SetState` and `LoadStatesFromJson`.
> - Suggestion: one docs pass with the list above; add `ai-context.md` as the single place that summarizes packages and point `claude.md` at it.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)
- Implementer: Grok session 01a12712-c358-7502-b742-e3d0a464b84e (2026-10-11)
- Review oracle: Claude Opus 5.5 (ganda task work, 2026-10-11), effort 3, roster general (Sonnet subagent)
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-10T19:33:55Z
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-10T20:26:57Z

## Notes

- CI fix required before merge. PR #634 run 38079798434 failed the e2e job (10 tests). This is not a flake.
  `TestThrowException`, `TestResetStore`, `TestJavaScriptInterop`, `TestGoBack`, `TestEventStream`,
  `CloneSuitePassesInServerAndWasm`, `TestCounterComponents`, and `TestChangeRoute` expected render mode
  `WebAssembly` after reload and got `Static`. `TestPersistence` and `TestCacheableWeather` expected state
  text and got `null`. The persistence page snapshot shows the nav button disabled. `scripts/test.cs` does
  not run e2e; CI does (`scripts/e2e.cs`). Fix the product change that keeps the test app on static render
  after the WASM reload, then push so CI on this PR is green. Do not split this into a new task.
  The fix is on this branch (Results, CI e2e). The published Release probe reached WebAssembly and the
  persistence counts survived a reload. The e2e job itself still runs only in CI.
- Related: 103 (companion DevTools app, relevant to M1), 104 (M1), 105 (H1; overlaps M14 packaging test),
  106 (H2; prerequisite for M5 `GetSemaphore` removal), 107 (H3), 108 (H4). M14 also lists the missing interleaved-actions test that 106 adds.
- The kitchen text asked for child tasks. This walk implemented M2–M15 on task 109 because the implementer brief said to finish the remaining product work on this id. M1 stays on task 104.

## Results

M2–M15 are implemented on this branch at version `12.0.0-beta.11` (no version bump). M1 is closed on this checklist with a recorded reason: Steven still chooses finish-versus-delete on task 104, and `LoadStatesFromJson` stays.

- **M2.** `IsAotCompatible` and the same IL2xxx/IL3xxx warnings-as-errors list as the core package are on Blazor and Plus. Trim suppressions name the rooting (generated `StateCloneRegistry`, `AddJavaScriptDispatch`, the app's component type). `dev verify-samples` publishes `samples/05-persistence/wasm/sample-05-wasm` trimmed with `TrimmerSingleWarn=false`.
- **M3.** After rollback, `OperationCanceledException` is always rethrown and is not published. Other handler exceptions still publish `ExceptionNotification` and return `default!` unless `TimeWarpStateOptions.RethrowHandlerExceptions` is true. Topic: `documentation/topics/state-transactions.md`.
- **M4.** `Store.Reset` calls the remove path for every key (cancel, drop previous state, drop the initialization task).
- **M5.** `IStore` no longer exposes `GetSemaphore` or `StateInitializationTasks`. `GetState(Type)` returns `IState`. `WaitForInitializationAsync<TState>()` ensures the state and returns its init task. `FindInitializationTask` looks up without creating state.
- **M6.** `PushRouteInfo` imports `./_content/TimeWarp.State.Plus/js/document-title.js` and calls `getDocumentTitle`. A disconnected circuit, a JS exception, or prerender yields an empty title.
- **M7.** `ActiveActionBehavior` sends `CompleteProcessing` with `CancellationToken.None`.
- **M8.** Elapsed work is an `async Task` posted on the `SynchronizationContext` captured at construction, logged on failure (event 1405). The timer type is unchanged.
- **M9.** `PersistentStateMethod` is only `SessionStorage` and `LocalStorage`. `PersistenceService` takes optional storage services and logs when one is missing. Hosts call `AddTimeWarpStatePersistence()`.
- **M10.** `ThrowIfNotTestAssembly` requires `StateTestOptions.Enable()`. Calling test assemblies that are not the core test project enable it from a module initializer.
- **M11.** `AddTimeWarpStateBlazor` does not register `HttpClient`. The beta.11 migration guide shows the one-line host registration.
- **M12.** The constructor logs naming policy and converter count only when Debug logging is enabled.
- **M13.** `TWS0009` (was `TWS001`), `TWS0010`, `TWS0011`, `TWS0012`. The read-only analyzer uses symbols (`init` allowed; `protected set` only on an abstract state). Ids stay unshipped until 12.0 GA. A positional record cannot inherit `State<T>` (the language forbids it); get-only and init cover that rule.
- **M14.** New tests: state-initialization preprocessor, `RouteState.ChangeRoute`, timer elapsed/restart, `TwPageTitle` / `TimeWarpPageRenderNotifier`, `JsonRequestHandler` init/dispose. Persistence e2e `[Ignore]` removed. `sample-test.cs` deleted. `deep-cloner-tests.cs` renamed to `clone-graph-tests.cs`. Packaging allow-list is task 105. Interleaved actions are task 106. Redux DevTools behavior/interop tests stay with M1.
- **M15.** `claude.md` points at `ai-context.md`. Badges, package readmes, getting-started sample link, solution folders, and the duplicate event id are corrected. `Store_SetState` stays 104. `LoadStatesFromJson` is 106 and `LoadStateFromJson` is 107.

### CI e2e (PR #634, run 38079798434)

`IsAotCompatible` on Blazor and Plus lets the published test-app WebAssembly client trim those assemblies. Two gaps left the reload on the static prerender (`RendererInfo.Name` stayed `Static`, so the `data-qa` spans the e2e job reads were absent and their text was null):

- ICloneable states are not field-cloned. `ActionTrackingState` and `TimerState` were constructed only through `EnsureStates`, and `Clone()` was the only call to their members, so the trimmer dropped the DI constructors and stubbed `Clone()`. Development host validation then threw `NoConstructorMatch` before interactive WebAssembly replaced the prerender. The clone generator emits `RootICloneableStates`: a module initializer that names each ICloneable state with `DynamicallyAccessedMembers` for public constructors and public methods. An ICloneable-only compilation still emits that root.
- `Routes.razor` on the server names `ReduxDevTools`, `TimeWarpJavaScriptInterop`, and `TimeWarpPageRenderNotifier`. That reference is not a root for the client trim, so the WASM runtime reported that the root component type could not be found. `ComponentTrimRoots` in Blazor and Plus names those components from a module initializer (`DynamicallyAccessedMembers.All`). CA2255 is suppressed on that initializer: the root has to live in the library.

After those roots, a published reload reaches `WebAssembly`. Persistence load already wrote the snapshot back with `Store.SetState` (the browser log showed the stored JSON and the loaded guid). `LoadPersistentStateRequest` is not an action, so nothing re-rendered and the page kept the `Initialize()` defaults. `PersistenceTestPage` and `ServerSidePersistenceTestPage` now await `WaitForInitializationAsync` for purple and blue, and they leave the counter buttons out of the DOM until that task finishes. That matches `documentation/topics/persistence.md` and sample 05, and a click cannot increment the defaults before the snapshot is in the store.

Local published Release probe of `tests/test-app/test-app-server` (content root `/tmp/task109-sut`, `ASPNETCORE_ENVIRONMENT=Development`, Chromium). This is not the CI e2e job:

- Counter after reload: `WebAssembly` / `InteractiveWebAssemblyRenderMode`, count `3` to `8`, `#blazor-error-ui` hidden.
- Persistence: purple guid and count `6`, and blue guid and count `5`, survived a reload in `WebAssembly`. A new tab kept the purple guid and count and showed a new blue guid with count `2`.
- Cacheable weather after reload: `WebAssembly`, cache duration `00:00:10`.

### How to validate

**Smoke**

```bash
dotnet build source/timewarp-state-blazor/timewarp-state-blazor.csproj -c Release
dotnet build source/timewarp-state-plus/timewarp-state-plus.csproj -c Release
dotnet run --file scripts/test.cs
ganda repo audit
```

**Expect**

- Both Release builds exit 0. IL2xxx/IL3xxx are warnings-as-errors on those projects, so a trim warning fails the build.
- `scripts/test.cs` exits 0. This walk: analyzer 44 passed; source generator 80 passed; state 115 passed, 1 skipped; plus 40 passed, 1 skipped; telemetry 13 passed; client 65 passed, 1 skipped; architecture 7 passed, 1 skipped.
- `ganda repo audit` exits 0. This walk: Passed 29, Skipped 1, one non-blocking `kebab-path-names` warning on the five Blazor `lib.module` paths. Those names are the Razor static-web-asset convention and were left as they are.

**Automated gate**

```bash
dotnet run --file scripts/test.cs
```

`./bin/dev test` is the same suite. It does not run end-to-end tests.

**Depends on**

- .NET 11 SDK. The trimmed sample publish in `dev verify-samples` runs after `dev pack`, because samples reference the packed feed.

**Not in scope**

- Task 104 still decides whether Redux DevTools time travel is finished or deleted. This branch does not delete `LoadStatesFromJson`.
- The full e2e job (`scripts/e2e.cs` on `ubuntu-latest`) was not executed on this machine. The published Release probe above covers the ten failures from run 38079798434: WebAssembly after reload, persistence guid/count across reload and a new tab, and the cache-duration text. CI on PR #634 is the remaining run of that job.
- The trimmed `sample-05-wasm` publish is wired into `verify-samples` and was not executed in this walk. Library Release builds already fail on IL warnings. The test-app client publish used for the probe above is trimmed (`Optimizing assemblies for size`).

### Review disposition

- **Rounds:** 3. Effort 3 (Budget.ByDiff: 1914 lines for rounds 1–2, 2432 for round 3). Roster: general. Round 3 covered the PR #634 e2e fix (`2d150884..d2e2e215`) and found nothing new.
- **Final counts:** bug 0; suggestion 2 fixed; nit 1 fixed, 1 wontfix; open 0.
- **Disposition:** `accepted-exceptions`.
- **Fixed on this task:**
  - `Store.Reset` walks every key in `States`, `PreviousStates` and the init-task map, and rethrows per-key failures as `AggregateException`.
  - The `TimerState` Design region says the circuit context is captured at first construction.
  - TWS0012 now reports once per property on partial states. New test: `Given_PartialTimeWarpState_ReportsOnce`.
- **Wontfix (M4, nit):** the transaction rollback check can re-create a state that `Reset` or `RemoveState` removed mid-action. The reviewer rated this acceptable, and a non-creating lookup would widen `IStore` again right after M5 narrowed it.
- **Artifacts:** `review/review-framework.md`, `review/round-1/{general,merged}.md`, `review/round-2/{general,merged}.md`, `review/round-3/{general,merged}.md`, `review/disposition.md`.
- After the fixes, `./bin/dev build` and `./bin/dev test` are green. Analyzer tests: 44 passed.
