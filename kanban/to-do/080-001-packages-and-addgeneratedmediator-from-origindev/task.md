# Packages and AddGeneratedMediator from origin/dev

## Description

Parent: **080**. Swap martinothamar packages for **TimeWarp.Mediator 14.0.0-beta.1** and register the **generated** mediator. Do this on the **origin/dev** line, not origin-home `master`.

Wait for mediator **005-003** (package on nuget.org) unless the operator waives to a 14.0.0-beta.1 project reference after **005-001**.

## Requirements

- Task branch includes `origin/dev` (net10 + current martinothamar State). Do not implement on `origin/master`.
- Remove `Mediator.Abstractions` / `Mediator.SourceGenerator`
- Add TimeWarp.Mediator 14-beta packages (Contracts + Generators; Analyzers as needed)
- `AddGeneratedMediator()` (unscoped) for this slice; scopes are **080-002**
- `[assembly: MediatorAssembly]` (or equivalent membership) so handlers are linked
- Pipeline behaviors that today register as open generics must be `[assembly: MediatorBehavior]` (or the generated equivalent)
- `AddMediator()` (legacy reflection) must **not** be the State path

## Checklist

- [ ] Branch from origin/dev
- [ ] Package refs
- [ ] Generated registration + membership
- [ ] Solution builds (handlers discovered)

## Out of scope

- Client vs server `ISender<TScope>` (080-002)
- E2E soak (080-003)

## Session

- Created: 154892 (2026-09-01)
