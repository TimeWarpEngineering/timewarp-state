# PushRouteInfo can replace the current breadcrumb entry

## Description

`PushRouteInfo` updates the top entry when the URL is unchanged and truncates when an older URL is revisited. A different URL always pushes. A page that is one section with many URLs (a style-guide part) cannot keep a parent crumb and swap the current one.

TimeWarp.Architecture task 423 needs that. Steve chose replace: Brand, then Color Tokens, reads `Style Guide / Color Tokens`. The library owns the stack. The app must not poke it from outside.

## Requirements

- `PushRouteInfo(replaceCurrent: true)` drops the top entry when its URL differs from the current URL, then records the current URL and document title. The entries under that top stay. The same URL still updates the title in place. An older copy of the current URL still truncates.
- `PushRouteInfo(belowUrl:, belowTitle:)` records that route immediately under the current entry. A relative URL is resolved with `NavigationManager.ToAbsoluteUri`. If the current URL is already on top, it is lifted, the parent is ensured, and the current entry is written back with the document title. History under the parent is left in place (no truncate through unrelated pages).
- Calling `PushRouteInfo()` with no arguments keeps today's behavior.
- Tests cover replace, the parent-under-current case, and the unchanged default push.

## Checklist

- [x] Replace drops only the top entry
- [x] Below URL sits under the current entry
- [x] Default push and truncate stay as they are
- [x] Plus routing tests pass

## Session

- Created: 429324 (2026-10-27)
- Implementation: 01a17e39-f9c4-7111-9688-894e68e3ca4c (2026-10-27) for architecture task 423
- Plus routing tests: 47 passed, 1 skipped. Source-generator tests: 90 passed.
- Audit: `ganda repo audit --fix --checks memsearch-scaffold` deployed `.githooks/pre-push.cs`. kanban-landed (claim 098 has no worktree) and kebab-path-names (Blazor `*.lib.module.*` names plus one backlog folder) stay advisory. Renaming those paths would break platform file names.

## Notes

- The next unreleased package version on master is already 12.0.0-beta.16. This change ships in that version.
- Consumer: timewarp-architecture task 423.

## Archived

Archived 2026-10-28 without merging (PR #660 closed). An architecture task 423 worker opened this
task from a spec line that should not have authorized work in this repo. Architecture is moving to
a location-based breadcrumb built from its own page registry, so the replace/parent options on
PushRouteInfo are not needed.
