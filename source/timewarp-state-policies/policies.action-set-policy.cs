#region Purpose
// Architecture policy for ActionSets: internal and static.
#endregion

#region Design
// ActionSets are only containers for an Action and its Handler; callers use the generated method on the state.
#endregion

namespace TimeWarp.State.Policies;

public static partial class Policies
{
  public static PolicyDefinition CreateActionSetPolicy(params Assembly[] assemblies)
  {
    return Policy.Define("TimeWarp Action Set Policy", "See https://timewarpengineering.github.io/timewarp-architecture/")
     .For(Types.InAssemblies(assemblies))
     .Add
      (
        t => t
         .That()
         .HaveNameEndingWith("ActionSet")
         .Should()
         .BeInternal(),
        "internal ActionSet",
        "ActionSets should be internal. Their Action and Handler can still be registered with DI, but they are not intended to be called directly. The associated method will be exposed on the State."
      )
      .Add
      (
        t => t
          .That()
          .HaveNameEndingWith("ActionSet")
          .Should()
          .BeStatic(),
        "static ActionSet",
        "ActionSets should be static. They are just containers for Actions and Handlers and thus don't need to be instantiated."
      );
  }
}
