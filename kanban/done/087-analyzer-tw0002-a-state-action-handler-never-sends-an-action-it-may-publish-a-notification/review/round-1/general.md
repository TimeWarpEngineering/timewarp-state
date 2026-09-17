# Round 1 — general
**Date:** 2026-09-17
**Scope reviewed:** branch `task/087-analyzer-tw0002-a-state-action-handler-never-sends` vs `origin/master` (commit `d0e8f692`)

## Summary

Adds TW0002 (`HandlerMustNotSendAction`, Design/Warning) and TW0003 (`AllowActionSend` Info) so state action handlers cannot dispatch actions, with a documented escape hatch. Handler detection is symbol-based (base chain + `IActionHandler<>`), dispatch covers `ISender`/`IMediator`/`ISender<>` Send of `IAction` and `{MethodName}ActionSet` entry methods on `IState`, and the test matrix plus docs (including the known TW0002 ID collision) match the brief. Risk is low: false positives from Publish, own-state mutation, generated wrappers, and pipeline behaviors are avoided by design.

## Issues

No issues found.
