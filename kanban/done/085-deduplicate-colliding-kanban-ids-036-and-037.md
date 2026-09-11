# Deduplicate colliding kanban ids 036 and 037

## Description

Two ids each had two kitchens. Keep the **earliest** file as the id; give the later cancelled mediator-test kitchens new reserved ids and archive them.

| Keep | File | Why |
|------|------|-----|
| **036** | `done/036-remove-redundant-render-mode-tracking.md` | 2025-08-22, completed |
| **037** | `to-do/037-factor-out-blazor-specific-code.md` | 2025-08-22, live inbox |

| New (reserved) | Was | Why |
|----------------|-----|-----|
| **083** | `done/036-create-mediator-pipeline-tests.md` | 2025-12-04, **STATUS: CANCELLED** (superseded by 052–057) |
| **084** | `done/037-mediator-migration-test-plan.md` | 2025-12-04, **STATUS: CANCELLED** (superseded by 052–057) |

Ids **083** and **084** were allocated with `ganda kanban reserve` (not hand-numbered). They stay archived; do not claim them as new work.

## Checklist

- [x] Reserve 083, 084, 085
- [x] `git mv` cancelled 036 → `archived/083-create-mediator-pipeline-tests.md`
- [x] `git mv` cancelled 037 → `archived/084-mediator-migration-test-plan.md`
- [x] Retitle those files; 083 “before 037” → 084
- [x] Note on keeper 036 and live 037
- [x] One kitchen per id on this branch (origin-home after merge)

## Out of scope

- Product code
- Renaming 052–057 or other historical ids
- Executing 037 (factor Blazor out)

## Session

- Created: 587773 (2026-09-05)
- Cockpit: board hygiene after 080-003 column cleanup

## Notes

Board had `in-progress` empty after 080-003 → done (#586). Remaining mess was id collisions only.
