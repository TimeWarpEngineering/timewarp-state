namespace TimeWarp.State.Policies;

public static partial class Policies
{
  /// <summary>
  /// Creates the TimeWarp Action Handler Policy, requiring handlers to be public sealed.
  /// </summary>
  public static PolicyDefinition CreateActionHandlerPolicy(params Assembly[] assemblies) =>
    CreateActionHandlerPolicy(requirePublicHandlers: true, assemblies);

  /// <summary>
  /// Creates the TimeWarp Action Handler Policy.
  /// </summary>
  /// <param name="requirePublicHandlers">
  /// When true, Handlers must be `public sealed` (for libraries whose generated mediator code in the
  /// consuming host references them by concrete type, e.g. TimeWarp.State.Plus). When false, Handlers
  /// need only be `sealed`; they may stay internal because app handlers resolve inside the host assembly.
  /// </param>
  /// <param name="assemblies">The assemblies to evaluate the policy against.</param>
  public static PolicyDefinition CreateActionHandlerPolicy(bool requirePublicHandlers, params Assembly[] assemblies)
  {
    BeNestedInStateCustomRule beNestedInState = new();
    PolicyDefinition policy = Policy.Define("TimeWarp Action Handler Policy", "See https://timewarpengineering.github.io/timewarp-architecture/")
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
      );

    if (requirePublicHandlers)
    {
      policy = policy.Add
      (
        t => t
          .That()
          .Inherit(typeof(TimeWarp.Mediator.ActionHandler<>))
          .And()
          .AreNotAbstract()
          .Should()
          .BeSealed().And().BePublic(),
        "public sealed Handler",
        "Handlers shipped in a referenced library (e.g. TimeWarp.State.Plus) must be `public sealed`: the consuming host's TimeWarp.Mediator generated code references them by concrete type."
      );
    }
    else
    {
      policy = policy.Add
      (
        t => t
          .That()
          .Inherit(typeof(TimeWarp.Mediator.ActionHandler<>))
          .And()
          .AreNotAbstract()
          .Should()
          .BeSealed(),
        "sealed Handler",
        "Handler should be `sealed`. App handlers resolve inside the host assembly and may stay internal."
      );
    }

    return policy;
  }
}
