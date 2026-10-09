#region Purpose
// Emits AOT-safe clone methods for State<T> and [GenerateClone] types, plus a module initializer that registers states.
#endregion

#region Design
// External cloners, not partial members, so existing states stay non-partial. ICloneable is skipped and wins at
// runtime. One compilation produces one source file. TWSG002 is an error: there is no reflection fallback.
// The syntax provider only contributes equatable identity; planning runs against the compilation in Execute.
#endregion

namespace TimeWarp.State.SourceGenerator;

[Generator]
public sealed class StateCloneSourceGenerator : IIncrementalGenerator
{
  public const string DiagnosticId = "TWSG002";

  internal static readonly DiagnosticDescriptor UnsupportedRule =
    new
    (
      DiagnosticId,
      title: "State clone cannot be generated",
      messageFormat: "Cannot generate a clone for '{0}' because {1}. Implement ICloneable on '{0}', or change the unsupported member.",
      category: "Cloning",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "The state clone source generator could not emit a clone. Implement ICloneable or change the member named in the message."
    );

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    IncrementalValuesProvider<string?> typeNames = context.SyntaxProvider
      .CreateSyntaxProvider(
        predicate: static (node, _) => node is TypeDeclarationSyntax,
        transform: static (syntaxContext, _) => TypeIdentity(syntaxContext))
      .Where(static name => name is not null);

    IncrementalValueProvider<(Compilation Compilation, ImmutableArray<string?> Names)> input =
      context.CompilationProvider.Combine(typeNames.Collect());

    context.RegisterSourceOutput(
      input,
      static (sourceContext, pair) => Execute(pair.Compilation, pair.Names, sourceContext));
  }

  private static string? TypeIdentity(GeneratorSyntaxContext syntaxContext)
  {
    if (syntaxContext.Node is not TypeDeclarationSyntax typeDeclaration)
    {
      return null;
    }

    if (syntaxContext.SemanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol symbol)
    {
      return null;
    }

    return symbol.ToDisplayString();
  }

  private static void Execute(
    Compilation compilation,
    ImmutableArray<string?> typeNames,
    SourceProductionContext sourceContext)
  {
    if (typeNames.IsDefault || compilation.GetTypeByMetadataName("TimeWarp.State.State`1") is null)
    {
      return;
    }

    StateClonePlanner planner = new(compilation);
    string? source = planner.Plan(sourceContext);
    if (source is not { Length: > 0 })
    {
      return;
    }

    sourceContext.AddSource("TimeWarpStateClones.g.cs", SourceText.From(source, Encoding.UTF8));
  }
}
