# Task 051: Migrate Mediator - Bump Version and Release

## Description

**Archived (080-003).** Do not execute this task as written.

051 would have cut a TimeWarp.State major (suggested 13.0.0) as a **martinothamar** destination release. That path is not the destination. 080 is soak of TimeWarp.Mediator **14-beta**, not a State NuGet “we switched, ship it.”

## Disposition

- Archived by 080-003 so nobody ships a martinothamar State.
- A later State NuGet release (major bump after 14-beta soak / TimeWarp.Mediator 14.0.0 stable) is a **new** task after 080, not this id.

## Out of scope (080)

- TimeWarp.State NuGet release as “done”
- TimeWarp.Mediator 14.0.0 stable

## Requirements (obsolete — do not execute)

- Increment major version number
- Update changelog/release notes
- Create NuGet packages
- Verify packages

## Notes

Do not bump `TimeWarpStateVersion` or publish from this checklist.
