#region Purpose
// The two contracts of the sample: add a role and remove a role.
#endregion

#region Design
// Plain records. The Description attribute becomes the action catalog entry's Description, because a declared action
// takes its catalog text from the contract (TimeWarp.State task 160).
#endregion

namespace Sample09Console.Features.Roles;

/// <summary>Adds the role <paramref name="Name"/>.</summary>
/// <param name="Name">The role name.</param>
[System.ComponentModel.Description("Add a role.")]
public sealed record AddRole(string Name);

/// <summary>Removes the role <paramref name="Name"/>.</summary>
/// <param name="Name">The role name.</param>
[System.ComponentModel.Description("Remove a role.")]
public sealed record RemoveRole(string Name);
