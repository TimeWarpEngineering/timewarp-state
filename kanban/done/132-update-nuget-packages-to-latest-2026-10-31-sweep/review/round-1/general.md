# Round 1 — general
**Date:** 2026-10-31
**Scope reviewed:** branch diff vs merge-base with `master` (product paths: `Directory.Packages.props`, `.editorconfig`, `msbuild/external-sources.globalconfig`, `samples/04-telemetry/apphost/sample-04-apphost.csproj`, `tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs`, `tools/dev-cli/**`, `documentation/release-notes/release12.0.0-beta.16.md`) plus task.md Results.

## Summary

The pin bumps match the Results table and the task's baseline. No stale old-version literals remain in csproj, props, cs, json, or yml (grep for 13.6.1, 4.0.0-beta.7, 2.0.0-beta.19, 1.19.1, 18.10.1, 4.5.1; the only hit is the deliberate non-v4 Playwright comment). The Roslyn hold and Blazored holds are recorded with reasons. The dev CLI `Unit.Value` change is complete: no bare `Value` returns or `Unit.Task` references remain in `tools/dev-cli`, and `./bin/dev check-version` runs and reports source `12.0.0-beta.16`, NuGet `12.0.0-beta.15`, safe to release. Turning the new TWA rules off follows the existing policy from task 099-001 (only TWA0004 applies to this library; every other TWA id is set to `none` in the same block), so it is not a suppression used to pass a gate. The one real gap: the list of new rules is incomplete.

Goal restatement matches the task Requirements; no misread.

## Issues

### Issue 1 — Severity: suggestion
- File: .editorconfig:295 (also `.editorconfig:322-329`, `msbuild/external-sources.globalconfig:18-25`, `documentation/release-notes/release12.0.0-beta.16.md:39`, task.md Results "Exceptions")
- Description: The `timewarp-architecture-convention-analyzers.dll` in `artifacts/nuget-packages/timewarp.architecture.analyzers/2.0.0-beta.29` defines 10 ids that beta.19 does not: TWA0025, TWA0026, TWA0027, TWA0028, TWA0032, TWA0033, TWA0034, TWA0035, TWA0036, TWA0037. The change lists and disables 8 of them and leaves out TWA0025 ("Operation-outcome message bars belong to the shell's MessageBars host", a Razor component rule) and TWA0035 ("Hosted contract has no [Operation]"). Both are TimeWarp.Architecture app rules, the same category the policy turns off. They stay at their upstream default, so the documented policy "only TWA0004 applies; the rest are off" no longer holds. The `.editorconfig` comment, the release notes, and Results all state that beta.29 "adds" the 8, which is factually wrong. They produce no warnings today, but a sample or test-app page that renders a `FluentMessageBar` could start warning under TWA0025 if it is on by default.
- Suggestion: Add `dotnet_diagnostic.TWA0025.severity = none` and `dotnet_diagnostic.TWA0035.severity = none` to the `[*.cs]` TWA block and to `msbuild/external-sources.globalconfig` (TWA0025 targets Razor components, so the globalconfig entry matters), and to the `[*.razor]` block if it stays. Correct the "adds …" list in the `.editorconfig` comment, the beta.16 release notes, and the Results Exceptions paragraph to all 10 ids.
- Status: open

### Issue 2 — Severity: nit
- File: .editorconfig:331-342 vs msbuild/external-sources.globalconfig:14-17
- Description: The two new comments contradict each other. `.editorconfig` says TWA0036 reports against the `.razor` path and the new `[*.razor]` section keeps these rules off. The globalconfig says Razor files are additional files and "the `[*.razor]` editorconfig section does not reach that compilation, so TWA0036 still warns there". If the globalconfig comment is right (Roslyn applies editorconfig severity per syntax tree, and additional-file diagnostics only see global config), the `[*.razor]` block is dead config with a misleading comment.
- Suggestion: Settle which file actually silences the Razor hits. If it is the globalconfig, drop the `[*.razor]` block (or keep it and say plainly that it has no effect on additional-file diagnostics). Otherwise reword the globalconfig comment. Either way, the two comments should agree.
- Status: open
