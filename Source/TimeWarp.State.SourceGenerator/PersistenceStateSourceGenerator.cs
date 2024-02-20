namespace TimeWarp.State.SourceGenerator;

[Generator]
public class PersistenceStateSourceGenerator : ISourceGenerator
{
  public void Initialize(GeneratorInitializationContext context) =>
    // Register a syntax receiver that will be called for each syntax tree in the compilation
    context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

  public void Execute(GeneratorExecutionContext context)
  {

    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SG001", "Debug", "{0}", "Debug", DiagnosticSeverity.Warning, true), Location.None, "****** Your debug message here ******"));

    // System.Diagnostics.Debugger.Launch();

    // Retrieve the populated receiver from the context
    if (context.SyntaxReceiver is not SyntaxReceiver receiver) return;

    foreach (ClassDeclarationSyntax classDeclaration in receiver.CandidateClasses)
    {
      string namespaceName = GetNamespace(classDeclaration);
      string className = classDeclaration.Identifier.Text;
      string generatedCode = GenerateCode(namespaceName, className);
      string uniqueHintName = $"{namespaceName}.{className}_Persistence.g.cs";
      ReportUniqueHintNameDiagnostic(context, uniqueHintName);
      context.AddSource(uniqueHintName, SourceText.From(generatedCode, Encoding.UTF8));
    }
  }
  private static void ReportUniqueHintNameDiagnostic(GeneratorExecutionContext context, string uniqueHintName)
  {
    var diagnostic = Diagnostic.Create(
    new DiagnosticDescriptor(
    id: "SG001",
    title: "Unique Hint Name",
    messageFormat: "Unique hint name for generated file: {0}",
    category: "SourceGeneratorDebug",
    defaultSeverity: DiagnosticSeverity.Warning,
    isEnabledByDefault: true
    ),
    location: Location.None,
    uniqueHintName
    );

    context.ReportDiagnostic(diagnostic);
  }
  
  private static string GenerateCode(string namespaceName, string className) =>
    $$"""
    #pragma warning disable CS1591
    namespace {{namespaceName}};

    public partial class {{className}}
    {
        // Generated persistence handling code
    }

    """;

  private static string GetNamespace(SyntaxNode? node)
  {
    // Traverse up to find the NamespaceDeclarationSyntax, if any
    while 
    (
      node != null && 
      node is not NamespaceDeclarationSyntax &&
      node is not FileScopedNamespaceDeclarationSyntax
    )
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

  class SyntaxReceiver : ISyntaxReceiver
  {
    public HashSet<ClassDeclarationSyntax> CandidateClasses { get; } = new(new ClassDeclarationSyntaxComparer());

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
      // Look for class declarations with the [PersistentState] attribute
      if (syntaxNode is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclarationSyntax) return;
      bool hasPersistentStateAttribute = classDeclarationSyntax.AttributeLists
        .SelectMany(attrList => attrList.Attributes)
        .Any(attr => attr.Name.ToString() == "PersistentState" || attr.Name.ToString().EndsWith(".PersistentState"));

      if (hasPersistentStateAttribute)
      {
        CandidateClasses.Add(classDeclarationSyntax);
      }
    }

    class ClassDeclarationSyntaxComparer : IEqualityComparer<ClassDeclarationSyntax>
    {
      public bool Equals(ClassDeclarationSyntax? x, ClassDeclarationSyntax? y)
      {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;

        // Consider classes equal if they have the same name and namespace
        string xNamespace = GetNamespace(x);
        string yNamespace = GetNamespace(y);
        return x.Identifier.ValueText == y.Identifier.ValueText && xNamespace == yNamespace;
      }

      public int GetHashCode(ClassDeclarationSyntax obj)
      {
        if (ReferenceEquals(obj, null)) return 0;

        // Use the hash code of the class name and namespace
        int hashClassName = obj.Identifier.ValueText.GetHashCode();
        int hashNamespace = GetNamespace(obj).GetHashCode();

        // Calculate a combined hash code
        return hashClassName ^ hashNamespace;
      }

      private static string GetNamespace(ClassDeclarationSyntax classDeclaration)
      {
        // Walk the syntax tree to find the namespace declaration
        SyntaxNode? namespaceDeclaration = classDeclaration.Parent;
        while (namespaceDeclaration != null && !(namespaceDeclaration is NamespaceDeclarationSyntax))
        {
          namespaceDeclaration = namespaceDeclaration.Parent;
        }

        return namespaceDeclaration is NamespaceDeclarationSyntax namespaceSyntax ? namespaceSyntax.Name.ToString() : string.Empty;
      }
    }
  }
}
