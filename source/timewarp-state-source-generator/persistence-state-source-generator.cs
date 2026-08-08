namespace TimeWarp.State.SourceGenerator;

[Generator]
public class PersistenceStateSourceGenerator : IIncrementalGenerator
{
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    var classDeclarations = context.SyntaxProvider
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
    var classDeclaration = (ClassDeclarationSyntax)context.Node;
    
    string namespaceName = GetNamespace(classDeclaration);
    string className = classDeclaration.Identifier.Text;
    string persistentStateMethod = GetPersistentStateMethod(classDeclaration);
    
    return new ClassModel(namespaceName, className, persistentStateMethod);
  }

  private static void Execute(ClassModel model, SourceProductionContext context)
  {
    string generatedCode = GenerateLoadClassCode(
      model.NamespaceName,
      model.ClassName,
      model.PersistentStateMethod);
    
    string uniqueHintName = $"{model.NamespaceName}.{model.ClassName}_Persistence.g.cs";
    
    ReportUniqueHintNameDiagnostic(context, uniqueHintName);
    context.AddSource(uniqueHintName, SourceText.From(generatedCode, Encoding.UTF8));
  }

  private static void ReportUniqueHintNameDiagnostic(SourceProductionContext context, string uniqueHintName)
  {
    var diagnostic = Diagnostic.Create(
      new DiagnosticDescriptor(
        id: "SG001",
        title: "Unique Hint Name",
        messageFormat: "Unique hint name for generated file: {0}",
        category: "SourceGeneratorDebug",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true),
      location: Location.None,
      uniqueHintName);

    context.ReportDiagnostic(diagnostic);
  }

  private static string GenerateLoadClassCode(string namespaceName, string className, string persistentStateMethod)
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

  private static string ToCamelCase(string str)
  {
    if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
    {
      return char.ToLower(str[0]) + str.Substring(1);
    }
    return str;
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

  private static string GetPersistentStateMethod(MemberDeclarationSyntax classDeclaration)
  {
    foreach (AttributeListSyntax attributeList in classDeclaration.AttributeLists)
    {
      foreach (AttributeSyntax attribute in attributeList.Attributes)
      {
        if (!attribute.Name.ToString().EndsWith("PersistentState")) continue;
        AttributeArgumentSyntax? argument = attribute.ArgumentList?.Arguments.FirstOrDefault();
        if (argument?.Expression is not null)
        {
          string methodArgument = argument.Expression.ToString();
          string? method = methodArgument.Split('.').LastOrDefault();
          return method ?? "SessionStorage";
        }
        break;
      }
    }

    return "SessionStorage";
  }

  private sealed class ClassModel
  {
    public string NamespaceName { get; }
    public string ClassName { get; }
    public string PersistentStateMethod { get; }

    public ClassModel(string namespaceName, string className, string persistentStateMethod)
    {
      NamespaceName = namespaceName;
      ClassName = className;
      PersistentStateMethod = persistentStateMethod;
    }
  }
}
