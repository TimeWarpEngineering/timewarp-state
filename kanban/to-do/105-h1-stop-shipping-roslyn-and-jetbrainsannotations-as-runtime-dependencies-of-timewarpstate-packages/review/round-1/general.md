# Round 1 — general
**Date:** 2026-10-10
**Scope reviewed:** branch vs master, all non-kanban files plus the task.md Results

## Summary

The change removes the repo-wide `Microsoft.CodeAnalysis.CSharp` PackageReference. It references Roslyn with `PrivateAssets="all"` only from the analyzer, the generator, and their test projects, and makes `JetBrains.Annotations` private on State and Plus. A new `NuspecDependencyAllowList` runs inside `dev pack`, which CI runs through `workflow` → `PackCommand.Handler`, so regressions fail CI. The allow-list fails closed for unknown package ids. Risk is low. Two minor findings.

## Issues

### Issue 1 — Severity: nit
- File: tests/timewarp-state-tests/packaging/nuspec-dependency-allow-list-tests.cs:73
- Description: `PinRoslynAtTheConsumerCompilerFloor` checks `Directory.Build.props` with two literal strings. Any other form slips through, for example `<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version=...>`, a child-element form, or different whitespace.
- Suggestion: parse the props XML and assert that no `PackageReference` has `Include="Microsoft.CodeAnalysis.CSharp"`.
- Status: open

### Issue 2 — Severity: nit
- File: kanban/.../task.md (Results, How to validate)
- Description: The Results call `./bin/dev` a "checked-in" binary. `bin/` is not tracked by git. It is a locally built binary.
- Suggestion: change the wording to "locally built".
- Status: open
