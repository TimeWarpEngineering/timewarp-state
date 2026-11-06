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

## Goal (implementer restatement)

Make the `[Reducer]` shape from task 144 the recommended way to write an action. Prove the existing analyzers accept
it, and give readers a worked example in a real sample. No generator or attribute changes. If one turns out to be
needed, stop and record it in `## Notes`.

Acceptance criteria:

- A topic in `documentation/topics/` teaches actions. It shows the long shape (`Action` plus a hand-written
  `Handler`) and the short shape (`[Reducer]` plus `static void Reduce(TState, Action)`) side by side. It says what
  the generator emits for each shape, and says to hand-write a `Handler` for async work, injected services, or
  anything beyond a synchronous state mutation. The topic is in `toc.yml`. `readme.md`, `claude.md` and
  `documentation/partials/summary.md` point to the short shape where they describe action authoring.
- Analyzer tests run TWS0001 (nesting), TWS0004 to TWS0008 (catalog) and TWS0010/TWS0011 (inheritance) against
  `[Reducer]` action sets. One test uses a `[Reducer]` action that also has `[CatalogAction]`. Any false positive is
  fixed and recorded in `AnalyzerReleases.Unshipped.md` and the release note.
- One state in a `samples/` app uses `[Reducer]`. Another action in the same sample keeps the long shape.
- `release12.0.0-beta.17.md` gets an "Authoring" section. The TimeWarp.State version stays the same.
- The build has 0 errors and no new warnings, tests pass, samples verify, and `ganda repo audit` is clean.

## Checklist

