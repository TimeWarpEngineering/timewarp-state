# Option 1: one State graph, several emitters, and a State manifest

## Description

Source: task 140 `report/analysis.md` (options as code, costs, what each unblocks). Read it first. Option 1. Follows Option 0.

Replace the per-generator scans with one `TimeWarpStateGenerator` in the Mediator style: one equatable graph of states, actions, handlers, catalog entries, persistence and clone facts, built once; emitters for the dispatch methods, catalog, persistence, clone, and a State manifest DevTools and analyzers can read.

## Requirements

- One graph, one builder. Analyzers stay a separate project but read the same fact extractors linked as source. Do not copy Mediator's two-builder split.
- Emitters on the graph for everything the four generators emit today; output identical for existing shapes (golden tests).
- State manifest: states and actions known up front for DevTools, dispatch from DevTools, and for consumer analyzers (architecture's coverage checks).
- Incrementality test on the graph; caching pinned.
- Version bump per the repo's release rule.

## Checklist

- [ ] Graph and builder
- [ ] Emitters with golden parity
- [ ] Manifest and DevTools consumption
- [ ] Incrementality test; build 0/0

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)

## Depends on

- Option 0 task (filed with this one)

## Notes

Steve 2026-11-06 decision; order 0 → 1.
