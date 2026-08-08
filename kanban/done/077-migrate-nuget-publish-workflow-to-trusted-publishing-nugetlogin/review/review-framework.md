# Review framework — task 077

**Date:** 2026-08-08
**Host task:** kanban/in-progress/077-migrate-nuget-publish-workflow-to-trusted-publishing-nugetlogin/
**Diff scope:** commit `1ef1b770` — `.github/workflows/ci-cd.yml` (workflow migration); kanban checklist/session updates
**Plan / brief:** Migrate NuGet publish from long-lived `PUBLISH_TO_NUGET_ORG` secret to Trusted Publishing via `nuget/login@v1` (OIDC), mirror timewarp-nuru pattern; fix draft-release trigger; gate workflow_dispatch publish
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok orchestrate-task 077 (2026-08-08)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
