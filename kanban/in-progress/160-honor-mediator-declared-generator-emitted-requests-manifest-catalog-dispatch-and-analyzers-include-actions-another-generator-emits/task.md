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

## Goal (implementer restatement)

Another source generator can emit `RoleState.CreateRoleActionSet.Action` (and its handler) into the host. State's
generator cannot see that output, so today the action gets no dispatch method, no catalog entry, no manifest row, and
no analyzer coverage. Mediator 15.0.0-beta.10 (task 043) added `GeneratedRequestPatternAttribute`, which a source type
carries to declare those requests by name templates plus contract types. Make State read that declaration (and
derived attributes) with 043's placeholder rules, and treat each declared request shaped
`<State>.<Name>ActionSet.Action` as an action of that state.

Acceptance:

- Mediator beta.10 is pinned here, and its released surface fits this spec (or I stop and report).
- For each declared action, the generator emits a dispatch method, a catalog entry (Description and Permissions from
  the contract's existing attributes, else empty; no new State attribute), and a manifest row with origin
  `declared`, handler, and response. The manifest JSON gains `origin`; `StateManifest.Parse` and the DevTools
  manifest tab read it.
- A new analyzer rule reports a declared action missing from the final compilation. The analyzer's Design region
  records how each TWS rule treats declared actions. `AnalyzerReleases.Unshipped.md` lists the rule.
- Tests: a sibling test generator emits the Action and handler; the dispatch method, catalog entry, and manifest row
  are present and the output compiles; the analyzer fires when the declared type is missing; a test pins the
  template rules to 043's documented set.
- One sample pairs a sibling generator with a declaration.
- `dev build` 0/0, `dev test`, `dev pack`, `ganda repo audit` clean, `dev check-version` passes.
- No edits to timewarp-mediator or timewarp-architecture.

## Checklist

- [x] Depends on Mediator 043 released and pinned here (do not start until the pin lands; stop and report if the released surface differs from this spec)
- [x] Generator expands declared requests under state types into actions; dispatch, catalog, and manifest entries emitted; manifest origin field added; `StateManifest.Parse` and DevTools manifest tab updated
- [x] Analyzer rule for a declared action missing from the final compilation; per-rule disposition of TWS0001-TWS0012 for declared actions recorded in the analyzer Design region; `AnalyzerReleases.Unshipped.md`
- [x] Tests: a test generator emits the action and handler; dispatch method, catalog entry, and manifest entry present; analyzer fires on a missing declared type
- [x] Sample: one sample with a sibling generator and a declaration
- [x] `dev build` 0/0, `dev test`, `dev pack`, `ganda repo audit` clean, `dev check-version` (next beta; library surface change)
- [x] No cross-repo work: do not edit timewarp-mediator or timewarp-architecture; stop and report instead

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-17)
- 2026-11-17 implementer (ganda task work, implement oracle): Mediator 15.0.0-beta.10 (contains 043, merge b15c0aa)
  is on nuget.org; pinned it. Built the declared-action reader, extractor, emitter changes, manifest fields, TWS0015,
  tests, sample 09, and docs. Gates under Results.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-17T11:22:05Z

## Results

### Mediator 043 surface versus this spec

