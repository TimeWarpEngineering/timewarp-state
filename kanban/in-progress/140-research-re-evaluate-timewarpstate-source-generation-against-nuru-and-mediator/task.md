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

## Goal (implementer restatement)

Find out whether TimeWarp.State should change how it does source generation, by comparing its four
generators with the larger pipelines in TimeWarp.Nuru and TimeWarp.Mediator. Deliver a proposal
Steve can choose from. Change no product code.

Acceptance criteria:

- An inventory of State's generators: what each one finds, how it finds it, what it emits, and what
  it cannot see, with file and line citations.
- The same inventory for Nuru and Mediator, citing their code and design notes (documentation/,
  .agent/workspace/, done kanban tasks).
- A comparison covering discovery, pipeline shape, caching and equatable models, diagnostics and
  analyzers, testing, AOT and trimming, and maintenance cost.
- Options for State, each with its trade-offs. Any option that changes what a consumer writes or gets
  shows the code the developer writes and the code that gets generated.
- What each option costs and what it unblocks downstream (the architecture template and task 464,
  DevTools, apps).
- Stop at the proposal: no implementation tasks are split off, and Steve's decision stays open.

## Checklist

- [x] State generator inventory (report/analysis.md §4.1, spike in report/spike/)
- [x] Nuru and Mediator generator inventory (§4.2, §4.3)
- [x] Comparison (§4.4, §4.5)
- [x] Options as code, with costs and what each unblocks (§5, Options 0-5)
- [ ] Steve's decision recorded (pending: Steve decides; the proposal stops here)

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06, from the timewarp-architecture cockpit)
- 2026-11-06 implement oracle (ganda task work, claude):
  - Read every State generator file.
  - Helper agents inventoried Nuru, Mediator and architecture task 464. Their citations were spot-checked.
  - Ran a CSharpGeneratorDriver spike against the ActionSet method generator.
  - Wrote report/analysis.md. No product code changed.

## Results

The report is `report/analysis.md`. Its options are in §5.

Findings:

- **ActionSet method generator:**
  - It is not cached: an unrelated edit gives output reason Modified.
  - It ignores positional records.
  - A primary-constructor action produces CS7036 in generated code.
  - Init-only properties are dropped.
  - A nested class named `*ActionSet` outside a state produces broken partials.
  - It has no direct tests.
  - The spike is `report/spike/`.
- **Clone generator:** it reruns on every compilation, by design. That is the shape Mediator removed in 15.0.
- **The real limit is that generators cannot see each other's output** (State and Mediator in task 075, State and System.Text.Json, the generators named in 464 §4a). Only a Mediator change (Option 3b) lifts it for handlers and actions.
- **Options:**
  - 0: hygiene.
  - 1: one State graph plus a manifest for DevTools.
  - 2: an analyzer plus a code fix for the JSON context.
  - 3a: a partial shell with a generated body.
  - 3b: Mediator declared handlers.
  - 4: contract-bound action sets.
  - 5: reducer authoring.
- No decision has been made. No implementation tasks were split off.

### How to validate

Smoke:

```bash
dotnet build source/timewarp-state-source-generator/timewarp-state-source-generator.csproj -c Debug
mkdir -p /tmp/spike-140 && cp kanban/in-progress/140-research-re-evaluate-timewarpstate-source-generation-against-nuru-and-mediator/report/spike/action-set-generator-spike.cs /tmp/spike-140/
(cd /tmp && dotnet run /tmp/spike-140/action-set-generator-spike.cs -- "$OLDPWD/artifacts/analyzers/Debug/netstandard2.0/timewarp-state-source-generator.dll")
```

Expect: output matching `report/spike/action-set-generator-spike.output.txt`:

- `ctor class` generates `Add(int amount = 1, …)` and reports `Modified` after the unrelated edit.
- `positional record` generates 0 files.
- `primary ctor class` reports CS7036.
- `name match in non-state class` reports CS0260, CS0708 and CS0119.

The spike must run outside the repository: inside it, Central Package Management rejects the runfile's `#:package` version (NU1008).

## Notes

Steve 2026-11-06: "Action set was a source generator that I wrote by hand, which were really hard
to do back in the day, but now we can write them easily. Maybe we should just reevaluate the
whole way we do source gen on TimeWarp.State, and that will afford us even more capability on
the front end." Filed without a preferred answer on purpose.
