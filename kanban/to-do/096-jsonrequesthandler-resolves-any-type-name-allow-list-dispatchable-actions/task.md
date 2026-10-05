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

- [ ] Allow-list mechanism chosen and recorded; arbitrary `Type.GetType` removed
- [ ] Fail-closed rejections (unknown name, non-action, bad JSON) with warnings
- [ ] Redux DevTools path preserved, gated
- [ ] Analyzer (if cheap), docs updated
- [ ] Tests for allowed and rejected cases
- [ ] Version bump + release notes (breaking: JS dispatch opt-in)
- [ ] Preserve the Blazor JS initializer filename `Test.App.Client.lib.module.js`. If
      `ganda repo audit --fix` lowercases it, revert that rename.
- [ ] Implementation review; host `open-pr`

## Notes

- Not a vulnerability report against third parties. It hardens a first-party library: page
  script is already same-origin, but the handler should not be a generic object factory.
- Consumer follow-up: timewarp-architecture opts in `CounterState+IncrementCounterActionSet+Action`
  after release.

## Session

- Created: 2026-10-05 (cockpit, from timewarp-architecture 275 research)
