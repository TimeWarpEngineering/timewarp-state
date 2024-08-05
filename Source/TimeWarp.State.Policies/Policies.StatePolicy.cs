namespace TimeWarp.State.Policies;

public static partial class Policies
{
  public static PolicyDefinition CreateStatePolicy(params Assembly[] assemblies)
  {
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
      );
  }
}
