#region Purpose
// Emits a Load() partial for top-level [PersistentState] types; rejects nested states.
#endregion

#region Design
// Nested [PersistentState] is unsupported: policies nest actions in states, not states in types.
// TWSG001 + skip emit rather than a containing-type partial chain (not a product feature).
// Hint names include containing types with CLR '+' so AddSource cannot collide if emit is added later.
// Load() sends the shared LoadPersistentStateRequest; PersistentStateMethod is read at runtime.
#endregion

namespace TimeWarp.State.SourceGenerator;

[Generator]
public class PersistenceStateSourceGenerator : IIncrementalGenerator
{
  public const string NestedPersistentStateDiagnosticId = "TWSG001";

  private static readonly DiagnosticDescriptor NestedPersistentStateRule =
    new
    (
      NestedPersistentStateDiagnosticId,
      title: "[PersistentState] is not supported on nested classes",
      messageFormat: "[PersistentState] is not supported on nested class '{0}'. Move the state to a top-level type.",
      category: "Persistence",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "Policies nest actions in states, not states in other types. Nested [PersistentState] would emit a top-level partial that does not merge with the nested type."
    );

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    IncrementalValuesProvider<ClassModel?> classDeclarations = context.SyntaxProvider
      .CreateSyntaxProvider(
        predicate: static (node, _) => IsCandidateClass(node),
        transform: static (ctx, _) => GetSemanticTarget(ctx))
      .Where(static m => m is not null);

    context.RegisterSourceOutput(classDeclarations,
      static (spc, source) => Execute(source!, spc));
  }

  private static bool IsCandidateClass(SyntaxNode node)
  {
    if (node is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclaration)
      return false;

    return classDeclaration.AttributeLists
      .SelectMany(attrList => attrList.Attributes)
      .Any(attr => attr.Name.ToString() == "PersistentState" || attr.Name.ToString().EndsWith(".PersistentState"));
  }

  private static ClassModel? GetSemanticTarget(GeneratorSyntaxContext context)
  {
    ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;

    string namespaceName = GetNamespace(classDeclaration);
    string className = classDeclaration.Identifier.Text;
    bool isNested = classDeclaration.Parent is TypeDeclarationSyntax;
    string hintName = BuildHintName(classDeclaration, namespaceName);
    Location location = classDeclaration.Identifier.GetLocation();

    return new ClassModel(namespaceName, className, isNested, hintName, location);
  }

  private static void Execute(ClassModel model, SourceProductionContext context)
  {
    if (model.IsNested)
    {
      context.ReportDiagnostic(Diagnostic.Create(NestedPersistentStateRule, model.Location, model.ClassName));
      return;
    }

    string generatedCode = GenerateLoadClassCode(model.NamespaceName, model.ClassName);
    context.AddSource(model.HintName, SourceText.From(generatedCode, Encoding.UTF8));
  }

  private static string GenerateLoadClassCode(string namespaceName, string className)
  {
    return $$$"""
      #nullable enable

      #pragma warning disable CS1591
      namespace {{{namespaceName}}};

      public partial class {{{className}}}
      {
        /// <summary>
        /// (Re)loads this [PersistentState] state from its configured persistence store.
        /// </summary>
        public async Task Load(CancellationToken? externalCancellationToken = null)
        {
          using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
            : null;

          // Load is dispatched through a single hand-written Mediator request whose handler is
          // registered by Mediator's generator (a per-state generated handler would not be).
          await Sender.Send
          (
            new global::TimeWarp.State.Plus.PersistentState.LoadPersistentStateRequest(typeof({{{className}}})),
            linkedCts?.Token ?? CancellationToken
          );
        }
      }
      #pragma warning restore CS1591

      """;
  }

  private static string BuildHintName(ClassDeclarationSyntax classDeclaration, string namespaceName)
  {
    List<string> typeNames = [];
    SyntaxNode? node = classDeclaration;
    while (node is TypeDeclarationSyntax typeDeclaration)
    {
      typeNames.Insert(0, typeDeclaration.Identifier.Text);
      node = node.Parent;
    }

    string typePath = string.Join(separator: "+", typeNames);
    return $"{namespaceName}.{typePath}_Persistence.g.cs";
  }

  private static string GetNamespace(SyntaxNode? node)
  {
    while (node != null
           && node is not NamespaceDeclarationSyntax
           && node is not FileScopedNamespaceDeclarationSyntax)
    {
      node = node.Parent;
    }

    return node switch
    {
      NamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.Name.ToString(),
      FileScopedNamespaceDeclarationSyntax fileScopedNamespace => fileScopedNamespace.Name.ToString(),
      _ => "Global"
    };
  }

  private sealed class ClassModel
  {
    public string NamespaceName { get; }
    public string ClassName { get; }
    public bool IsNested { get; }
    public string HintName { get; }
    public Location Location { get; }

    public ClassModel(
      string namespaceName,
      string className,
      bool isNested,
      string hintName,
      Location location)
    {
      NamespaceName = namespaceName;
      ClassName = className;
      IsNested = isNested;
      HintName = hintName;
      Location = location;
    }
  }
}
