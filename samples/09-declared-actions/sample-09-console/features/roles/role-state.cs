#region Purpose
// Role state for the declared-actions sample: the role names, changed only through the generated actions.
#endregion

#region Design
// [ContractState] names the contracts. The sibling generator emits AddRoleActionSet and RemoveRoleActionSet in this
// class, and their Handlers call OnAddRole and OnRemoveRole. TimeWarp.State emits AddRole(action) and
// RemoveRole(action) from the declaration, although it never sees the emitted Actions.
#endregion

namespace Sample09Console.Features.Roles;

/// <summary>The role names; changed only through the AddRole and RemoveRole actions.</summary>
[ContractState(typeof(AddRole), typeof(RemoveRole))]
public sealed partial class RoleState : State<RoleState>
{
  private readonly List<string> RoleNames = [];

  /// <summary>The current role names, in the order they were added.</summary>
  public IReadOnlyList<string> Roles => RoleNames;

  /// <inheritdoc />
  public override void Initialize() => RoleNames.Clear();

  private void OnAddRole(AddRole contract) => RoleNames.Add(contract.Name);

  private void OnRemoveRole(RemoveRole contract) => RoleNames.Remove(contract.Name);
}
