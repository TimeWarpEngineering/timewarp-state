# Round 1 — merged findings
**Date:** 2026-10-31
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: open
- File: .editorconfig:295 (also `.editorconfig:322-329`, `msbuild/external-sources.globalconfig:18-25`, `documentation/release-notes/release12.0.0-beta.16.md:39`, task.md Results)
- Description: TimeWarp.Architecture.Analyzers 2.0.0-beta.29 adds 10 ids over beta.19 (TWA0025, 0026, 0027, 0028, 0032, 0033, 0034, 0035, 0036, 0037). The change turns off 8 and leaves out TWA0025 (Razor message bars outside the shell's MessageBars host) and TWA0035 (hosted contract without [Operation]). That breaks the stated "only TWA0004 applies; the rest are off" policy, and the "adds …" list in the config comment, release notes, and Results is wrong.
- Suggestion: Set TWA0025 and TWA0035 to `none` wherever the other 8 are set (`[*.cs]` block, `external-sources.globalconfig`, and `[*.razor]` if kept). Correct the list to all 10 ids in the `.editorconfig` comment, the beta.16 release notes, and Results Exceptions. Re-run `./bin/dev build` to confirm there is no new TWA output.
- Source: general
- Disposition notes:

### M2 — Severity: nit — Status: open
- File: .editorconfig:331-342 vs msbuild/external-sources.globalconfig:14-17
- Description: The `.editorconfig` comment says the new `[*.razor]` section keeps the rules off for Razor. The globalconfig comment says that section "does not reach that compilation". One of them is wrong. If the globalconfig is right, the `[*.razor]` block is dead config.
- Suggestion: Settle which file silences the Razor hits (for example, build with only one of the two in place). Remove the dead block or reword the comment so both files agree.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None; single reviewer.

## Verified without findings

- Every pin in the Results table matches `Directory.Packages.props`; no stale old-version literals remain outside the intentional non-v4 Playwright comment.
- Aspire.AppHost.Sdk 13.6.4 is applied in both sample 04 and the packaged DevTools consumer test, matching the central Aspire pins.
- `tools/dev-cli`: there are no bare `Value` returns or `Unit.Task` uses left. `./bin/dev check-version` runs: source 12.0.0-beta.16, NuGet 12.0.0-beta.15, safe to release.
- Version handling follows the task: beta.16 is unreleased, so the dependency list was added to the existing beta.16 release notes, which `toc.yml` already lists.
- Turning the new TWA rules off follows the task 099-001 policy (every non-TWA0004 id is `none`), so it is not gate-silencing.
- Goal restatement matches the task; no misread.
