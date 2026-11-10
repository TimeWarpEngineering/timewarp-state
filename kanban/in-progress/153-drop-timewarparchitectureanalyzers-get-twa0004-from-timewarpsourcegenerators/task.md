# Drop TimeWarp.Architecture.Analyzers; get TWA0004 from TimeWarp.SourceGenerators

## Description

Remove State's reference to `TimeWarp.Architecture.Analyzers`. That package is pulled in only for
TWA0004 (`#region Purpose` on every source file) via `Directory.Build.props` line 47
(`PrivateAssets="all"`). Other TWA severities are configured in `.editorconfig`.

Once `timewarp-source-generators` **034** releases the moved generic rules (including TWA0004),
bump `TimeWarp.SourceGenerators`, delete the Architecture.Analyzers PackageReference, and keep
existing `.editorconfig` / globalconfig TWA settings working under the same IDs.

Acceptance: State has no reference to any Architecture package, and the false Architecture ↔ State
cycle disappears from the Core repo dependency tree (Architecture still depends on State as a
library; State must not depend on Architecture at all).

## Depends on (cross-repo)

Blocked by **timewarp-source-generators 034** release that hosts TWA0004 (and any other generic
rules State currently configures).

Upstream of that: **timewarp-architecture 486** classification + move-out.

## Related tasks

- `timewarp-architecture` **486** — Split TimeWarp.Architecture.Analyzers
- `timewarp-source-generators` **034** — Receive generic TWA rules and release them

## Requirements

1. Wait for SourceGenerators 034 to release a version that includes TWA0004 (same diagnostic ID).
2. Bump the `TimeWarp.SourceGenerators` PackageReference.
3. Remove the `TimeWarp.Architecture.Analyzers` PackageReference from `Directory.Build.props`
   (currently ~line 47) and any related comments that say Architecture owns TWA0004.
4. Keep `.editorconfig` and `msbuild/external-sources.globalconfig` TWA settings working
   (same IDs; no severity regression).
5. Confirm `dotnet build` / analyzer behavior still enforces Purpose regions.
6. Confirm no remaining PackageReference / ProjectReference to any Architecture package.

## Checklist

- [ ] Blocked until source-generators 034 is released
- [ ] Bump TimeWarp.SourceGenerators
- [ ] Remove Architecture.Analyzers from Directory.Build.props
- [ ] Verify .editorconfig TWA settings still apply
- [ ] Verify TWA0004 still fires
- [ ] Grep: no Architecture package references remain
- [ ] Note on Core repo dependency tree: Architecture ↔ State cycle gone

## Session

- Created: 1548987 (2026-11-09)

## Notes

Filed by Jenzi after Steven approved filing the three linked tasks (2026-11-09). State's comment
on the PackageReference already says the intent is TWA0004 only; other TWA rules in .editorconfig
are unused without the analyzer package.
