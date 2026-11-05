# Research: re-evaluate TimeWarp.State source generation against Nuru and Mediator

## Description

Research only. No product code in this task.

TimeWarp.State's generators (`source/timewarp-state-source-generator/`: the ActionSet method
generator, the action catalog generator, the persistence generator, and the state-clone planner)
were written by hand when incremental generators were hard to write. Two sibling repositories have
since built much larger generator pipelines: TimeWarp.Nuru (`timewarp-nuru/source/timewarp-nuru-analyzers/generators/`,
locators, a DSL interpreter, emitters, interceptors) and TimeWarp.Mediator
(`timewarp-mediator/source/timewarp-mediator-generators/`, a MessageGraph built once and read by
several emitters, discovery across referenced assemblies). Both are local worktrees under
`/home/steve/worktrees/github.com/TimeWarpEngineering/`.

Look at how those two do it, look at how State does it, and report what State could do
differently or better. Treat the whole approach as open: structure, discovery, what is generated,
what consumers (apps built on State, the TimeWarp.Architecture template, DevTools) could get that
they cannot get today. Do not assume the current generator set or its boundaries are the answer.

## Requirements

- Inventory State's generators: what each discovers, how (syntax, attribute, semantic model,
  metadata), what it emits, and what it cannot see. Cite files and lines.
- Inventory the same for Nuru and Mediator, with the design notes in each repo
  (`documentation/`, `.agent/workspace/`, kanban done tasks) as sources. Cite them.
- Compare: discovery model, pipeline shape, incremental caching and equatable models, diagnostics
  and analyzer pairing, testing approach, AOT and trimming posture, maintenance cost.
- Propose what State should change, as options with trade-offs. Where an option changes what a
  consumer can do, show that with code: what the developer writes, what is generated. Prose
  alone is not enough for Steve to compare.
- Say what each option costs and what it unblocks, downstream included. Architecture task 464
  (`timewarp-architecture/kanban/in-progress/464-*/proposal.md` and `question.md`) is one
  consumer waiting on this; read it for context on what the front end wants, but it is one input,
  not the brief.
- Stop at the proposal. Steve decides. Do not split implementation tasks.

## Checklist

- [ ] State generator inventory
- [ ] Nuru and Mediator generator inventory
- [ ] Comparison
- [ ] Options as code, with costs and what each unblocks
- [ ] Steve's decision recorded

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06, from the timewarp-architecture cockpit)

## Notes

Steve 2026-11-06: "Action set was a source generator that I wrote by hand, which were really hard
to do back in the day, but now we can write them easily. Maybe we should just reevaluate the
whole way we do source gen on TimeWarp.State, and that will afford us even more capability on
the front end." Filed without a preferred answer on purpose.