Mediator shipped `TimeWarp.Mediator.GeneratedRequestPatternAttribute(requestTypeName, handlerTypeName,
requestInterfaceName, params Type[] contracts)`, `AllowMultiple = true`, `Inherited = false`, with placeholders
`{Host}`, `{Contract}`, `{ContractType}`. Derived attributes give the templates as constructor parameters of the same
name or as `const string` fields. The spec called it a working name and left the name and placeholders to 043, so it
fits. One difference: 043 declares the request interface (`requestInterfaceName`) instead of a response type. State
reads the response from that interface (`IRequest<T>`'s `T`, or none for `IRequest`/`IAction`) for the manifest.
Mediator's reader ships inside its analyzer assembly, which State cannot reference, so State mirrors the rules
(`facts/declared-action-facts*.cs`) and `Should_Expand_Templates_Like_Mediator` pins them.

### What a declared action gets

A declared request counts when it expands to `<container>.<Name>ActionSet.Action`, the container is a non-generic
`IState` type in this assembly, and the request is not already in source (source wins; duplicates are dropped by type).

- **Dispatch method:** `internal async Task <Name>(<Name>ActionSet.Action action, CancellationToken? externalCancellationToken = null)`,
  which sends the given Action. State cannot see the Action, so it does not know its constructor. Taking the instance
  is the only signature it can emit without knowing anything about the contract. `internal` because the Action's
  accessibility is unknown and `public` could be CS0051. The emitting generator can add a typed public overload.
  The method is skipped when the state already has a member with that name or the state chain is not partial.
- **Catalog entry:** default name `<StateWithoutSuffix>.<Name>`, one `action` parameter (a host passes an Action
  instance). `Description` comes from the contract's `System.ComponentModel.DescriptionAttribute`, otherwise from any
  contract attribute's string `Description` named argument. `Permissions` comes from any contract attribute's
  `string[] Permissions` named argument. Otherwise both are empty. There is no new State attribute.
  `[CatalogAction]` on a contract would still be TWS0004, so it is not a source.
- **Manifest row:** runtime `Type`, `Name` `<Name>ActionSet.Action`, method, the `action` parameter, catalog name,
  `tracked:false`, `origin:"declared"`, `handler` (runtime name), `response`. Source rows now carry
  `"origin":"source","handler":null,"response":null`. Version stays 1, and readers ignore unknown fields.
  `StateManifestAction` gains init properties `Origin`, `Handler`, `Response`, and `IsDeclared`; the positional
  constructor is unchanged. DevTools' Manifest tab shows a `declared` badge whose tooltip names the handler and
  response.
- **TWS0015 (Error, `DeclaredActionAnalyzer`):** a declared action that is missing after every generator ran. Its
  Design region has the per-rule table for TWS0001-TWS0014: 0001 holds by construction; 0002/0003/0005/0006/0007 are
  skipped (generated syntax, or contract-owned text; a 0006 clash still throws in `ActionCatalog` at startup);
  0004/0008/0014 do not apply; 0009-0013 check states and are unchanged.

### Changes

- Generator: `facts/declared-action-facts.cs` and `.templates.cs` (also linked into the analyzer),
  `extractors/declared-action-extractor.cs`, pipeline step `State.Declared` (`TrackingNames.Declared`),
  `StateGraphBuilder` dedupe by action type, `ActionSetMethodModel.SendsActionParameter`, and
  `ManifestActionModel.Origin/Handler/Response`. `ActionSetShape.IsPartial`, `ActionFactExtractor.Keyword`, and
  `ActionFactExtractor.Declaration` are now `internal` so the new extractor can reuse them.
- Runtime: `StateManifest.Parse` and `StateManifestAction` (see above).
- DevTools: `ManifestPanel.razor` and `.razor.css` (declared badge).
- Analyzer: `declared-action-analyzer.cs`, `AnalyzerReleases.Unshipped.md` (TWS0015), analyzer readme.
- Tests: `declared-action-generator-tests.cs` and `declared-action-sources.cs` (17 cases with a `RegisterSourceOutput`
  sibling generator: output compiles, dispatch method text, catalog from contract attributes, manifest rows, derived
  const attribute, source wins, not under a state, existing member, template rules, manifest parse, and a Cached
  `State.Declared` step). `declared-action-analyzer-tests.cs` covers 3 cases: TWS0015 fires; it does not fire when
  the action is emitted or when the request is not under a state. The `state-manifest-tests.cs` expected JSON gains
  the three fields.
- Sample: `samples/09-declared-actions` (`sample-09-generator`, a netstandard2.0 sibling generator, and
  `sample-09-console`, a `[ContractState]` host), added to the slnx and `samples/overview.md`.
- Docs: `documentation/topics/state-manifest.md` (new section and fields), `documentation/topics/analyzers.md`
  (TWS0015), `documentation/release-notes/release12.0.0-beta.18.md` (task 160 section and the Mediator pin), and
  the Mediator version in `readme.md`, `claude.md`, `documentation/overview.md`, and
  `documentation/partials/summary.md`.
- Pins: TimeWarp.Mediator and TimeWarp.Mediator.Generators `15.0.0-beta.9` → `15.0.0-beta.10`.
- Version: 12.0.0-beta.18 is not released yet (nuget latest is beta.17), so there was no bump. `dev check-version`
  passes.

### Gates

- `./bin/dev build`: Build succeeded, 0 Warning(s), 0 Error(s).
- `./bin/dev test`: "Tests completed successfully". Analyzer 93, generator 188, state 153 (+1 skipped), plus 40, 21,
  124, 4, 67, and 7 in the other suites; 0 failed.
- `./bin/dev pack`: exit 0, packed the 12.0.0-beta.18 nupkgs; no tracked files changed.
- `./bin/dev check-version`: "Source is 1 prerelease increment ahead of v12.0.0-beta.17 ... safe to release."
- `ganda repo audit`: first run 43 passed / 1 failed. `vscode-window-icon` failed because `.vscode/settings.json` was missing in this
  fresh worktree. `ganda repo audit --fix` wrote it; the file is git-ignored and local only. Then: Passed 44, Failed 0, Skipped 3, exit 0.

### How to validate

- Smoke: `./bin/dev build && dotnet run --project samples/09-declared-actions/sample-09-console/sample-09-console.csproj -c Release --no-build`
- Expect: it prints `Roles=Editor`, `Catalog=Role.RemoveRole: Remove a role.`, and `Declared=AddRole,RemoveRole`,
  then exits 0. The Actions come from the sibling generator, which neither State's nor Mediator's generator can see.
  `AddRole` dispatches through State's generated `RoleState.AddRole(action)` and Mediator's declared `Send` arm.
  `RemoveRole` dispatches through the generated catalog entry. In the generator tests, `DeclaredAction_` passes
  (17 cases), and in the analyzer tests `DeclaredActionAnalyzer_` passes (3 cases).

### Follow-ups / risks

- Architecture 483 can move its pins to State 12.0.0-beta.18 and Mediator 15.0.0-beta.10 after release. Its
  `[ContractState]` can derive from `GeneratedRequestPatternAttribute`. State's dispatch method takes the Action
  instance, so 483's generator emits any typed overload it wants (for example `CreateRole(string name, ...)`) that
  calls it.
- The catalog `Description` and `Permissions` rule (`[Description]`, or named arguments `Description` /
  `Permissions` on any contract attribute) is my reading of "the contract type's existing attributes". In
  architecture, `[Operation(Description = ...)]` sits on the nested `Command`, not on the contract type, so 483
  may want a `[Description]` on the contract type, or a follow-up that also reads the nested request type.
- TWS0006 does not see declared catalog names. A clash with a source `[CatalogAction(Name = ...)]` is caught by
  `ActionCatalog` at startup.

## Notes

- Origin: timewarp-architecture task 483 "Blocker" (2026-11-07), Steve's option 1 on 2026-11-17.
  Verified at master 0aaa1f92: `state-manifest-attribute.cs:17`, `timewarp-state-generator.cs:38-46,80-127`,
  `action-catalog-provider-attribute.cs:17`, `action-catalog.cs:17,29`, `reducer-attribute.cs:40-41`,
  `emitters/dispatch-method-emitter.cs`.
- Related but not a dependency: backlog 110 (State Plus pluggable action sets).
- Consumer after release: architecture 483 resumes when both 043 and this ship and the pins move.
- Release of the resulting beta is Steve's call; the task stops at the merged PR.
