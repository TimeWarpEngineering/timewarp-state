# Disposition — task 097

**Date:** 2026-10-10
**Outcome:** clean
**Rounds:** 4 (rounds 1–3 by the general reviewer, round 4 an orchestrator check)
**Final open count:** 0

## Summary

The review ran at effort 3 with a general reviewer. Reviewers built throwaway consumer projects (`/tmp/rv097*`) to see what the generator actually emits.

- **Round 1 (M1–M14):** 11 bugs, 2 suggestions, 1 nit.
  - Clone code broke for fields declared on generic types.
  - Interface collections did not compile.
  - BCL values behind interfaces threw at runtime.
  - Types from other assemblies with private state came back silently empty.
  - Polymorphic members were sliced to their declared type.
  - Constructor arguments were ambiguous, `required` members on base types failed, and `ImmutableStack` came back reversed.
  - Generated code used `System.Linq` without qualifying it.
  - IL warnings were not listed, coverage was missing, the pipeline did no caching, diagnostics were noisy, and generated names could collide.
- **Round 2 (M15–M20):** 3 bugs and 3 nits.
  - Hidden subtypes threw at runtime instead of failing the build.
  - Hierarchies split across assemblies did not clone.
  - `ICloneable` subtypes hit a cast error.
  - `file` types, the reference-assembly hint and a trim justification were wrong.
- **Round 3 (M21):** generic subtypes of generic members threw at runtime.

All 21 findings are fixed on this task. Fix commits are ab291e16, 858de934 and 36208023, each re-verified by probe. There are no wontfix items.

## Exception log (if accepted-exceptions)

None.

## Escalations

None.

## Notes

- Known documented limitation, which the requirements allow: a subtype declared in an assembly the project does not reference, and that does not implement `ICloneable`, throws `InvalidOperationException` at clone time (cloning.md, "Polymorphic members").
- Behaviour change: a DTO from a `ProjectReference` is compiled against a reference assembly that hides private fields, so it reports TWSG002 unless it has only auto-properties or its project sets `ProduceReferenceAssembly=false`. This is documented in cloning.md, the migration guide and the release notes.
- `sample-04-server` fails restore with NU1102 (TimeWarp.State.Telemetry 12.0.0-beta.11). That is expected from the version bump: samples consume the packed local feed. It is not a finding.
