#region Purpose
// Inline sources and the sibling test generator for the declared-action tests (task 160).
#endregion

#region Design
// SiblingGenerator stands in for architecture's contract generator: it emits RoleState's CreateRoleActionSet and
// DeleteRoleActionSet (Action and Handler) through RegisterSourceOutput, so TimeWarpStateGenerator, running in the
// same driver, cannot see them. That is the situation a GeneratedRequestPattern declaration exists for.
// Host declares two patterns: CreateRole returns its contract's Response, DeleteRole is a void IAction. The CreateRole
// contract carries System.ComponentModel.Description and an attribute with a Permissions named argument, which is
// where the catalog entry's Description and Permissions come from.
#endregion

// ReSharper disable InconsistentNaming
namespace DeclaredAction_;

internal sealed class SiblingGenerator(string source) : IIncrementalGenerator
{
  public void Initialize(IncrementalGeneratorInitializationContext context) =>
    context.RegisterSourceOutput(context.CompilationProvider, (sourceContext, _) => sourceContext.AddSource("Sibling.g.cs", source));
}

internal static class DeclaredSources
{
  public const string Host =
    """
    namespace App
    {
      using TimeWarp.Mediator;
      using TimeWarp.State;

      [System.AttributeUsage(System.AttributeTargets.Class)]
      public sealed class PolicyAttribute : System.Attribute
      {
        public string[] Permissions { get; set; } = [];
      }

      [System.ComponentModel.Description("Create a role.")]
      [Policy(Permissions = ["roles.manage"])]
      public static class CreateRole
      {
        public sealed class Response { }
      }

      public static class DeleteRole { }

      [GeneratedRequestPattern("{Host}.{Contract}ActionSet.Action", "{Host}.{Contract}ActionSet.Handler", "TimeWarp.Mediator.IRequest<{ContractType}.Response>", typeof(CreateRole))]
      [GeneratedRequestPattern("{Host}.{Contract}ActionSet.Action", "{Host}.{Contract}ActionSet.Handler", "TimeWarp.Mediator.IAction", typeof(DeleteRole))]
      public sealed partial class RoleState : State<RoleState>
      {
        public override void Initialize() { }
      }
    }
    """;

  public const string Sibling =
    """
    namespace App
    {
      partial class RoleState
      {
        public static class CreateRoleActionSet
        {
          public sealed class Action : global::TimeWarp.Mediator.IRequest<global::App.CreateRole.Response> { }

          public sealed class Handler : global::TimeWarp.Mediator.IRequestHandler<Action, global::App.CreateRole.Response>
          {
            public global::System.Threading.Tasks.ValueTask<global::App.CreateRole.Response> Handle(Action request, global::System.Threading.CancellationToken cancellationToken) =>
              new(new global::App.CreateRole.Response());
          }
        }

        public static class DeleteRoleActionSet
        {
          public sealed class Action : global::TimeWarp.Mediator.IAction { }

          public sealed class Handler : global::TimeWarp.Mediator.IRequestHandler<Action>
          {
            public global::System.Threading.Tasks.ValueTask Handle(Action request, global::System.Threading.CancellationToken cancellationToken) => default;
          }
        }
      }
    }
    """;

  public static (GeneratorDriver Driver, Compilation Output) Run(string host, string? sibling = Sibling, string assemblyName = "Declared.Tests")
  {
    IIncrementalGenerator[] generators = sibling is null
      ? [new TimeWarpStateGenerator()]
      : [new SiblingGenerator(sibling), new TimeWarpStateGenerator()];
    (GeneratorDriver driver, Compilation output, _) = GeneratorTestHost.Run(GeneratorTestHost.CreateCompilation(host, assemblyName), generators);
    return (driver, output);
  }

  public static string? Source(GeneratorDriver driver, string hintName) =>
    GeneratorTestHost.Sources(driver)
      .Where(source => source.HintName == hintName)
      .Select(source => source.SourceText.ToString())
      .SingleOrDefault();

  public static StateManifest Manifest(Compilation output)
  {
    INamedTypeSymbol manifest = output.GetTypeByMetadataName("Declared.Tests.TimeWarpStateManifest").ShouldNotBeNull();
    return StateManifest.Parse((string)manifest.GetMembers("Json").OfType<IFieldSymbol>().Single().ConstantValue!);
  }
}
