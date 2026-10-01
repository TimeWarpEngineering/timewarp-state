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

- [ ] `DisplayName` on the attribute and on `ActionCatalogEntry` (generator-emitted, no reflection)
- [ ] Analyzer coverage for constant and non-empty values
- [ ] Tests (generator, entry, analyzer)
- [ ] XML docs on the public surface
- [ ] Version bump (next 12.0.0 beta); gates per `tw-pr`
- [ ] Preserve the Blazor JS initializer filename `Test.App.Client.lib.module.js`. If
      `ganda repo audit --fix` lowercases it, revert that rename.
- [ ] Implementation review; host `open-pr`

## Session

- Created: 14268 (2026-10-01)

## Notes

- Consumer follow-up after release: timewarp-architecture pins the new beta, sets
  `DisplayName` where generated labels read badly (starting with "Link Microsoft 365"), and the
  palette uses `DisplayName ?? <generated>`. That work gets its own task in that repo once this
  ships.

## Results

*(fill when done)*

### How to validate

*(required before done)*
