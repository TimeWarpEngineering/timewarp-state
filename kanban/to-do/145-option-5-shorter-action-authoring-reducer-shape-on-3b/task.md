# Option 5: shorter action authoring

## Description

Source: task 140 `report/analysis.md`, option 5. Read it, then read what task 144 (done) delivered and its PR #675.

Task 144 already ships the short shape: `[Reducer]` on the nested `Action` plus one
`private static void Reduce(TState state, Action action)` in a partial action set. The State generator emits the
`Handler`, the dispatch method, the catalog entry and the manifest entry from that one action fact, TWS0014 reports a
`[Reducer]` it will not serve, and `CounterState.DecrementCountActionSet` in the test app proves it end to end.

This task finishes option 5 on top of that. It adds no generator or attribute work. If you find that a generator or
analyzer change is needed, stop and report in `## Notes`.

## Requirements

- Documentation: the `[Reducer]` shape is the recommended way to write an action. Show the full long shape and the
  short shape side by side in the topic that teaches actions (`documentation/topics/`), state what the generator
  emits for each, and when a hand-written `Handler` is still the right choice (async work, injected services,
  anything beyond a synchronous state mutation). The long shape keeps working and stays documented. Update
  `readme.md`, `claude.md` and `documentation/partials/summary.md` where they describe action authoring.
- Analyzers: prove every State analyzer accepts the short shape. Add analyzer tests for TWS0001 (nesting),
  TWS0004 to TWS0008 (catalog) and TWS0010/0011 (inheritance) against a `[Reducer]` action set, including a
  `[Reducer]` action that also carries `[CatalogAction]`. Any analyzer that rejects the shape is a finding: fix it
  and record it in `AnalyzerReleases.Unshipped.md` and the release note.
- Samples: migrate at least one state in a sample (`samples/`), not the test app, to `[Reducer]` as the worked
  example. Keep at least one long-shape action in the same sample so readers see both.
- Release note: the unreleased `release12.0.0-beta.17.md` gets an "Authoring" section for the short shape.
- No TimeWarp.State version bump: 12.0.0-beta.17 is unreleased and this joins it.

## Checklist

- [ ] Docs: side-by-side shapes, guidance on when to hand-write a Handler, readme/claude.md/summary updated
- [ ] Analyzer tests for the short shape across TWS0001, TWS0004-0008, TWS0010/0011; fixes if any
- [ ] One sample state migrated; build 0/0; tests pass; `ganda repo audit` clean

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)
- 2026-11-06: scope trimmed after 144 merged (PR #675); generator work removed from this task. Steve approved the trim.

## Depends on

- Task 144 (done, PR #675)

## Notes

Steve 2026-11-06 decision; order 3b → 5 → 4. Architecture task 483 (option 4) depends on this task.
