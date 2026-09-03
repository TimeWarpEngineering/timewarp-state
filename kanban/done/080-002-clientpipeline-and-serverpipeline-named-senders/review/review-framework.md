# Review framework — task 080-002

**Date:** 2026-09-04
**Host task:** kanban/in-progress/080-002-clientpipeline-and-serverpipeline-named-senders/
**Diff scope:** branch `task/080-002-clientpipeline-and-serverpipeline-named-senders` vs `origin/feature/080-timewarp-mediator-14-beta` (commit f58795e0), excluding `kanban/`
**Plan / brief:** Split the Blazor client store pipeline from server handlers using marker types (`ClientPipeline` / `ServerPipeline`), not strings. `ISender<ClientPipeline>` (and `IPublisher<ClientPipeline>` if used) in WASM/Blazor client code; `ISender<ServerPipeline>` on the server / API handlers; `[MediatorScope(typeof(ClientPipeline))]` / `ServerPipeline` on corresponding handlers and behaviors; state pipeline behaviors are client-scope; no `if (request is IAction)` filtering in a shared pipeline; re-entrant `Send` stays on the same scope. See task.md Requirements and Results.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle — Grok 4.6 (ganda task work, 2026-09-04)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
