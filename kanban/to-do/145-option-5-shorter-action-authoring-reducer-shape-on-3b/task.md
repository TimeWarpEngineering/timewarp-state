# Option 5: shorter action authoring

## Description

Source: task 140 `report/analysis.md` (options as code, costs, what each unblocks). Read it first. Option 5. Needs 3b.

About 25 lines per action today (`counter-state.increment-counter.cs`) down to about 3: the developer writes the action and a `Reduce` body; the generator emits the handler, dispatch method, and catalog entry.

## Requirements

- New authoring convention in State's documentation and analyzers; TWS0001 nesting rule accepts the second shape.
- Clone and registration paths unchanged.
- Sample and test app migrated for at least one state as the worked example; old shape keeps working.

## Checklist

- [ ] Convention, analyzer update, docs
- [ ] Generator emits handler, dispatch, catalog from the short shape
- [ ] Sample migrated; build 0/0; version bump

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)

## Depends on

- Option 3b (State side) task

## Notes

Steve 2026-11-06 decision; order 3b → 5 → 4.
