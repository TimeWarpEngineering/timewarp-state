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
            string namespaceName = GetNamespace(classDeclaration);
            string className = classDeclaration.Identifier.Text;
            string methodName = className.Replace("ActionSet", "");
            var actionClass = classDeclaration.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == "Action");

            if (actionClass != null)
            {
                var parameters = GetActionConstructorParameters(actionClass);
                string parentClassName = GetParentClassName(classDeclaration);
                string generatedCode = GenerateMethodCode(namespaceName, className, methodName, parameters, parentClassName);
                string uniqueHintName = $"{namespaceName}.{parentClassName}.{className}_Method.g.cs";
                ReportUniqueHintNameDiagnostic(context, uniqueHintName);
                context.AddSource(uniqueHintName, SourceText.From(generatedCode, Encoding.UTF8));
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
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("#pragma warning disable CS1591");
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine($"public partial class {parentClassName}");
        sb.AppendLine("{");
        sb.Append($"    public async Task {methodName}(");

        for (int i = 0; i < parameters.Count; i++)
        {
            sb.Append($"{parameters[i].Type} {parameters[i].Name}");
            if (i < parameters.Count - 1)
                sb.Append(", ");
        }

        if (parameters.Count > 0)
            sb.Append(", ");
        sb.AppendLine("CancellationToken? externalCancellationToken = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue");
        sb.AppendLine("            ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)");
        sb.AppendLine("            : null;");
        sb.AppendLine();
        sb.Append($"        await Sender.Send(new {className}.Action(");

        for (int i = 0; i < parameters.Count; i++)
        {
            sb.Append(parameters[i].Name);
            if (i < parameters.Count - 1)
                sb.Append(", ");
        }

        sb.AppendLine("),");
        sb.AppendLine("            linkedCts?.Token ?? CancellationToken");
        sb.AppendLine("        );");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("#pragma warning restore CS1591");

        return sb.ToString();
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

    private static List<(string Type, string Name)> GetActionConstructorParameters(ClassDeclarationSyntax actionClass)
    {
        var constructor = actionClass.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
        if (constructor == null)
            return new List<(string, string)>();

        return constructor.ParameterList.Parameters.Select(p => (p.Type?.ToString() ?? "object", p.Identifier.Text)).ToList();
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
