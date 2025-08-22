namespace TimeWarp.State.Policies;

using Features.Persistence;

public static partial class Policies
{
  
  public static PolicyDefinition CreateStatePolicy(params Assembly[] assemblies)
  {
    HaveJsonConstructor haveJsonConstructor = new();
    HaveInjectableConstructor haveInjectableConstructor = new();
    
    return Policy.Define("TimeWarp State Policy", "See https://timewarpengineering.github.io/timewarp-architecture/")
      .For(Types.InAssemblies(assemblies))
      .Add
      (
        t => t
          .That()
          .Inherit(typeof(State<>))
          .And()
          .AreNotAbstract()
          .Should()
          .HaveNameEndingWith("State"),
        "suffix State",
        "State filenames should be suffixed with `State`."
      )
      .Add
      (
        t => t
          .That()
          .Inherit(typeof(State<>))
          .And()
          .AreNotAbstract()
          .Should()
          .BePublic(),
        "public State",
        "States should be public. This allows for them to be packaged in separate assemblies. And their methods can be called from other assemblies."
      )
      .Add
      (
        t => t
          .That()
          .Inherit(typeof(State<>))
          .And()
          .AreNotAbstract()
          .Should()
          .BeSealed(),
        "sealed State",
        "States should be sealed. If they were to be extended, they should be made abstract."
      )
      .Add
      (
        t => t
          .That()
          .Inherit(typeof(State<>))
          .And()
          .AreNotAbstract()
          .And()
          .HaveCustomAttribute(typeof(PersistentStateAttribute))
          .Should()
          .MeetCustomRule(haveJsonConstructor),
        "PersistentState requires JsonConstructor",
        "States with [PersistentState] attribute must have a constructor marked with [JsonConstructor] for proper deserialization of persisted state."
      )
      .Add
      (
        t => t
          .That()
          .Inherit(typeof(State<>))
          .And()
          .AreNotAbstract()
          .And()
          .HaveCustomAttribute(typeof(PersistentStateAttribute))
          .Should()
          .MeetCustomRule(haveInjectableConstructor),
        "PersistentState requires injectable constructor",
        "States with [PersistentState] attribute must have an injectable constructor (with non-primitive parameters) for use by the DI container."
      );
  }
}
