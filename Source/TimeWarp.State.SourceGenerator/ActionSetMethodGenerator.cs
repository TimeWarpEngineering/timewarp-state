namespace TimeWarp.State.SourceGenerator;

[Generator]
public class ActionSetMethodSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) =>
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public void Execute(GeneratorExecutionContext context)
    {
      if (context.SyntaxReceiver is not SyntaxReceiver receiver) return;

      foreach (ClassDeclarationSyntax classDeclaration in receiver.CandidateClasses)
      {
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
        string namespaceName = GetNamespace(classDeclaration);
        string className = classDeclaration.Identifier.Text;
        string methodName = className.Replace("ActionSet", "");
        var actionClass = classDeclaration.DescendantNodes().OfType<ClassDeclarationSyntax>()
          .FirstOrDefault(c => c.Identifier.Text == "Action");

        if (actionClass != null)
        {
          List<(string Type, string Name)> parameters = GetActionConstructorParameters(actionClass, semanticModel);
          string parentClassName = GetParentClassName(classDeclaration);
          string generatedCode = GenerateMethodCode(namespaceName, className, methodName, parameters, parentClassName);
          string uniqueHintName = $"{namespaceName}.{parentClassName}.{className}_Method.g.cs";
          ReportUniqueHintNameDiagnostic(context, uniqueHintName);
          context.AddSource(uniqueHintName, generatedCode);
        }
      }
    }

    private static void ReportUniqueHintNameDiagnostic(GeneratorExecutionContext context, string uniqueHintName)
    {
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor(
                id: "SG002",
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

    private static string GenerateMethodCode(string namespaceName, string className, string methodName, List<(string Type, string Name)> parameters, string parentClassName)
    {
        string parameterList = string.Join(", ", parameters.Select(p => $"{p.Type} {p.Name}"));
        string argumentList = string.Join(", ", parameters.Select(p => p.Name));

        return $$"""
        #nullable enable

        #pragma warning disable CS1591
        namespace {{namespaceName}};

        using System.Threading;
        using System.Threading.Tasks;

        public partial class {{parentClassName}}
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
        while (node != null &&
               node is not NamespaceDeclarationSyntax &&
               node is not FileScopedNamespaceDeclarationSyntax)
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

    private static List<(string Type, string Name)> GetActionConstructorParameters(ClassDeclarationSyntax actionClass, SemanticModel semanticModel)
    {
      var constructor = actionClass.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
      if (constructor == null)
        return new List<(string, string)>();

      return constructor.ParameterList.Parameters.Select(p =>
      {
        var parameterSymbol = semanticModel.GetDeclaredSymbol(p) as IParameterSymbol;
        string fullTypeName = parameterSymbol?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "System.Object";
        return (fullTypeName, p.Identifier.Text);
      }).ToList();
    }

    class SyntaxReceiver : ISyntaxReceiver
    {
        public HashSet<ClassDeclarationSyntax> CandidateClasses { get; } = new(new ClassDeclarationSyntaxComparer());

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not ClassDeclarationSyntax classDeclarationSyntax) return;
            if (classDeclarationSyntax.Identifier.Text.EndsWith("ActionSet") && classDeclarationSyntax.Parent is ClassDeclarationSyntax)
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

                string xNamespace = GetNamespace(x);
                string yNamespace = GetNamespace(y);
                return x.Identifier.ValueText == y.Identifier.ValueText && xNamespace == yNamespace;
            }

            public int GetHashCode(ClassDeclarationSyntax obj)
            {
                if (ReferenceEquals(obj, null)) return 0;

                int hashClassName = obj.Identifier.ValueText.GetHashCode();
                int hashNamespace = GetNamespace(obj).GetHashCode();

                return hashClassName ^ hashNamespace;
            }

            private static string GetNamespace(ClassDeclarationSyntax classDeclaration)
            {
                SyntaxNode? namespaceDeclaration = classDeclaration.Parent;
                while (namespaceDeclaration != null && namespaceDeclaration is not NamespaceDeclarationSyntax)
                {
                    namespaceDeclaration = namespaceDeclaration.Parent;
                }

                return namespaceDeclaration is NamespaceDeclarationSyntax namespaceSyntax ? namespaceSyntax.Name.ToString() : string.Empty;
            }
        }
    }
}
