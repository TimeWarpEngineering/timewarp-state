# CatalogAction DisplayName for human-facing labels

## Description

`[CatalogAction]` has `Name`, the catalog identifier (default `<State>.<ActionSet>`, e.g.
`Credentials.LinkMicrosoft365`), and `Description`. It has nothing for a **human-facing label**,
so consumers derive one from the identifier. In timewarp-architecture the Ctrl-K palette turns
`Credentials.LinkMicrosoft365` into "Credentials: Link microsoft 365", which loses brand casing.
Maintainer decision (2026-10-01): a palette label is user-facing copy and should be authored, not
derived.

Add an optional `DisplayName` to `[CatalogAction]` and surface it on `ActionCatalogEntry`.

## Requirements

1. `CatalogActionAttribute.DisplayName` (`string?`). The XML doc says it is the human-facing
   label for UIs such as command palettes and menus, and that `Name` stays the stable identifier.
2. `ActionCatalogEntry` exposes `DisplayName` (`string?`), copied by the generator from the
   attribute literal. It stays reflection-free, like the rest of the catalog. Null when not set.
   Do **not** synthesize a fallback inside TimeWarp.State; consumers decide how to fall back.
3. Analyzer, in the TWS0004–TWS0007 family, matching how `Name` and `Description` are validated:
   - `DisplayName` must be a compile-time constant when given;
   - an empty or whitespace `DisplayName` is an error.
4. Tests: generator emission with and without DisplayName, entry round trip, and the analyzer
   cases.
5. Release as the next 12.0.0 beta. Bump the version and pins in the same commit, per this repo's
   release policy, and cut the release with `dev release` after merge, following the `tw-release`
   skill.

## Checklist

- [x] `DisplayName` on the attribute and on `ActionCatalogEntry` (generator-emitted, no reflection)
- [x] Analyzer coverage for constant and non-empty values
- [x] Tests (generator, entry, analyzer)
- [x] XML docs on the public surface
- [x] Version bump (next 12.0.0 beta); gates per `tw-pr`
- [x] Preserve the Blazor JS initializer filename `Test.App.Client.lib.module.js`. If
      `ganda repo audit --fix` lowercases it, revert that rename.
- [ ] Implementation review; host `open-pr`

## Session

- Created: 14268 (2026-10-01)
- 2026-10-01: implement oracle (ganda task work, Claude) — attribute, entry, generator, TWS0008, tests, docs, beta.7 bump.

## Notes

- Consumer follow-up after release: timewarp-architecture pins the new beta, sets
  `DisplayName` where generated labels read badly (starting with "Link Microsoft 365"), and the
  palette uses `DisplayName ?? <generated>`. That work gets its own task in that repo once this
  ships.

## Results

- `CatalogActionAttribute.DisplayName` (`string?`) with XML docs: human-facing label for palettes and menus;
  `Name` stays the stable identifier; no fallback is derived.
- `ActionCatalogEntry.DisplayName` (`string?`), set via a trailing optional `displayName` constructor
  parameter (source compatible with existing positional callers). The generator copies the attribute
  literal and always emits `displayName: "…"` or `displayName: null` — no reflection, no synthesized fallback.
- Analyzer **TWS0008** (Error, `CatalogActionAnalyzer`): `DisplayName` set to empty or whitespace. Explicit
  `null` equals unset. Compile-time constancy is enforced by the C# compiler (CS0182), which a test pins;
  no separate rule is needed. Added to `AnalyzerReleases.Unshipped.md`, analyzer readme and
  `documentation/topics/action-catalog.md`.
- Tests: generator emission with DisplayName (escaped literal) and without (`null`); expected snapshot updated;
  client integration round trip (`Counter.AddToCount` now has `DisplayName = "Add to Count"`,
  `EventStream.AddEvent` stays null, direct constructor round trip); analyzer TWS0008 empty, whitespace,
  const-field OK, non-constant → CS0182, explicit `null` OK.
- Version 12.0.0-beta.6 → **12.0.0-beta.7** (`msbuild/repository.props` + `source/Directory.Build.props`),
  release notes `release12.0.0-beta.7.md` + toc.
- `ganda repo audit`: passes; the only advisory is the kebab warning for `Test.App.Client.lib.module.js`,
  kept on purpose (Blazor JS initializer name).
- Release (`dev release` per `tw-release`) happens after merge — not done here.

### How to validate

**Smoke:**

```bash
dotnet tool restore && dotnet run --file scripts/test.cs
ganda repo audit
```

**Expect:**

- All suites green: analyzer 33 passed, source generator 15 passed, Client.Integration 56 passed (1 skipped),
  including `Should_Report_TWS0008.*`, `Should_Emit_DisplayName.*`,
  `Registry_Should.Leave_DisplayName_Null_When_Not_Set`, `Catalog_Should.Round_Trip_DisplayName_Through_Constructor`.
- `ganda repo audit` reports "Repository passes" (only the advisory kebab warning on
  `Test.App.Client.lib.module.js`).
- `[CatalogAction(Description = "x.", DisplayName = " ")]` fails the build with TWS0008.
