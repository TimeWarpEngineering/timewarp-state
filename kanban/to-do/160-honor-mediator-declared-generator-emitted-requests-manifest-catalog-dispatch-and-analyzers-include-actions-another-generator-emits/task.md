# Honor Mediator declared generator-emitted requests: manifest, catalog, dispatch, and analyzers include actions another generator emits

## Description

State's generator and analyzers see only actions written in host source (`timewarp-state-generator.cs`
`CreateSyntaxProvider`). An action that **another source generator emits into the same compilation**
is invisible to them, so:

- `[StateManifest]` (`AllowMultiple = false`, written by State's generator) omits it, and "the
  manifest stays true" fails for every generated action.
- The action catalog entry is not emitted (a sibling can register an `ActionCatalogSource`
  itself today, but the entry shape then lives in two places).
- The dispatch method is not emitted (a sibling can emit one in State's shape, same duplication).
- TWS0001-TWS0012 never evaluate it.

timewarp-mediator task 043 adds a Mediator-owned, inheritable `AllowMultiple = true` attribute
on a source type that declares request types another generator will emit, by name pattern plus
contract types (working name `GeneratedRequestPattern`; final name and placeholders come from
043). This task makes State treat each declared request that lives under a State type as an
action of that state, exactly as if it were in source.

Steve chose this path on 2026-11-17 (architecture 483 "Blocker", option 1).

### Shape to build

- State's generator reads the Mediator declaration attribute (and derived attributes) on state
  types and expands each contract into a declared action name using 043's placeholder rules.
  Share the expansion code with Mediator if 043 exposes it in a package State can reference;
  otherwise mirror it and add a test that pins the rules to 043's documented set.
- For each declared action, emit what a source action gets today: the dispatch method
  (`dispatch-method-emitter.cs`), the catalog entry, and the manifest entry (state, action,
  handler, response, origin "declared"). The manifest schema gains an origin field so DevTools
  can show declared versus source actions; `StateManifest.Parse` and the DevTools manifest tab
  accept it.
- Catalog `Description` and `Permissions` for a declared action: take them from the contract
  type's existing attributes when present (the declaring generator owns the contract), else leave
  them empty. Do not invent a new State attribute for this.
- Analyzers: TWS rules that apply to action shape run on declared actions where the information
  is available from the declaration; rules that need the action's syntax are skipped for
  declared actions and the skip is documented per rule. A new rule reports a declared action
  whose expanded type is missing from the final compilation (mirror of 043's analyzer, so the
  consumer learns of drift from either side).
- `[Reducer]` is unaffected; a declared action's handler is emitted by the sibling generator.

Scope boundaries:
- No knowledge of contracts or architecture naming here; State only expands 043's pattern.
- No change to `StateManifestAttribute` multiplicity; the single manifest simply includes
  declared actions.

## Checklist

- [ ] Depends on Mediator 043 released and pinned here (do not start until the pin lands; stop and report if the released surface differs from this spec)
- [ ] Generator expands declared requests under state types into actions; dispatch, catalog, and manifest entries emitted; manifest origin field added; `StateManifest.Parse` and DevTools manifest tab updated
- [ ] Analyzer rule for a declared action missing from the final compilation; per-rule disposition of TWS0001-TWS0012 for declared actions recorded in the analyzer Design region; `AnalyzerReleases.Unshipped.md`
- [ ] Tests: a test generator emits the action and handler; dispatch method, catalog entry, and manifest entry present; analyzer fires on a missing declared type
- [ ] Sample: one sample with a sibling generator and a declaration
- [ ] `dev build` 0/0, `dev test`, `dev pack`, `ganda repo audit` clean, `dev check-version` (next beta; library surface change)
- [ ] No cross-repo work: do not edit timewarp-mediator or timewarp-architecture; stop and report instead

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-17)

## Notes

- Origin: timewarp-architecture task 483 "Blocker" (2026-11-07), Steve's option 1 on 2026-11-17.
  Verified at master 0aaa1f92: `state-manifest-attribute.cs:17`, `timewarp-state-generator.cs:38-46,80-127`,
  `action-catalog-provider-attribute.cs:17`, `action-catalog.cs:17,29`, `reducer-attribute.cs:40-41`,
  `emitters/dispatch-method-emitter.cs`.
- Related but not a dependency: backlog 110 (State Plus pluggable action sets).
- Consumer after release: architecture 483 resumes when both 043 and this ship and the pins move.
- Release of the resulting beta is Steve's call; the task stops at the merged PR.
