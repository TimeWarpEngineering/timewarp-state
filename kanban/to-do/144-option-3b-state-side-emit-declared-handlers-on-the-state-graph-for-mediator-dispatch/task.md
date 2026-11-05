# Option 3b (State side): emit declared handlers on the State graph

## Description

Source: task 140 `report/analysis.md` (options as code, costs, what each unblocks). Read it first. Option 3b, State half. Mediator half is a timewarp-mediator task filed the same day (`GeneratedHandlerAttribute`).

State adds `ReducerAttribute : GeneratedHandlerAttribute` (HandlerTypeName "Handler") and emitters on the Option 1 graph that generate the handler Mediator will dispatch to. A State analyzer reports a declared handler the generator did not emit before Mediator's CS0246 can.

## Requirements

- Depends on the Mediator release that ships `GeneratedHandlerAttribute`; bump the Mediator pin in the same commit.
- Emitters on the Option 1 graph; no second scan.
- Analyzer for the missing-handler case with release tracking.
- Tests: generated handler dispatches end to end in the test app.

## Checklist

- [ ] Attribute and emitters
- [ ] Analyzer
- [ ] End-to-end test; build 0/0; pin bump; version bump

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)

## Depends on

- Option 1 task; timewarp-mediator Option 3b task

## Notes

Steve 2026-11-06 decision; order 0 → 1 → 3b.
