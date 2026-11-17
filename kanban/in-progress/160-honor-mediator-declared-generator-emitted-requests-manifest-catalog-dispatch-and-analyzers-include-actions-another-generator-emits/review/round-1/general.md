# Round 1 — general
**Date:** 2026-11-17
**Scope reviewed:** branch `task/160-honor-mediator-declared-generator-emitted-requests` vs `master` (commit `a3e5420b` and the kanban land commit). Generator facts and extractor, emitters, graph dedupe, manifest parse, DevTools badge, TWS0015, tests, sample 09, and the recorded gates.

## Summary

State reads Mediator `GeneratedRequestPatternAttribute` (and derived attributes), places a request only when it expands to `<container>.<Name>ActionSet.Action` on a non-generic `IState` in the same assembly, and emits an internal dispatch method that sends the Action instance, a catalog entry taken from the contract's attributes, and a manifest row with `origin` `declared`. TWS0015 reports the action when it is still missing after generators run. The template reader matches Mediator 15.0.0-beta.10's `RequestPatternReader` (decompiled with `ilspycmd -t TimeWarp.Mediator.Analyzers.RequestPatternReader` from the `timewarp.mediator.generators` 15.0.0-beta.10 analyzer, to check the mirrored rules against the package). The recorded `## Results` gates cover the change: `./bin/dev build` 0/0, `./bin/dev test` with the new declared-action cases, pack, `dev check-version`, and a clean `ganda repo audit` after the gitignored window-icon fix. The restatement matches the task. No issues.
