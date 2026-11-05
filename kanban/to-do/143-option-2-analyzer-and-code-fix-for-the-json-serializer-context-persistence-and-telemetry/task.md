# Option 2: analyzer and code fix for the JSON serializer context

## Description

Source: task 140 `report/analysis.md` (options as code, costs, what each unblocks). Read it first. Option 2. Independent of the others.

Where output must be seen by a generator TimeWarp does not own (System.Text.Json), use an analyzer and code fix that write into user source. The analyzer reports each state that persistence or telemetry needs but no `JsonSerializerContext` lists; the code fix adds the `[JsonSerializable]` line.

## Requirements

- One TWS analyzer, one code fix, tests for both, release tracking.
- `AddTimeWarpState` guidance for wiring the context as `TypeInfoResolver`.
- Telemetry today silently skips a state with no type info (`state-snapshot-recorder.cs`); the rule must cover that path.
- No generator change.

## Checklist

- [ ] Analyzer and code fix with tests
- [ ] Documentation updated
- [ ] Build 0/0; release tracking

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)

## Notes

Steve 2026-11-06 decision; can run alongside Option 1.
