# Code Review — TimeWarp.State — 2026-06-11

**Scope:** Full review of `source/` (timewarp-state, timewarp-state-plus, timewarp-state-policies, timewarp-state-analyzer, timewarp-state-source-generator — 102 files, ~5,100 LOC) plus the pending working-tree deletions (`aider.instructions.md`, `qodana.yaml`).

**Method:** 7 independent finder passes (3 correctness angles, reuse, simplification, efficiency, altitude) followed by per-finding verification against the source. Every finding below was verified against the actual code; verdicts are noted. One candidate was refuted during verification and excluded (action-tracking `finally` with a cancelled token — the CompleteProcessing dispatch path never observes the token, so the claimed failure is not constructible).

**Pending deletions:** Repo-wide grep found no remaining references to `qodana` or `aider.instructions.md` in `.github/workflows`, `scripts/`, or `documentation/`. The deletions are safe to commit.

---

## Top 10 findings (ranked by severity)

| # | File | Issue |
|---|------|-------|
| 1 | `store.redux-dev-tools.cs:76` | DevTools time-travel always throws under default options (short name vs FullName mismatch) |
| 2 | `state-transaction-behavior.cs:98,118` | *(revised — see §2)* Error-path publishes with the cancelled token; misleading log text; `default!` only safe for Unit responses |
| 3 | `route-state.go-back.cs:37` | Off-by-one clamp lets GoBack empty the stack, then `Peek()` throws — **reproduced by failing test** |
| 4 | `timer-state.add-timer.cs:27` | Timers added/updated via actions are never wired or started — feature silently dead |
| 5 | `persistence-service.cs:5,26` | Persistence save/load use mismatched serializer options; storage key collides on simple type name |
| 6 | `multi-timer-post-processor.cs:19` | Post-processor re-dispatches through its own pipeline — unbounded recursion when registered as documented |
| 7 | `store.cs:96–138` | Check-then-add races in `GetState`/`GetSemaphore` throw on concurrent first access |
| 8 | `check-complex-parameter-changed.cs:92` + `timewarp-state-component.cs:143` | Swapped current/incoming parameter values; reachable leftover `throw new Exception("WTF")` |
| 9 | `json-request-handler.cs:78` | One `DotNetObjectReference` leaked per render of `TimeWarpJavaScriptInterop` |
| 10 | `redux-dev-tools.ts:61,99,106` + `redux-dev-tools-interop.cs:52` | DevTools `init` never reaches `DevTools.init()`; Commit button dead; unhandled promise rejection race |

---

## Correctness bugs

### 1. Redux DevTools time-travel is broken with default options — **CONFIRMED**
`source/timewarp-state/store/store.redux-dev-tools.cs:76`

`UseFullNameForStatesInDevTools` defaults to `false` (`extensions/timewarp-state-options.cs:27`), so `GetSerializableState` emits short state names via `pair.Key.Split('.').Last()` (line 21). But `LoadStateFromJson` resolves the type with `type.FullName?.Equals(typeName) == true` (line 76) — a namespaced state's `FullName` never equals its short name, so every time-travel/import operation throws `InvalidOperationException` for any default-configured app.

**Failure scenario:** User enables DevTools, clicks any time-travel step → `LoadStatesFromJson` passes `"CounterState"` to a FullName-equality scan → `FirstOrDefault` returns null → throw. The feature only works at all with `UseFullNameForStatesInDevTools = true`.

**Fix:** Resolve via the Store's own `States` dictionary (already keyed by `FullName`) and make serialization/lookup use the same key format in both modes.

**Related (same method):**
- Line 82–88: `hydrateMethodInfo = stateType.GetMethod(...) ?? throw new InvalidOperationException();` makes the following null-check branch with the *descriptive* `NotImplementedException("The Hydrate Method was not found for the type:{typeName}")` unreachable — failures surface as a bare, undiagnosable `InvalidOperationException`. **CONFIRMED**
- Lines 73–77: even when it works, it rescans `AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypes())` per state per time-travel step, though `States[typeName].GetType()` already yields the type. O(all loaded types) repeated work, and `ReflectionTypeLoadException` risk. **CONFIRMED**

