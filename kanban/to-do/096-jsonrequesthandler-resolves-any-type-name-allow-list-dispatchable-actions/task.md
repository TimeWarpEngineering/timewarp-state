# JsonRequestHandler resolves any type name: allow-list dispatchable actions

## Description

`JsonRequestHandler.Handle(string requestTypeAssemblyQualifiedName, string? json)` is
`[JSInvokable]` (`source/timewarp-state/features/javascript-interop/json-request-handler.cs`, ~L51–91),
and JavaScript reaches it through `timeWarpState.DispatchRequest(name, request)`. Today it:

- resolves the type with plain `Type.GetType(name)` (~L65), with **no allow-list**;
- does not check that the type is an action (`IAction` / `IBaseAction`);
- calls `Activator.CreateInstance(type)` when the JSON is empty, otherwise
  `JsonSerializer.Deserialize(json, type, …)` (~L85);
- then calls `Sender.Send(instance)` on `ISender<ClientPipeline>` (~L88).

So any script running in the page can make the app construct, and attempt to send, any loaded
type. Found on 2026-10-05 while evaluating hypermedia for timewarp-architecture task 275. The only
known consumer is that template's Counter JS-interop demo (`Spa.Counter.DispatchIncrementCountAction`
→ `CounterState+IncrementCounterActionSet+Action`) and Redux DevTools (`redux-dev-tools.js`).

## Requirements

1. **Allow-list by default.** Only types the host has opted in are dispatchable from JS. Choose a
   mechanism and record why:
   - an attribute on the Action class, for example `[JsDispatchable]`, with source-generated
     registration and no reflection scan at runtime; or
   - explicit registration, for example `services.AddJavaScriptDispatch(b => b.Allow<…>())`; or
   - reuse the action catalog (opt-in by `[CatalogAction]`). Consider and record why or why not.
2. **Resolve against the allow-list, never `Type.GetType` on arbitrary input.** Accept the same
   wire name the JS sends today, or a shorter stable name. If the name changes, keep a migration
   path for the existing `DispatchRequest(fullName, …)` call shape. Reject:
   - unknown names;
   - types that are not actions;
   - deserialization failures.

   Each rejection logs a warning and throws a clear exception, failing closed.
3. **Redux DevTools:** jump-to-state and dispatch replay must keep working, but only in
   Development or when DevTools is enabled. Decide whether DevTools gets its own gated path or
   uses the allow-list.
4. **Analyzer**, if cheap: warn when `DispatchRequest` JS interop is enabled but nothing is
   allow-listed, or an attribute on a non-action type. Use the TWS id range and register it in
   the analyzer docs.
5. **Tests:** an allowed action dispatches; an unknown name, a non-action type, bad JSON and an
   empty-but-non-default-constructible type each fail closed; the DevTools path in Development
   works.
6. **Release** as the next 12.0.0 beta, with release notes calling out the breaking change: JS
   dispatch now requires opt-in. Follow up in timewarp-architecture to opt in the Counter demo's
   action and pin the new beta. File that follow-up when this ships.

## Checklist

- [x] Allow-list mechanism chosen and recorded; arbitrary `Type.GetType` removed
- [x] Fail-closed rejections (unknown name, non-action, bad JSON) with warnings
- [x] Redux DevTools path preserved, gated
- [x] Analyzer (if cheap), docs updated — analyzer not needed (see Results)
- [x] Tests for allowed and rejected cases
- [x] Version bump + release notes (breaking: JS dispatch opt-in)
- [x] Preserve the Blazor JS initializer filename `Test.App.Client.lib.module.js`. If
      `ganda repo audit --fix` lowercases it, revert that rename.
- [ ] Implementation review; host `open-pr`

## Notes

- Not a vulnerability report against third parties. It hardens a first-party library: page
  script is already same-origin, but the handler should not be a generic object factory.
- Consumer follow-up: timewarp-architecture opts in `CounterState+IncrementCounterActionSet+Action`
  after release.

## Results

**Mechanism: explicit registration.** `services.AddJavaScriptDispatch(b => b.Allow<TAction>(alias?))`
fills a singleton `JavaScriptDispatchRegistry`; `JsonRequestHandler.Handle` resolves names only against it.
`Type.GetType` is gone.

- Why not an attribute (`[JsDispatchable]`): needs a second generated registry and an analyzer for
  "attribute on a non-action"; the generic constraint `TAction : class, IAction` gives the same proof at
  compile time for free. `Allow(Type)` checks `IAction` at startup and throws.
- Why not `[CatalogAction]`: the catalog is a user-facing palette/agent surface. Reusing it would expose every
  cataloged action to page script and force a `Description` on purely technical interop actions.
- **Wire names:** full name, `"FullName, AssemblyName"`, assembly-qualified name (version/culture/token
  ignored, so the existing `DispatchRequest("…+Action, Asm, Version=…", …)` call shape keeps working), or an
  optional alias. Generic names match only exactly.
- **Fail closed:** unknown/not-allowed name, non-action type, bad JSON / `null` JSON, and empty JSON for a type
  without a public parameterless constructor each log a warning (EventIds 203/204) and throw
  `InvalidRequestTypeException` (inner exception kept). Nothing is sent.
- **Redux DevTools:** Start/Commit requests are not actions; `UseReduxDevTools()` allow-lists them through
  internal `AllowReduxDevToolsRequests()`. Without DevTools enabled they are rejected. (Jump-to-state is not
  implemented in the JS mapper today, so only Start/Commit exist to gate.)
- **Analyzer: skipped.** "Attribute on a non-action" cannot happen (no attribute; generic constraint).
  "Interop enabled but nothing allow-listed" is not decidable at compile time (enablement is a rendered
  component plus runtime DI); the runtime warning on the first rejected name covers it. No TWS id used.
- Test app opts in `CounterState.IncrementCountActionSet.Action` in `tests/test-app/test-app-client/program.cs`.
- Docs: `documentation/topics/enable-javascript-interop.md` (was empty; now in topics toc), readme section,
  `release12.0.0-beta.8.md` with the breaking change. Version 12.0.0-beta.7 → 12.0.0-beta.8.
- Follow-up after release (not filed yet, file when this ships): timewarp-architecture opts in
  `CounterState+IncrementCounterActionSet+Action` via `AddJavaScriptDispatch` and pins 12.0.0-beta.8.
- Not run: Playwright E2E (`JavaScriptInteropPage`); the E2E suite baseline already fails on origin/dev.

### How to validate

**Smoke:**

```bash
dotnet test tests/timewarp-state-tests --logger "console;verbosity=normal" 2>&1 | grep JsonRequestHandlerDispatch
dotnet test tests/client-integration-tests
ganda repo audit
```

**Expect:** 10 `JsonRequestHandlerDispatchTests.Should_` cases pass (allowed dispatch by AQN, full name and
alias; rejects unknown name, empty allow-list, non-action type, `Allow(typeof(NonAction))`, bad JSON,
empty JSON without parameterless ctor, DevTools request when DevTools is off; DevTools Start dispatches when
`UseReduxDevTools()` is on). `timewarp-state-tests` 62 passed, client-integration 56 passed. Audit passes with
one advisory: `Test.App.Client.lib.module.js` kebab-path (intentionally preserved Blazor initializer name).
Manual: run the test app, open `/JavaScriptInteropPage`, click the button → count +7; remove the
`AddJavaScriptDispatch` line → click logs a "not allowed" warning and the count does not change.

## Session

- Created: 2026-10-05 (cockpit, from timewarp-architecture 275 research)
- 2026-10-05 implement oracle: allow-list via AddJavaScriptDispatch, tests, docs, beta.8 bump
