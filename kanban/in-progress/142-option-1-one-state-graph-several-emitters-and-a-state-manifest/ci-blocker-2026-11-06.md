# CI blocker (2026-11-06 Asia/Bangkok)

PR #672 CI red on run 42119659172 (head 5cd4ebc).

Cause: base `feature/state-graph` added `source/timewarp-state-analyzer/state-json-serializable-analyzer.cs` after this branch forked. That file still calls `StateSymbolHelpers.GetTimeWarpStateType` / `InheritsFromTimeWarpState` (lines 67 and 104). This branch deleted `StateSymbolHelpers` and moved the API to `StateSymbolFacts` (linked from `facts/`). GitHub's merge commit therefore fails with CS0103.

Fix (do not delete the analyzer): merge or rebase `origin/feature/state-graph` into the task branch, then change those two call sites to `StateSymbolFacts` (same method names as the other analyzers). Rebuild the analyzer project; push; leave the PR open (--no-merge). Proof: green CI on #672.
