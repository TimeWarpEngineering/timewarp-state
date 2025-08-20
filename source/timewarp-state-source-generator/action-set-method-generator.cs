namespace TimeWarp.State.SourceGenerator;

[Generator]
public class ActionSetMethodSourceGenerator : IIncrementalGenerator
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
    if (node is not ClassDeclarationSyntax classDeclaration) return false;
    
    return classDeclaration.Identifier.Text.EndsWith("ActionSet") 
           && classDeclaration.Parent is ClassDeclarationSyntax;
  }

  private static ClassModel? GetSemanticTarget(GeneratorSyntaxContext context)
  {
    var classDeclaration = (ClassDeclarationSyntax)context.Node;
    
    string namespaceName = GetNamespace(classDeclaration);
    string className = classDeclaration.Identifier.Text;
    string methodName = className.Replace(oldValue: "ActionSet", newValue: "");
    string parentClassName = GetParentClassName(classDeclaration);
    
    ClassDeclarationSyntax? actionClass = classDeclaration.DescendantNodes()
      .OfType<ClassDeclarationSyntax>()
      .FirstOrDefault(c => c.Identifier.Text == "Action");

    if (actionClass == null) return null;

    List<(string Type, string Name, string? DefaultValue)> parameters = 
      GetActionConstructorParameters(actionClass, context.SemanticModel);

    return new ClassModel(
      namespaceName,
      className,
      methodName,
      parentClassName,
      parameters);
  }

  private static void Execute(ClassModel model, SourceProductionContext context)
  {
    string generatedCode = GenerateMethodCode(
      model.NamespaceName,
      model.ClassName,
      model.MethodName,
      model.Parameters,
      model.ParentClassName);
    
    string uniqueHintName = $"{model.NamespaceName}.{model.ParentClassName}.{model.ClassName}_Method.g.cs";
    
    ReportUniqueHintNameDiagnostic(context, uniqueHintName);
    context.AddSource(uniqueHintName, generatedCode);
  }

  private static void ReportUniqueHintNameDiagnostic(SourceProductionContext context, string uniqueHintName)
  {
    var diagnostic = Diagnostic.Create(
      new DiagnosticDescriptor(
        id: "SG002",
        title: "Unique Hint Name",
        messageFormat: "Unique hint name for generated file: {0}",
        category: "SourceGeneratorDebug",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true),
      location: Location.None,
      uniqueHintName);

    context.ReportDiagnostic(diagnostic);
  }

  private static string GenerateMethodCode(
    string namespaceName,
    string className,
    string methodName,
    List<(string Type, string Name, string? DefaultValue)> parameters,
    string parentClassName)
  {
    string parameterList = string.Join(
      separator: ", ",
      parameters.Select(p => $"{p.Type} {p.Name}{(p.DefaultValue != null ? $" = {p.DefaultValue}" : "")}"));

    string argumentList = string.Join(separator: ", ", parameters.Select(p => p.Name));

    return $$"""
      #nullable enable

      #pragma warning disable CS1591
      namespace {{namespaceName}};

      using System.Threading;
      using System.Threading.Tasks;

      partial class {{parentClassName}}
      {
          public async Task {{methodName}}({{parameterList}}{{(parameters.Any() ? ", " : "")}}CancellationToken? externalCancellationToken = null)
          {
              using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
                  ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
                  : null;
      
              await Sender.Send
              (
                  new {{className}}.Action({{argumentList}}),
                  linkedCts?.Token ?? CancellationToken
              );
          }
      }
      #pragma warning restore CS1591
      """;
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

  private static string GetParentClassName(ClassDeclarationSyntax classDeclaration)
  {
    var parentClass = classDeclaration.Parent as ClassDeclarationSyntax;
    return parentClass?.Identifier.Text ?? "UnknownParentClass";
  }

  private static List<(string Type, string Name, string? DefaultValue)> GetActionConstructorParameters(
    ClassDeclarationSyntax actionClass,
    SemanticModel semanticModel)
  {
    ConstructorDeclarationSyntax? constructor = actionClass.DescendantNodes()
      .OfType<ConstructorDeclarationSyntax>()
      .FirstOrDefault();

    if (constructor == null)
      return new List<(string, string, string?)>();

    return constructor.ParameterList.Parameters.Select(p =>
    {
      var parameterSymbol = semanticModel.GetDeclaredSymbol(p) as IParameterSymbol;

      string fullTypeName = parameterSymbol?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) 
                            ?? "System.Object";

      string? defaultValue = p.Default?.Value?.ToString();
      return (fullTypeName, p.Identifier.Text, defaultValue);
    }).ToList();
  }

  private sealed class ClassModel
  {
    public string NamespaceName { get; }
    public string ClassName { get; }
    public string MethodName { get; }
    public string ParentClassName { get; }
    public List<(string Type, string Name, string? DefaultValue)> Parameters { get; }

    public ClassModel(
      string namespaceName,
      string className,
      string methodName,
      string parentClassName,
      List<(string Type, string Name, string? DefaultValue)> parameters)
    {
      NamespaceName = namespaceName;
      ClassName = className;
      MethodName = methodName;
      ParentClassName = parentClassName;
      Parameters = parameters;
    }
  }
}
