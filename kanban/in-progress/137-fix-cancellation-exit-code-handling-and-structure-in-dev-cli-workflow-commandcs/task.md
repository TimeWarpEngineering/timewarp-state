# Fix cancellation, exit-code handling and structure in dev-cli workflow-command.cs
## Description

`tools/dev-cli/endpoints/workflow-command.cs` (734 LOC) ignores cancellation, has dead exit-code branches, leaks the NuGet API key to the process list, and bundles five concerns into one endpoint. Fix the bugs and split it.

## Requirements

Bugs:
- All git and gh calls pass `CancellationToken.None` (lines ~449, 464, 474, 496, 505, 518, 538, 553, 583). The handler stores `Ct` in a field that static helpers cannot see. Pass the token through.
- Lines ~615-623 and ~680-688 call `RunAsync()` without `WithNoValidation`; Amuru throws on non-zero exit so the `exitCode != 0` throws and the "Failed to push X!" messages never run.
- `RunStepAsync` (~358-369) depends on `Environment.ExitCode` set elsewhere and reset at ~82 and ~123. A step that throws bypasses "Pipeline FAILED". Return a result instead of using global state.
- Unguarded `XDocument` loads at ~418 and ~434; malformed props throw raw `XmlException`.
- `propsVersion!` at ~325 relies on an earlier step having failed.
- NuGet API key passed on the command line (~677) is visible in the process list. Use env or stdin if the push tool supports it, otherwise document the exposure.
- `Handler.ApiKey` field duplicates the command property.

Split (leaves ~130 lines in the endpoint):
- `ReleasePipeline` (~120-356)
- `ReleaseGitGate` (~442-531 plus `TagPin`, `Ancestor` types ~694-712)
- `CiArtifactService` (~533-629 plus `Locate`/`Download` outcome types ~714-732)
- `NuGetPushService` (~631-692)
- `RepoVersionService` (~380-437); `ReadPropsVersion` duplicates `TryReadMsBuildProperty(..., "Version")`.

Dedupe the 79-char banner/step header repeated 7 times and the three identical `default:` abort cases.

## Checklist

- [ ] CancellationToken threaded to every process call
- [ ] Exit-code handling consistent with Amuru throw semantics
- [ ] Pipeline result returned, not Environment.ExitCode
- [ ] XML loads guarded with a clear error
- [ ] API key not on command line, or exposure documented
- [ ] Split into the five services above
- [ ] `dev workflow` still runs end to end

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC.

## Session

- Created: claude code review session (2026-11-04)
