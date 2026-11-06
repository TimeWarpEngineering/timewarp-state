# CI blocker (2026-11-06 Asia/Bangkok)

PR #672 CI was red on run 42119659172 (head 5cd4ebc).

Cause: base `feature/state-graph` added `source/timewarp-state-analyzer/state-json-serializable-analyzer.cs` after this branch forked. That file called `StateSymbolHelpers.GetTimeWarpStateType` / `InheritsFromTimeWarpState`. This branch deleted `StateSymbolHelpers` and moved the API to `StateSymbolFacts` (linked from `facts/`). GitHub's merge commit therefore failed with CS0103.

Resolved on this branch: merged `origin/feature/state-graph`, then changed those two call sites to `StateSymbolFacts`. The analyzer stays. Local `./bin/dev build` and `./bin/dev test` are green. The PR stays open. Proof still wanted from GitHub: a green CI run on #672 after the push.
