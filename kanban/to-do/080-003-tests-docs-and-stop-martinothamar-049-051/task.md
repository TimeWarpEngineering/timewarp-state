# Tests docs and stop martinothamar 049-051

## Description

Parent: **080**. Prove the 14-beta path in test-app / integration / e2e as this repo actually runs them. Docs say TimeWarp.Mediator 14-beta, not martinothamar and not 13.0.0 reflection.

**049, 050, 051** (run tests / docs / release as martinothamar destination) are **not** the goal. Retarget or archive them so nobody ships a martinothamar State.

## Depends on

- 080-002

## Requirements

- Existing test suites green on generated + scoped senders (or a listed, justified skip)
- README / claude.md / package tags no longer advertise martinothamar as current
- 049–051: archive or rewrite to “soak 14-beta / later State release” — do not execute them as written
- Record gaps found in TimeWarp.Mediator (file issues or mediator follow-ups). This soak is why we are on **beta**.

## Checklist

- [ ] Tests
- [ ] Docs
- [ ] 049–051 disposition
- [ ] Feedback list for mediator (if any)

## Out of scope

- TimeWarp.State NuGet release as “done”
- TimeWarp.Mediator 14.0.0 stable

## Session

- Created: 154892 (2026-09-01)