- [x] Docs: side-by-side shapes, guidance on when to hand-write a Handler, readme/claude.md/summary updated
- [x] Analyzer tests for the short shape across TWS0001, TWS0004-0008, TWS0010/0011; fixes if any (none needed)
- [x] One sample state migrated; build 0 errors (no new warnings); tests pass; `ganda repo audit` clean

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)
- 2026-11-06: scope trimmed after 144 merged (PR #675); generator work removed from this task. Steve approved the trim.
- 2026-11-06: implementer (ganda task work, implement oracle, claude-opus-5-5). Added the Actions topic, the analyzer
  tests for the short shape and the sample 05 migration, and updated the docs and release note. No analyzer rejected the
  short shape, so no analyzer, generator or attribute code changed. `ganda repo audit` passes. It shows the same 2
  advisories as task 144: kebab-path-names is set to warning on purpose in `.editorconfig` (Blazor `lib.module.js`
  names), and kanban-landed is task 098's claim. Neither belongs to this task.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 8cb3ca8e-d72c-4713-923f-c7fb9a8856af, max-turns 500, Done — 2026-11-06T04:13:45Z

## Results

`[Reducer]` is now the documented, recommended way to write an action. The analyzers have tests for it, and a sample
uses it. No generator, analyzer or attribute code changed.

**Docs**

- New `documentation/topics/actions.md` (uid `TimeWarpState:Actions.md`), listed first in `topics/toc.yml`. It shows
  the same `DecrementCount` action in the short and long shapes. A table says what you write and what the generator
  emits for each: the `Handler` for the short shape only, and the same dispatch method, catalog entry (with
  `[CatalogAction]`) and manifest entry for both. It also says how Mediator finds each handler. It shows the generated
  `Handler`. It says when to hand-write the `Handler`: async work, injected services, the cancellation token, or more
  than a synchronous change to one state. It gives the steps to convert either way, the TWS0014 rules, a note that the
  other analyzers treat both shapes the same, and a link to sample 05.
- `readme.md`: a new "Writing an action" section shows the short shape and points to the long shape and the topic.
- `claude.md`: a new "Action authoring" bullet under Key Patterns. CRLF line endings are kept.
- `documentation/partials/summary.md`: a paragraph on `[Reducer]` with a link to the topic.
- `documentation/topics/analyzers.md`: TWS0001 says it applies to both shapes, and TWS0014 links to the topic.
- `release12.0.0-beta.17.md`: a new "Authoring: the short action shape (task 145)" section. The version is unchanged.

**Analyzer tests** (`tests/timewarp-state-analyzer-tests/reducer-shape-analyzer-tests.cs`, 13 tests)

- Should not report: TWS0001, the catalog rules, the inheritance rules, and TWS0014 on a `[Reducer]` action that also
  has `[CatalogAction]`. Each runs twice: on the source as written, and with the generator's `Handler` added as a
  `.g.cs` file. A fifth test puts the long and short shapes in one state and runs all four analyzers.
- Still reports: TWS0001 (a `[Reducer]` action set outside a state), TWS0004 (generic state), TWS0005, TWS0006 (two
  `[Reducer]` actions with one catalog name), TWS0007, TWS0008, TWS0010 and TWS0011, each on a `[Reducer]` action.
- Finding: none. Every analyzer accepts the short shape and still reports real faults in it, so
  `AnalyzerReleases.Unshipped.md` is unchanged.

**Sample** (`samples/05-persistence`)

- `DisplayPreferencesState.SetAccent` and `ToggleCompact` now use `[Reducer]` in `static partial` action sets, with
  no hand-written `Handler`. `DraftNoteState.UpdateText` and `Clear` keep the long shape. The sample readme has a new
  "Two action shapes" section. `Home.razor` is unchanged because the generated `SetAccent(...)` and
  `ToggleCompact()` keep their signatures.
- The generator emits `...SetAccentActionSet.Handler_Reducer.g.cs` and `...ToggleCompactActionSet.Handler_Reducer.g.cs`,
  and `Sender_ClientPipeline.g.cs` dispatches to `SetAccentActionSet.Handler`.

**Gates run**

- `./bin/dev build`: exit 0, 0 errors. The 151 warnings are the existing baseline (TW0007, BL0010/BL0016, NU1510 and
  others). None is in a file this task touched.
- `./bin/dev test`: exit 0. All 9 suites pass. The analyzer suite went from 77 to 90 tests.
- `./bin/dev verify-samples`: exit 0.
- `ganda repo audit`: passes, with the 2 advisories noted under Session.

**Not done**

- Nobody ran sample 05 in a browser. Task 144's client integration tests already prove that a generated `Handler`
  dispatches end to end. In this sample, the dispatch wiring was checked in the generated Mediator code.

### How to validate

Smoke:

```bash
./bin/dev build
dotnet test tests/timewarp-state-analyzer-tests/timewarp-state-analyzer-tests.csproj --no-build -v n | grep -E "ReducerShapeAnalyzers_|Passed!|Failed!"
./bin/dev verify-samples
ls samples/05-persistence/wasm/sample-05-wasm/obj/Debug/net11.0/generated/timewarp-state-source-generator/*/ | grep Handler_Reducer
grep -o "DisplayPreferencesState.SetAccentActionSet.Handler" samples/05-persistence/wasm/sample-05-wasm/obj/Debug/net11.0/generated/TimeWarp.Mediator.Generators/*/Sender_ClientPipeline.g.cs | head -1
```

Expect: the build exits 0 with 0 errors. The analyzer suite passes 90 tests, including 13 `ReducerShapeAnalyzers_`
tests. verify-samples prints "Samples verified successfully!". The generated folder lists
`DisplayPreferencesState+SetAccentActionSet.Handler_Reducer.g.cs` and `+ToggleCompactActionSet.Handler_Reducer.g.cs`.
The Sender grep prints the handler name. In the docs, `documentation/topics/actions.md` shows both shapes and the
table of what each generates. If you add a hand-written `Handler` to
`display-preferences-state.set-accent.cs`, the build fails with TWS0014 on `[Reducer]`.

## Depends on

- Task 144 (done, PR #675)

## Notes

Steve 2026-11-06 decision; order 3b → 5 → 4. Architecture task 483 (option 4) depends on this task.
