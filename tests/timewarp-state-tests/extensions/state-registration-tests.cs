#region Purpose
// AddTimeWarpState applies the generated registrars for the selected assemblies and keeps a host registration.
#endregion

#region Design
// Fixtures are public so the generator can name them, and [NotTest] so Fixie does not run their members.
// Assertions look at ServiceDescriptors. They do not resolve a state, because that needs ISender.
// The test assembly contains many other states, so the checks are presence and absence, not a descriptor count.
#endregion

namespace StateRegistrationTests;

public class Should_
{
  public static void Register_Concrete_Nested_ICloneable_And_Closed_Generic_States()
  {
    ServiceCollection services = new();
    services.AddTimeWarpState(options => options.Assemblies = [typeof(RegistrationHistState).Assembly]);

    HasTransient<RegistrationHistState>(services).ShouldBeTrue();
    HasTransient<RegistrationOuter.RegistrationNestedState>(services).ShouldBeTrue();
    HasTransient<RegistrationHandState>(services).ShouldBeTrue();
    HasTransient<RegistrationBox<int>>(services).ShouldBeTrue();
    services.Any(descriptor =>
      descriptor.ServiceType.IsGenericType
      && descriptor.ServiceType.IsGenericTypeDefinition
      && descriptor.ServiceType.GetGenericTypeDefinition() == typeof(RegistrationBox<>)).ShouldBeFalse();
    services.Any(descriptor => descriptor.ServiceType == typeof(RegistrationListState<>)).ShouldBeFalse();
  }

  public static void Skip_States_From_An_Assembly_That_Was_Not_Selected()
  {
    ServiceCollection services = new();
    services.AddTimeWarpState(options => options.Assemblies = [typeof(TimeWarpStateOptions).Assembly]);

    HasTransient<RegistrationHistState>(services).ShouldBeFalse();
    HasTransient<RegistrationHandState>(services).ShouldBeFalse();
    HasTransient<RegistrationBox<int>>(services).ShouldBeFalse();
  }

  public static void Keep_A_Host_Registration()
  {
    ServiceCollection services = new();
    services.AddTransient(_ => new RegistrationHistState());
    services.AddTimeWarpState(options => options.Assemblies = [typeof(RegistrationHistState).Assembly]);

    ServiceDescriptor descriptor = services.Single(candidate => candidate.ServiceType == typeof(RegistrationHistState));
    descriptor.ImplementationFactory.ShouldNotBeNull();
  }

  private static bool HasTransient<TService>(ServiceCollection services) =>
    services.Any(descriptor => descriptor.ServiceType == typeof(TService) && descriptor.Lifetime == ServiceLifetime.Transient);

  [NotTest]
  public abstract class RegistrationListState<TState> : State<TState>
    where TState : RegistrationListState<TState>;

  [NotTest]
  public sealed class RegistrationHistState : RegistrationListState<RegistrationHistState>
  {
    public RegistrationHistState()
    {
    }

    public int Count { get; set; }

    public override void Initialize()
    {
    }
  }

  [NotTest]
  public static class RegistrationOuter
  {
    [NotTest]
    public sealed class RegistrationNestedState : State<RegistrationNestedState>
    {
      public override void Initialize()
      {
      }
    }
  }

  [NotTest]
  public sealed class RegistrationHandState : State<RegistrationHandState>, ICloneable
  {
    public override void Initialize()
    {
    }

    public object Clone() => new RegistrationHandState();
  }

  [NotTest]
  public sealed class RegistrationBox<T> : State<RegistrationBox<T>>, ICloneable
  {
    public override void Initialize()
    {
    }

    public object Clone() => new RegistrationBox<T>();
  }
}
