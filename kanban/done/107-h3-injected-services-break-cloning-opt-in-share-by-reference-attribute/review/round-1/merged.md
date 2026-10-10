# Round 1 — merged findings
**Date:** 2026-10-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 1 | 2 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: tests/timewarp-state-source-generator-tests/state-clone-shape-tests.cs
- Description: No test covers `[CloneShared]` winning over ignore attributes, `[field: CloneShared]`, or a private base-class field.
- Suggestion: Add a shape test.
- Source: general
- Disposition notes: Added the `CloneSharedPrecedence` shape (`[CloneShared, IgnoreDataMember]` field, `[CloneShared, JsonIgnore]` init property, `[field: CloneShared]`, `[CloneShared]` private field on a generic base) to `Given_Shape` and `Given_CloneShared_Wins_Over_Ignore_Attributes`. Generator suite: 80 passed.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-source-generator/state-clone-planner.cs (FindMetadataProblem hidden-field loop)
- Description: A hidden private field in a metadata base that is marked `[CloneShared]` is reported as TWSG002 with advice to add `[CloneShared]`. With an ignore attribute too, it is silently skipped, which contradicts "CloneShared wins".
- Suggestion: Check CloneShared before the ignore skip and report it accurately.
- Source: general
- Disposition notes: The loop now checks `[CloneShared]` (on the field or the hidden property) before the ignore skip. It returns "has private member 'X' marked [CloneShared] that generated code cannot see".

### M3 — Severity: nit — Status: fixed
- File: state-clone-planner.cs:1189 / documentation/topics/cloning.md
- Description: `[CloneShared]` on a property that is not an auto-property is silently ignored.
- Suggestion: Document that the attribute goes on the backing field.
- Source: general
- Disposition notes: Added a sentence to cloning.md "What is copied".

### M4 — Severity: nit — Status: wontfix
- File: state-clone-planner.cs:1253
- Description: A struct with a shared member gets a redundant assignment and a Clone method instead of value sharing.
- Source: general
- Disposition notes: Output is correct. The cost is negligible, and the shared path stays uniform for classes and structs. Decided by review orchestrator.

### M5 — Severity: nit — Status: wontfix
- File: state-clone-planner.cs:1904
- Description: The attribute is matched by simple name in any namespace.
- Source: general
- Disposition notes: Intentional. It matches the existing ignore-attribute convention ("any namespace"). Decided by review orchestrator.

## Duplicates / conflicts

- None (single reviewer).
