# Round 1 — general
**Date:** 2026-10-11
**Scope reviewed:** master...HEAD (19815c99)

## Summary
The change is small and correct for the main path. `[CloneShared]` on a field, on an auto-property, or via `[field: CloneShared]` skips `Build(member.Type)`. That means no classification and no TWSG002 for the member type, and the generated code emits a plain `UnsafeAccessor` ref-field assignment. This works for readonly fields, getter-only/init-only backing fields, base-class private fields declared in source, and records. `AttributeUsage` (Field | Property) matches what the planner reads. TWSG002 still fires for unmarked services, and a test covers it. No analyzer needs to know about the attribute. Both suites pass (generator 78/78, state 106 passed / 1 skipped). The findings below are edge cases and test gaps, not regressions.

## Issues
### Issue 1 — Severity: suggestion
- File: tests/timewarp-state-source-generator-tests/state-clone-shape-tests.cs:362
- Description: The docs (cloning.md, migration, release notes, the attribute XML remarks and the planner Design region) say "`[CloneShared]` wins when a member has both". No test covers that. There are also no tests for `[CloneShared]` on a non-state `[GenerateClone]` type, on a struct member, on a private field of a source base class, or written as `[field: CloneShared]`. All new tests use public auto-properties (plus one private readonly field) declared directly on a `State<T>`.
- Suggestion: Add a generator or runtime test with `[CloneShared, IgnoreDataMember]` (or `[JsonIgnore]`) on a service member, and assert that the clone holds the same instance. Optionally add one case for a `[GenerateClone]` class or a base-class member.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1662
- Description: `FindMetadataProblem` checks hidden private fields of a base type from a referenced, non-TimeWarp assembly, and skips only `HasIgnoreAttribute` (lines 1662 and 1677). Two edge cases follow:
  - A hidden private field marked only `[CloneShared]` still produces TWSG002 ("has private field ... that generated code cannot see"). The new message then tells the user to "mark the member [CloneShared]", which they already did.
  - A hidden field marked with both `[CloneShared]` and an ignore attribute is silently left at its constructor value. That contradicts the documented "CloneShared wins".

  This is a narrow case: a state base class in a sibling library built as a reference assembly.
- Suggestion: In `FindMetadataProblem`, either give a `[CloneShared]` hidden field a specific problem text (for example "is marked [CloneShared] but generated code cannot see it; set ProduceReferenceAssembly=false"), or document that `[CloneShared]` needs the field to be visible, like any other copied field.
- Status: open

### Issue 3 — Severity: nit
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1189
- Description: `[CloneShared]` on a property that is not an auto-property (a manual backing field) is silently ignored. `EnumerateAutoProperties` filters it out, and the explicit backing field has no `AssociatedSymbol`, so `IsCloneShared(field)` does not see the property attribute. The backing field is deep-cloned, or reported as TWSG002 at the field, so the build does not end up silently wrong. The docs do say "field or auto-property", but nothing warns the user who puts the attribute on the property.
- Suggestion: Add a sentence to cloning.md saying the attribute must go on the backing field for a manually implemented property. Optionally have the generator warn when the attribute is on a non-auto property.
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1253
- Description: For a value type, `clone = source` already copies every field, so `TryAppendSharedMember` adds a redundant assignment. It also sets `copied = true`, so a struct whose only non-trivial member is `[CloneShared]` gets a Clone method instead of `SlotKind.Share`. For comparison, `TryAppendMember` returns early when the member is shared inside a value type (line 1236). The output is still correct.
- Suggestion: In `TryAppendSharedMember`, return `true` without emitting anything when `type.IsValueType`.
- Status: open

### Issue 5 — Severity: nit
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1904
- Description: `HasCloneSharedAttribute` matches by simple name (`CloneShared` or `CloneSharedAttribute`) in any namespace, not by the metadata name `TimeWarp.State.CloneSharedAttribute`. cloning.md names the type as `TimeWarp.State.CloneSharedAttribute`. This is consistent with how the ignore attributes are matched, and it is harmless in practice. Note that the `"CloneShared"` arm only matches a class literally named `CloneShared`.
- Suggestion: Either match the full metadata name, or keep name matching and drop the redundant `"CloneShared"` arm. Optional.
- Status: open
