#region Purpose
// Console host that dispatches actions another generator emits, through the methods, catalog and manifest
// TimeWarp.State generates from a GeneratedRequestPattern declaration.
#endregion

#region Design
// AddRole goes through the generated RoleState.AddRole(action) method, RemoveRole through the generated action catalog
// entry, so both State outputs are exercised. The manifest lists both actions with origin "declared". The process
// prints three lines and exits 0 when the roles end as [Editor] and both manifest rows are declared.
#endregion

namespace Sample09Console;

/// <summary>Entry point of the declared-actions sample.</summary>
public static class Program
{
  /// <summary>Runs the sample; returns 0 when every check passes.</summary>
  public static async Task<int> Main()
  {
    ServiceCollection services = new();
    services.AddGeneratedMediator<ClientPipeline>();
    services.AddTimeWarpState(options => options.Assemblies = [typeof(Program).Assembly]);
    services.AddActionCatalog(typeof(Program).Assembly);

    await using ServiceProvider provider = services.BuildServiceProvider();
    IStore store = provider.GetRequiredService<IStore>();
    RoleState roleState = store.GetState<RoleState>();

    await roleState.AddRole(new RoleState.AddRoleActionSet.Action(new AddRole("Admin")));
    await roleState.AddRole(new RoleState.AddRoleActionSet.Action(new AddRole("Editor")));

    ActionCatalogEntry remove = provider.GetRequiredService<IActionCatalog>().Find("Role.RemoveRole")
      ?? throw new InvalidOperationException("Role.RemoveRole is not in the action catalog.");
    await remove.Execute(store, [new RoleState.RemoveRoleActionSet.Action(new RemoveRole("Admin"))]);

    IReadOnlyList<string> roles = store.GetState<RoleState>().Roles;
    StateManifestState manifest = StateManifest.FromAssembly(typeof(Program).Assembly)!.States.Single();
    string[] declared = [.. manifest.Actions.Where(action => action.IsDeclared).Select(action => action.Method ?? action.Name)];

    Terminal.WriteLine($"Roles={string.Join(",", roles)}");
    Terminal.WriteLine($"Catalog={remove.Name}: {remove.Description}");
    Terminal.WriteLine($"Declared={string.Join(",", declared)}");
    return roles.SequenceEqual(["Editor"]) && declared.SequenceEqual(["AddRole", "RemoveRole"]) ? 0 : 1;
  }
}
