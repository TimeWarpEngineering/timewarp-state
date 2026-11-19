# Review framework — task 165

**Date:** 2026-11-19
**Host task:** kanban/in-progress/165-keep-the-package-version-in-one-file/
**Diff scope:** branch `task/165-keep-the-package-version-in-one-file` vs merge-base `b1bd4c0e` (`master...HEAD`). Product files: `msbuild/repository.props`, `Directory.Packages.props`, `tools/dev-cli/endpoints/build-command.cs`, `tools/dev-cli/endpoints/workflow-command.cs`, `tools/dev-cli/endpoints/workflow/pr-pipeline.cs`, `tools/dev-cli/endpoints/workflow/release-pipeline.cs`, `tools/dev-cli/endpoints/workflow/repo-version-service.cs`. Kitchen move of `task.md` is in the same range. 316 lines changed. The branch is 1 commit behind `master` (`land kanban 165`), a zero-line change to this `task.md`.
**Plan / brief:** One package version literal, in `source/Directory.Build.props` `<Version>`. `msbuild/repository.props` derives `TimeWarpStateVersion` at evaluation time, stays importable on its own, and fails the build with no fallback when the read is empty. `assert-version-ssot` stays, reduced to "derived value is non-empty and equals `<Version>`". Version number stays `12.0.0-beta.18`.
**Effort:** 2 (general only; no specialist axes)
**Reviewer roster:** general
**Session IDs:** review grok `01a1f178-5212-73e3-ad80-9b1e586f0610` (2026-11-19). Implementer: implementer-claude, session not reported (2026-11-19).

## Budget (by-diff)

- Lines changed: 316
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
