namespace TimeWarp.State.Policies;

public static partial class Policies
{
  public static PolicyDefinition CreateActionHandlerPolicy(params Assembly[] assemblies)
  {
    BeNestedInStateCustomRule beNestedInState = new();
    return Policy.Define("TimeWarp Action Handler Policy", "See https://timewarpengineering.github.io/timewarp-architecture/")
      .For(Types.InAssemblies(assemblies))
      .Add
      (
        t => t
          .That()
          .Inherit(typeof(ActionHandler<>))
          .And()
          .AreNotAbstract()
          .Should()
          .MeetCustomRule(beNestedInState),
        "Nest Handlers",
        "Action Handlers must be nested in the State they act upon."
      )
      .Add
      (
        t => t
          .That()
          .Inherit(typeof(ActionHandler<>))
          .And()
          .AreNotAbstract()
          .Should()
          .BeSealed()
          .And()
          .BeInternal(),
        "internal sealed Handler",
        "Handler should be `internal sealed`."
      );
  }
}
