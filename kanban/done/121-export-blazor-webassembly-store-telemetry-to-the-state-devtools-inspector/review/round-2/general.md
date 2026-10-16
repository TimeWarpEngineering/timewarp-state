# Round 2 — general
**Date:** 2026-10-16
**Scope reviewed:** the fix delta `90de0a77..ea02ee4a` (commit ea02ee4a, "fix: keep browser ingest working when Aspire runs the inspector"), 19 files. This re-checks round-1 M1–M9 and looks for new defects in the delta.

## Summary

The fix pass covers all nine round-1 findings. In run mode, `AddTimeWarpStateDevTools` sets `ASPNETCORE_ENVIRONMENT=Development` on the packaged inspector, and a new test launches the stamped `tools/` dll with that value and gets a 204 ingest preflight. CORS is now `UseCors()` plus `RequireCors` on `POST /ingest/spans`. The private-network header depends on the allow-origin header. Discovery keeps a path base. Origins collect into one annotation-backed callback. The down-inspector test now observes a throwing handler and a flush task that completed successfully. The docs, release note, and slnx items are fixed. No new defects found.

## Re-verification of round 1

| ID | Verdict | Evidence |
|----|---------|----------|
| M1 | fixed | `add-timewarp-state-dev-tools.cs:203` sets the variable when `!publishMode`. `RunThePackagedInspectorAsDevelopment` covers it, and the publish test asserts the variable is absent. `AnswerTheIngestPreflightWhenLaunchedLikeTheExecutable` starts the real dll with the callback's value and gets 204 plus allow-origin. The test passed here, with no leaked inspector process afterwards. |
| M2 | fixed | `dev-tools-ingest-cors.cs` calls `app.UseCors()` with no policy. `span-ingest-endpoint.cs` calls `RequireCors(PolicyName)`. The private-network header goes only on the ingest path and only when allow-origin is present. `LeaveAllowHeadersOffANonIngestRoute` covers `GET /` and the `/_blazor/negotiate` preflight. |
| M3 | fixed | Results record a 2026-10-16 AppHost smoke: count 3→8, `live wasm` rows for `WasmCounterState`, preflight 204, and no allow-origin on `GET /`. The sample host now serves `_framework/blazor.web.js` (`UseStaticWebAssets` + `MapStaticAssets`). This reviewer did not repeat the browser run. The launched-dll test covers the M1 path headlessly. |
| M4 | fixed | `ThrowingHandler` counts calls. The test asserts calls > 0, then `FlushTask.IsCompletedSuccessfully` after Dispose. `FlushTask` is public; the Design region documents why `InternalsVisibleTo` was rejected (it collides with the internal ingest-document copies). |
| M5 | fixed | `DiscoveryUri` appends a trailing slash and joins the relative path. `DiscoverUnderANonRootBaseAddress` asserts `http://localhost:5086/app/_timewarp/state-devtools`. The root case now asserts the request URI too. |
| M6 | fixed | `CorsOriginsAnnotation` and one `WithEnvironment` callback build a single `ReferenceExpression`. `KeepCorsOriginsFromEveryInspectedApp` asserts that both resources' endpoints are present. |
| M7 | fixed | `release12.0.0-beta.15.md` names the `Microsoft.AspNetCore.App` shared framework requirement. |
| M8 | fixed | The dead statements are removed. |
| M9 | fixed | `timewarp-state.slnx` lists the Debug/Release `<Configurations>` again. |

## Issues

None.

## Verification notes

- `dotnet fixie timewarp-state-devtools-tests --no-build`: 94 passed, 16 s. The repo's `dev test` runs `dotnet fixie` (`tools/dev-cli/endpoints/test-command.cs`).
- `dotnet test tests/timewarp-state-devtools-tests/...csproj` (the VSTest adapter) hung past 200 s here with no test output. The same command on a `git archive` of the round-1 commit `90de0a77`, built in a temp directory, hung the same way, so this fix pass did not cause it. The isolation-tests project passes under `dotnet test`. Results quote `dotnet test` with 94 passing. The count matches fixie, but the hang could not be reproduced as a pass here. This is recorded as an environment note, not a finding.
- The smoke instructions dropped `./bin/dev pack`. That is correct: the sample AppHost references the devtools app as a project and imports its targets in-repo.
- Restatement against the task: still consistent. The fix-pass paragraph matches round-1 M1–M9. No misread.
