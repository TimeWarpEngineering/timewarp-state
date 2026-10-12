# Package State DevTools inspector as plugin 1 for the TimeWarp DevTools shell

## Description

Package the task 103 State DevTools companion Blazor inspector as plugin 1 for the TimeWarp DevTools shell, which TimeWarpEngineering/timewarp-architecture task 335 builds. The plugin ships from this repo as today, alongside `TimeWarp.State.DevTools.Hosting` and `TimeWarp.State.DevTools.Export` (12.0.0-beta.12).

Also move out of State any shell-related pieces the Architecture shell needs, such as the generic hosting or the path-locator logic (`DevToolsProjectLocator`). The inspector is currently not packable and is found by walking folders inside this repo.

Origin: https://grok.com/share/bGVnYWN5LWNvcHk_5708bbd6-b09e-4faf-a204-1862df3f8d5c and the 2026-10-13 call.

## Checklist

- [ ] Make the inspector a packaged plugin implementing the shell's minimal contract (name, icon, tab content, optional service registration)
- [ ] Identify and move generic shell/hosting/locator pieces to the Architecture shell (task 335)
- [ ] Keep task 103's separate-store rule
- [ ] The plugin contract must not depend on State

## Sequencing

Starts after Architecture task 335 step 2: the current State DevTools setup works as-is in Architecture's AppHost.

## Session

- Created: 1230420 (2026-10-12)

## References

- Task 103 (State DevTools companion app)
- TimeWarpEngineering/timewarp-architecture task 335 (DevTools shell)

## Learnings

