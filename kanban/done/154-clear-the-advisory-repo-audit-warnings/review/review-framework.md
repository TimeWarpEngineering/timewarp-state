# Review framework

## Budget (by-diff)

- Lines changed: 227
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Round 2 scope

- **Date:** 2026-11-11
- **Diff scope:** delta since round 1, `b7c9f360..HEAD` (`34ad43c0` fix, `bb9fa831` docs). Product files:
  `.github/workflows/workflow.yml`, `scripts/e2e.cs`, `scripts/e2e-tests.cs`.
- **Brief:** fix pass for round-1 M1 and M2. The e2e `dotnet test` collects Cobertura into `artifacts/coverage`, and the upload fails when no file is written.
- **Effort:** 2, roster general (review oracle, claude-opus-5-5)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