### 2. StateTransactionBehavior error path — **REVISED after author feedback** (original finding overstated)
`source/timewarp-state/features/pipeline/state-transaction-behavior.cs:92–120`

**Correction (2026-06-11):** The original finding claimed "swallows all exceptions and returns `default!`" as a bug. That is the *intended design*: the behavior's documented purpose is to roll back to a known-good state and convert the exception to an `ExceptionNotification` rather than let it propagate and crash the circuit/app. Catch-rollback-notify is not a defect. Additionally, the behavior is constrained `where TRequest : IAction` and action handlers return `Unit` (a struct), so `default!` is a real value, not null, for essentially all traffic through this behavior — the "callers silently receive null" scenario requires a non-Unit `IAction` response, which the library's own patterns don't produce.

What remains, all minor:

- **Error reporting can itself be cancelled (line 118):** `Publisher.Publish(exceptionNotification, cancellationToken)` reuses the request's token. If the handler failed *because* that token was cancelled, the Publish throws `OperationCanceledException` immediately — so the `ExceptionNotification` handlers never run and an exception propagates anyway, defeating the don't-crash design in exactly that case. Publish the error notification with `CancellationToken.None`.
- **`OperationCanceledException` is reported as an error:** cancellation isn't a failure; converting it to an `ExceptionNotification` may surface spurious error UI. Worth a deliberate `catch (OperationCanceledException)` decision either way.
- **Misleading log text (line 98):** the catch logs `"Error cloning State"` for what is actually a handler failure (cloning happens earlier, outside the try). Same copy-paste family as the constructor logging `ReduxDevToolsBehavior` as its class name (finding 27).

### 3. `GoBack` can empty the route stack and crash on `Peek()` — **CONFIRMED (reproduced by test)**
`source/timewarp-state-plus/features/routing/route-state/route-state.go-back.cs:37,45`

The clamp is `Math.Min(action.Amount, RouteStack.Count)` — it should be `Count - 1` (the current route occupies one slot; `route-state.cs:15` itself defines `CanGoBack => RouteStack.Count > 1`). With `n >= Count`, the loop pops the stack empty and the unconditional `RouteState.RouteStack.Peek()` throws `InvalidOperationException`.

**Empirically reproduced (2026-06-11):** unit test `tests/timewarp-state-plus-tests/features/routing/go-back-repro-tests.cs` drives the real `Handler` with a faked `IStore`/`NavigationManager`. Depth 1 + `GoBack()` and depth 2 + `GoBack(2)` both throw `System.InvalidOperationException: Stack empty.` at `route-state.go-back.cs:45`; the normal case (depth 2, `GoBack()`) navigates correctly. (Run at commit `ff291b92`, the last commit that compiles — see addendum.)

**Mitigation in practice:** the exception is caught by `StateTransactionBehavior`, which rolls back and publishes an `ExceptionNotification` — so the app doesn't crash; the symptom is a navigation that silently does nothing (or surfaces as an error notification if the app handles those).

### 4. Timers created via `AddTimer`/`UpdateTimer` actions never fire — **CONFIRMED**
`source/timewarp-state-plus/features/timers/timer-state/timer-state.add-timer.cs:27`, `timer-state.update-timer.cs:32`

Both handlers store a bare `new Timer(duration)` without attaching `Elapsed += OnTimerElapsed`, without `AutoReset = false`, and without `Start()`. `TimerState.Initialize` (`timer-state.cs:55–58`) does all three. Nothing wires the handler later — `RestartTimer` only calls `Stop(); Start()`. Any timer added or updated through these actions silently never publishes `TimerElapsedNotification` (and if started later, fires with default `AutoReset = true`, unlike configured timers).

