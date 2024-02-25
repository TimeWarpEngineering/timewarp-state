namespace TimeWarp.State.SourceGenerator;

[Generator]
public class PersistenceStateSourceGenerator : ISourceGenerator
{
  public void Initialize(GeneratorInitializationContext context) =>
    // Register a syntax receiver that will be called for each syntax tree in the compilation
    context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

  public void Execute(GeneratorExecutionContext context)
  {
    // Retrieve the populated receiver from the context
    if (context.SyntaxReceiver is not SyntaxReceiver receiver) return;

    foreach (ClassDeclarationSyntax classDeclaration in receiver.CandidateClasses)
    {
      string namespaceName = GetNamespace(classDeclaration);
      string className = classDeclaration.Identifier.Text;
      string method = GetPersistentStateMethod(classDeclaration);
      string generatedCode = GenerateLoadClassCode(namespaceName, className, method);
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
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true
    ),
    location: Location.None,
    uniqueHintName
    );

    context.ReportDiagnostic(diagnostic);
  }

  private static string GenerateLoadClassCode(string namespaceName, string className, string persistentStateMethod)
  {
    string camelCaseClassName = ToCamelCase(className);

    return $$"""
      #nullable enable

      #pragma warning disable CS1591
      namespace {{namespaceName}};
      
      using JetBrains.Annotations;

      public partial class {{className}}
      {
          [UsedImplicitly]
          public static class Load
          {
              [UsedImplicitly]
              public class Action : IAction { }
      
              [UsedImplicitly]
              public class Handler
              (
                IStore store,
                IPersistenceService PersistenceService
              ): ActionHandler<Action>(store)
              {
                  public override async System.Threading.Tasks.Task Handle(Action aAction, System.Threading.CancellationToken aCancellationToken)
                  {
                      try
                      {
                          object? state = await PersistenceService.LoadState(typeof({{className}}), PersistentStateMethod.{{persistentStateMethod}});
                          if (state is {{className}} {{camelCaseClassName}}) Store.SetState({{camelCaseClassName}});
                      }
                      catch (Exception)
                      {
                          // if this is a JavaScript not available exception then we are prerendering and just swallow it
                      }
                  }
              }
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
          // Directly use the string representation of the argument
          string methodArgument = argument.Expression.ToString();
          // Assuming the argument is an enum member access, extract the member name
          string? method = methodArgument.Split('.').LastOrDefault();
          return method ?? "SessionStorage";// Default to "SessionStorage" if not specified
        }
        break;// Break after finding the PersistentState attribute, assuming one attribute per class
      }
    }

    // Default to "SessionStorage" if the attribute is not found
    return "SessionStorage";
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
