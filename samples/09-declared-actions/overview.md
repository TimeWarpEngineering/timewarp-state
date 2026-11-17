# Declared actions sample

Actions that another source generator emits, which TimeWarp.State serves from a declaration.

Roslyn generators cannot see each other's output. `sample-09-generator` emits `AddRoleActionSet` and
`RemoveRoleActionSet` (an `Action` and a `Handler` each) inside `RoleState`, so TimeWarp.State's and
TimeWarp.Mediator's generators never see those types. `RoleState` carries `[ContractState(typeof(AddRole), typeof(RemoveRole))]`,
which derives from TimeWarp.Mediator's `GeneratedRequestPatternAttribute`. From that declaration:

- TimeWarp.Mediator emits a `Send` arm for each Action.
- TimeWarp.State emits `RoleState.AddRole(action)` and `RoleState.RemoveRole(action)`, an action catalog entry for each
  (the Description comes from the contract's `[Description]`), and manifest rows with origin `declared`.
- TWS0015 reports a declared action the sibling generator did not emit.

```bash
dotnet run --project samples/09-declared-actions/sample-09-console/sample-09-console.csproj
```

The program prints `Roles=Editor`, `Catalog=Role.RemoveRole: Remove a role.`, and `Declared=AddRole,RemoveRole`, then
exits 0. It project-references the source projects, so it builds without `dev pack`.