**Fix:** Extract Initialize's wiring into a shared `CreateTimer(name, config)` helper and use it from all three sites.

### 5. Persistence round-trip uses mismatched serializer options and colliding keys — **CONFIRMED**
`source/timewarp-state-plus/features/persistence/services/persistence-service.cs:5,26`, `pipeline/persistent-state-post-processor.cs:53,63`

Two independent defects:
- **Options mismatch:** Save goes through Blazored `SetItemAsync` (serialized with Blazored's options); load deserializes with a locally constructed `private readonly JsonSerializerOptions JsonSerializerOptions = new();`. Neither side honors the user-configured `TimeWarpStateOptions.JsonSerializerOptions` that `Store` (`store.cs:37`) and `JsonRequestHandler` (`json-request-handler.cs:22`) use. Any state requiring custom converters or enum-as-string fails to round-trip; any Blazored-side option customization breaks loading outright.
- **Key collision:** The storage key is the state's *simple* type name (`stateType.Name`). Two `[PersistentState]` classes with the same simple name in different namespaces share one storage slot — last write wins, and the other state cross-hydrates from foreign JSON or throws `JsonException`.

**Fix:** Inject `TimeWarpStateOptions` and use its `JsonSerializerOptions` on both sides (serialize manually rather than via Blazored's object overload), and key by `FullName`.

### 6. MultiTimerPostProcessor recurses unboundedly when registered as documented — **CONFIRMED**
`source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs:19`

The post-processor runs for every `TRequest : notnull` and unconditionally awaits `TimerState.ResetTimersOnActivity()`, which the source generator implements as `Sender.Send(new ResetTimersOnActivityActionSet.Action(), ...)` — a full pipeline pass whose post-processors include `MultiTimerPostProcessor` itself, with no type guard breaking the cycle. The feature readme (line 48) documents registering it via `AddScoped(typeof(IRequestPostProcessor<,>), ...)`; following that documentation gives infinite recursion on the first dispatched action. (No sample/test in the repo actually registers it, which is consistent with it never having been exercised.) At absolute minimum, every user action would pay a second full pipeline pass.

**Fix:** Guard `if (request is ResetTimersOnActivityActionSet.Action) return;` as a stopgap; the deeper fix is a general marker (e.g. `IInternalAction`) that cross-cutting behaviors skip — `ActiveActionBehavior` already hand-rolls the same exclusion with `EnsureNotType` throws, confirming the missing abstraction.

### 7. Check-then-add races in `Store.GetState` and `Store.GetSemaphore` — **CONFIRMED**
`source/timewarp-state/store/store.cs:96–103,125–138`

Both methods do `TryGetValue` → create → `TryAdd` → `throw InvalidOperationException` on `TryAdd` failure. The Store is scoped (per-circuit in Blazor Server), but concurrent access within a circuit is reachable via thread-pool continuations and background dispatches. Two concurrent first accesses for the same state type: both miss, both construct and `Initialize()` a state, and the loser *throws* — turning a benign race into a faulted request. The `GetSemaphore` variant is worse: the throw happens exactly in the high-concurrency case the semaphore exists to serialize, and the losing `SemaphoreSlim` leaks undisposed.

**Fix:** Use `ConcurrentDictionary.GetOrAdd` and discard the losing instance (don't run `Initialize` until the winner is known, or accept idempotent double-init).

### 8. `CheckParameterChanged` swaps current and incoming values — **CONFIRMED**
`source/timewarp-state/components/timewarp-state-component.check-complex-parameter-changed.cs:92–93,120`

```csharp
object? newValue = property.GetValue(this);   // actually the OLD value (runs before base.SetParametersAsync)
object? currentValue = parameter.Value;        // actually the INCOMING value
```
The virtual `CheckComplexParameterChanged(parameterName, currentValue, incomingValue)` (documented contract at lines 174–175) therefore receives its arguments reversed. Harmless for the symmetric default `ReferenceEquals`, but any override with directional logic (e.g. "re-render only on version upgrade") is inverted, and the current/incoming type names in trace logs are swapped.

### 9. Reachable leftover debug throw `Exception("WTF")` in `ShouldRender` — **CONFIRMED**
`source/timewarp-state/components/timewarp-state-component.cs:142–143`

Marked `// TODO: Remove`, but shipping and reachable: `CheckParameterChanged` returns `HandleUnregisteredParameter(parameter)` (line 89) without setting `RenderReasonDetail`, and `RenderReasonDetail` has a private setter — so a derived component using the documented virtual `HandleUnregisteredParameter` extension point (returning true) *cannot avoid* `ParameterTriggered && RenderReasonDetail is null`, crashing the component with an undiagnosable exception instead of re-rendering.

### 10. `DotNetObjectReference` leaked on every render — **CONFIRMED**
`source/timewarp-state/features/javascript-interop/json-request-handler.cs:78–83`

`InitAsync` has no initialization guard (contrast `ReduxDevToolsInterop.InitAsync:58`, which has one) and creates `DotNetObjectReference.Create(this)` per call with no disposal. `TimeWarpJavaScriptInterop.razor:8–11` calls it from `OnAfterRenderAsync` with no `firstRender` check, and the JS side just overwrites `timeWarpState.jsonRequestHandler`, orphaning the prior reference. Net: one leaked, still-invokable `DotNetObjectReference` per render for the lifetime of the circuit.

**Fix:** `if (firstRender)` in the component, an `IsInitialized` guard in `InitAsync`, and dispose the reference in `Dispose`.

### 11. Redux DevTools JS interop: three defects — **CONFIRMED**
`source/timewarp-state/features/redux-dev-tools/redux-dev-tools-interop.cs:52`, `source/timewarp-state/wwwroot/typescript/redux-dev-tools.ts:61,99,106`

- **`init` never initializes:** C# sends the bare string `"init"`; JS checks `action.type === 'init'` — `undefined === 'init'` is always false, so `DevTools.init(state)` (ts:109) is unreachable and the lifted-state baseline is wrong; every init appends an `'init'` action via `send` instead of resetting history.
- **Commit button dead:** `dispatchRequests` maps `'COMMIT': undefined` (ts:61), so the `if (requestType)` guard skips dispatch — while C# ships a complete `CommitRequest`/`CommitHandler` pair that is unreachable dead code.
- **Startup race / unhandled rejection:** `MessageHandler` fire-and-forgets `DispatchRequest(...).then()` with no rejection handler (ts:99); `DispatchRequest` throws if `jsonRequestHandler` isn't set yet (`timewarp-state.ts:21–22`), which happens for any DevTools message arriving before `TimeWarpJavaScriptInterop`'s `OnAfterRenderAsync` completes — unhandled promise rejection, message silently dropped.

### 12. RenderSubscriptionContext suppression is sticky across dispatches — **CONFIRMED**
`source/timewarp-state/features/render-subscriptions/render-subscription-context.cs:22,55–57`

Suppression flags are keyed by action-type `FullName` in a `ConcurrentDictionary` on a scoped service (app-lifetime in WASM, circuit-lifetime in Server). Nothing in the framework ever calls `Reset()`/`RemoveAction()` (only tests do), so one `EnsureAction(action)` call suppresses re-render for **all subsequent and concurrent dispatches of that action type** until a handler manually removes it — a call-order/lifetime trap in the public API.

**Fix:** Make suppression declarative (a `[SuppressRender]` attribute / marker interface checked by the post-processor, cached per closed generic) or scope the flag to the single in-flight dispatch.

### 13. Unsynchronized subscription list races in Blazor Server — **PLAUSIBLE**
`source/timewarp-state/subscriptions.cs:7,87–106`

`Subscriptions` is scoped per circuit and holds a plain `List<Subscription>` with no lock. `ReRenderSubscribers` both reads (87–89) and mutates (`TimeWarpStateComponentReferencesList.Remove(subscription)` at 106) from the mediator pipeline thread, while component `Dispose` calls `Remove`/`RemoveAll` on the renderer dispatcher. An action dispatched off the circuit's sync context (timer, hub event, `Task.Run`) racing a component disposal can corrupt the list or throw. Reachable but requires off-context dispatch — hence PLAUSIBLE rather than CONFIRMED.

**Fix:** Lock around mutations/snapshots, or restructure per finding 24 below (a keyed dictionary solves both the race exposure and the O(n) scans).

### 14. Analyzers match state types by simple name `"State"` — **CONFIRMED**
`source/timewarp-state-analyzer/state-implementation-analyzer.cs:44`, `state-inheritance-analyzer.cs:39`, `state-read-only-public-properties-analyzer.cs:53`

All three analyzers identify TimeWarp state types by `BaseType.Name == "State"` (one with a type-argument count check), never by metadata name/assembly. Any consumer deriving from a *different* library's `State<T>` base gets error-severity `TWS001` false positives that break their build.

**Fix:** Compare against `compilation.GetTypeByMetadataName("TimeWarp.State.State`1")` via `SymbolEqualityComparer`.

### 15. PersistenceStateSourceGenerator mishandles nested classes and locale — **CONFIRMED**
`source/timewarp-state-source-generator/persistence-state-source-generator.cs:46,81,167`

The generator captures only namespace + class identifier. A nested `[PersistentState]` class emits a *top-level* partial that doesn't merge with the original — generated references to `Sender`/`CancellationToken` fail to compile. Two same-named nested classes in one namespace produce identical hint names → `AddSource` throws `ArgumentException`, failing the entire generator. Separately, `ToCamelCase` uses culture-sensitive `char.ToLower` for generated identifiers (the tr-TR dotless-i problem — generated output differs by build-machine locale).

**Fix:** Either reject nested classes with a diagnostic, or emit the containing-type chain; include containing types in the hint name; use `char.ToLowerInvariant`.

### 16. Repeated sequence number 0 in manual RenderFragment — **CONFIRMED** (limited impact)
`source/timewarp-state/features/redux-dev-tools/components/timewarp-state-dev-component.cs:15–18`

`OpenComponent<...>(0)` followed by three `AddComponentParameter(0, ...)` calls violates Blazor's distinct-and-increasing sequence number contract. Practical impact is limited (no conditional structure; attribute diffing is name-based), but it should be `0,1,2,3`.

---

## Efficiency

### 17. `new StackTrace()` on every render of every component — **CONFIRMED**
`timewarp-state-component.cs:95` (ShouldRender), `check-complex-parameter-changed.cs:35` (SetParametersAsync), `register-render-trigger.cs:49` (StateHasChanged)

Three unconditional stack captures + frame metadata resolution + string interpolation on the hottest paths in the library, consumed inside the library only by `LogTrace`. Especially costly on WASM. (Note: the resulting `...WasCalledBy` properties are public and rendered by test-app diagnostic pages, so gate rather than delete.)

**Fix:** Guard with `if (Logger.IsEnabled(LogLevel.Trace))`, or use `[CallerMemberName]` plumbing. Also extract the thrice-copy-pasted capture snippet into one helper — the copies have already drifted (two record `Class.Method`, one records method name only).

### 18. Full state serialization per action as an unguarded log argument — **CONFIRMED**
`source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs:46–62`

`JsonSerializer.Serialize(state)` is passed as a `LogTrace` template argument — evaluated eagerly regardless of log level — so every action on a persisted state serializes the whole state an extra time in production. Lines 29–32 also run `GetEnclosingStateType()`/`GetCustomAttribute<PersistentStateAttribute>()` reflection on *every action of every type*.

**Fix:** Wrap in `Logger.IsEnabled(LogLevel.Trace)`; cache the attribute lookup in a `static readonly` field of the closed generic.

### 19. Per-dispatch attribute reflection in ActiveActionBehavior — **CONFIRMED**
`source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs:23`

`typeof(TAction).GetCustomAttributes(typeof(TrackActionAttribute), false)` runs on every `Handle` call though the answer is constant per closed generic. Use `private static readonly bool IsTracked = typeof(TAction).IsDefined(typeof(TrackActionAttribute), false);`.

### 20. O(n) subscription scans on render and dispatch hot paths — **CONFIRMED**
`source/timewarp-state/subscriptions.cs:26,87–89`

`Add` does a LINQ `Any()` scan per `GetState` call (per component per render); `ReRenderSubscribers` allocates `Where().ToList()` per dispatched action. Restructure as `Dictionary<Type, Dictionary<string, Subscription>>` (state type → component id) for O(1) add and allocation-free per-state iteration (also addresses finding 13).

---

## Reuse / simplification

### 21. Hand-copied camelCase implementation — **CONFIRMED**
`source/timewarp-state/json/camel-case.cs` (entire file; oddly in namespace `Microsoft.JSInterop`)

Duplicates `JsonNamingPolicy.CamelCase`, which the codebase already uses (`timewarp-state-options.cs:38`) — and its callers exist precisely to reconstruct keys produced by that policy, so drift between the two breaks Debug-state hydration. Replace with `JsonNamingPolicy.CamelCase.ConvertName(memberName)` and delete the file.

### 22. Duplicated enclosing-state-type walk — **CONFIRMED**
`source/timewarp-state-policies/be-nested-in-state-custom-rule.cs:9–11`

Character-for-character the same `DeclaringType`-walk as `TypeExtensions.GetEnclosingStateType` (`timewarp-state/extensions/type-extensions.cs:8–11`), in a project that already references timewarp-state. Add a `TryGetEnclosingStateType` (the extension throws; the rule needs bool) and call it from both, so the architecture policy and the runtime pipeline can't enforce different rules.

### 23. Duplicated DI-registration guard — **CONFIRMED**
`source/timewarp-state-plus/extensions/service-collection-extensions.cs:26–27`

Identical to the private helper in `timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs:150–151`; both reinvent `TryAddScoped`/`TryAddTransient`. Note the current all-or-nothing early return (line 16) skips *handler* registration whenever `RouteState` is already present — `TryAdd*` per descriptor is also a behavior fix.

### 24. Static `RenderCounts` dictionary for per-instance data — **CONFIRMED**
`source/timewarp-state/components/timewarp-state-component.render-mode.cs:7`

Per-instance render counts in a static `ConcurrentDictionary<string,int>` keyed by component Id, requiring `TryRemove` cleanup in `Dispose` (leak if missed) and global contention. Nothing uses the static aspect. Replace with a plain instance `int` field.

### 25. Dead field `CompiledPropertyComparisons` — **CONFIRMED**
`source/timewarp-state/components/timewarp-state-component.register-render-trigger.cs:8`

A `ConcurrentDictionary` allocated per component instance with exactly one repo-wide reference: its declaration. Delete.

### 26. Dead debug scaffolding ships in the analyzer — **CONFIRMED**
`source/timewarp-state-analyzer/timewarp-state-action-analyzer.cs:41,45,60–64`

`DebugRule`/`TWD001` is advertised in `SupportedDiagnostics` but its only producer (`ReportDebugInformation`) has zero call sites; `LaunchDebugger`'s only call site is commented out. A registered diagnostic ID that can never fire confuses consumers, and `Debugger.Launch` code in a shipped NuGet analyzer is one accidental uncomment away from freezing every consumer's build. Delete all three.

### 27. Copy-paste wrong class name in constructor log — **CONFIRMED**
`source/timewarp-state/features/pipeline/state-transaction-behavior.cs:36`

`StateTransactionBehavior`'s constructor logs `typeof(ReduxDevToolsBehavior<,>).GetSimpleName()` as its own name — every "constructing {ClassName}" trace for state transactions points to the wrong class.

---

## Altitude / design

### 28. Auto-load persistence via string-mangled type names — **CONFIRMED**
`source/timewarp-state-plus/features/persistence/state-initialized-notification-handler.cs:22–46`

Auto-load works by rewriting the state's `AssemblyQualifiedName` into `"{fullName}+LoadActionSet+Action"`, `Type.GetType`, and `Activator.CreateInstance`. It works for generator-emitted actions only; any hand-written, renamed, generic, or parameterless-ctor-less load action *silently* gets no load (LogDebug and move on), and the string-only type reference breaks under trimming/AOT. Deeper fix: express the contract in the type system (e.g. `IStateLoader<TState>` or an attribute referencing the action `Type`), discovered at registration.

### 29. Hard-coded action-type exclusions in ActiveActionBehavior — design note
`source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs:25`

The behavior guards against self-recursion by `EnsureNotType` runtime throws against two specific action types. Together with finding 6, this shows the missing general mechanism: internal/infrastructure actions need a type-level marker that all cross-cutting behaviors honor, instead of each behavior hand-listing exclusions (failure mode today: runtime exception or infinite recursion, never a compile error).

---

## Suggested priorities

1. **Ship-blockers for advertised features:** findings 1, 4, 11 (DevTools time-travel, action-added timers, DevTools init/Commit) — these features do not work as documented under default configuration.
2. **Silent data/error loss:** findings 5, 12 (persistence round-trip mismatch, sticky render suppression), plus the cancellation-token sub-issue in finding 2.
3. **Crashes on reachable paths:** findings 3, 7, 9 (GoBack, Store races, "WTF" throw).
4. **Hot-path costs:** findings 17–20 are mechanical fixes with measurable render/dispatch wins.
5. **Cleanup batch:** findings 21–27 are low-risk deletions/replacements suitable for one housekeeping PR.

---

## Addendum (2026-06-11, after author review)

**Verification methodology.** "CONFIRMED" in this report means *verified by independent code reading* — a second agent traced the cited lines, callers, and DI registrations and had to quote the code to uphold or refute each claim. It does **not** mean executed. Findings re-verified at execution level are explicitly marked (currently: finding 3, reproduced by a failing unit test).

**Finding 1 disposition:** will be addressed by removing the ReduxDevTools integration in favor of OpenTelemetry + Aspire dashboard integration — see kanban task `058-implement-timewarpstatetelemetry`. That also retires findings 10, 11, 16, and the DevTools half of 17's JS layer.

**Finding 2:** revised after author feedback — the catch-rollback-notify pattern is intentional design (rethrowing would crash the circuit). See the corrected §2 for the remaining minor sub-issues.

**Build status discovery:** the `dev` branch at HEAD (`bf3990b9`) **does not compile**. Task 039 switched package references and global usings to the `Mediator` 3.0.1 packages, but the API migration (tasks 040–049 in `kanban/to-do/`: handler base classes, pipeline behaviors to `MessageHandlerDelegate`/`ValueTask`, processors, registration) has not been done yet, so `source/timewarp-state` fails with ~10 compile errors (`RequestHandlerDelegate<>` not found, `IRequestHandler` return-type mismatches, MSG0007 from MediatorGenerator). This is presumably known/expected mid-migration, but it means no tests can run on `dev` until task 049 lands — runtime verification for this review was done in a throwaway worktree at `ff291b92` (last compiling commit, TimeWarp.Mediator 13.0.0). Also note: restore requires the `artifacts/packages` directory to exist (NU1301 otherwise); it's created as a side effect of building `source/`, but a fresh worktree needs a build (or `mkdir`) before `dotnet test` will restore.
