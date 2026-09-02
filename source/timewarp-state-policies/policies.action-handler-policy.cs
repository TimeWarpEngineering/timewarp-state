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
          .Inherit(typeof(TimeWarp.Mediator.ActionHandler<>))
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
          .Inherit(typeof(TimeWarp.Mediator.ActionHandler<>))
          .And()
          .AreNotAbstract()
          .Should()
          .BeSealed(),
        "sealed Handler",
        "Handler should be `sealed`. The TimeWarp.Mediator generated mediator resolves handlers by concrete type inside the host assembly, so app handlers may stay internal; handlers shipped in a referenced library (e.g. TimeWarp.State.Plus) must be public so the host's generated code can reference them."
      );
  }
}
