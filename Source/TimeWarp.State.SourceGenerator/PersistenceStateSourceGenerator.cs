namespace TimeWarp.State.SourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

[Generator]
public class PersistenceStateSourceGenerator : ISourceGenerator
{
  public void Initialize(GeneratorInitializationContext context)
  {
    // Register a syntax receiver that will be called for each syntax tree in the compilation
    context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
  }

  public void Execute(GeneratorExecutionContext context)
  {
    
    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SG001", "Debug", "{0}", "Debug", DiagnosticSeverity.Warning, true), Location.None, "****** Your debug message here ******"));

    // System.Diagnostics.Debugger.Launch();

    // Retrieve the populated receiver from the context
    if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;

    foreach (ClassDeclarationSyntax classDeclaration in receiver.CandidateClasses)
    {
      string namespaceName = (classDeclaration.Parent as NamespaceDeclarationSyntax)?.Name.ToString() ?? "DefaultNamespace";
      string className = classDeclaration.Identifier.Text;
      string generatedCode = GenerateCode(namespaceName, className);

      // string uniqueHintName = $"{className}_Persistence.g.cs";
      // string uniqueHintName = $"{namespaceName}.{className}_Persistence.g.cs";
      string uniqueHintName = $"{className}_Persistence_{Guid.NewGuid()}.g.cs";

      context.AddSource(uniqueHintName, SourceText.From(generatedCode, Encoding.UTF8));

    }
  }

  private string GenerateCode(string namespaceName, string className)
  {
    return $@"
namespace {namespaceName};

public partial class {className}
{{
    // Generated persistence handling code
}}
";
  }

  class SyntaxReceiver : ISyntaxReceiver
  {
    public List<ClassDeclarationSyntax> CandidateClasses { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
      // Look for class declarations with the [PersistentState] attribute
      if (syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclarationSyntax)
      {
        CandidateClasses.Add(classDeclarationSyntax);
      }
    }
  }
}
