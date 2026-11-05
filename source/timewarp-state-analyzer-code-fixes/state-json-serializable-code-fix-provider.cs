#region Purpose
// Code fix for TWS0013: writes the missing [JsonSerializable(typeof(State))] line into user source, or creates a
// JsonSerializerContext when the project has none.
#endregion

#region Design
// User source is the only place System.Text.Json's generator can see, so the fix edits it rather than generating.
// One action per context declared in the project (sorted by name). The attribute goes after the last attribute list
// on the context's first non-generated declaration; trivia is copied from that declaration so the line matches the
// file's indentation and line endings. The typeof argument is written fully qualified with Simplifier.Annotation, so
// the code action's post-processing shortens it to what the file's usings allow.
// With no context, the fix adds state-json-context.cs (kebab-case, TimeWarp convention) beside the state's file,
// declaring internal sealed partial StateJsonContext in the state's namespace. Wiring it as TypeInfoResolver stays the
// developer's one line in AddTimeWarpState; the generated summary says so.
#endregion

namespace TimeWarp.State.Analyzer.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StateJsonSerializableCodeFixProvider))]
[Shared]
public sealed class StateJsonSerializableCodeFixProvider : CodeFixProvider
{
  public const string NewContextName = "StateJsonContext";
  public const string NewContextFileName = "state-json-context.cs";

  private const string JsonSerializableAttributeName = "global::System.Text.Json.Serialization.JsonSerializable";

  public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(StateJsonSerializableAnalyzer.DiagnosticId);

  // Batch fixing would race one edit per state against the same context declaration.
  public override FixAllProvider? GetFixAllProvider() => null;

  public override async Task RegisterCodeFixesAsync(CodeFixContext context)
  {
    Document document = context.Document;
    SyntaxNode? root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
    SemanticModel? semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
    if (root is null || semanticModel is null)
      return;

    Diagnostic diagnostic = context.Diagnostics[0];
    TypeDeclarationSyntax? stateDeclaration = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?
      .AncestorsAndSelf()
      .OfType<TypeDeclarationSyntax>()
      .FirstOrDefault();
    if (stateDeclaration is null)
      return;

    if (semanticModel.GetDeclaredSymbol(stateDeclaration, context.CancellationToken) is not INamedTypeSymbol state)
      return;

    INamedTypeSymbol? jsonSerializerContext = semanticModel.Compilation
      .GetTypeByMetadataName(StateJsonSerializableAnalyzer.JsonSerializerContextMetadataName);
    if (jsonSerializerContext is null)
      return;

    string stateTypeName = state.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    string stateShortName = state.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    Solution solution = document.Project.Solution;

    List<(INamedTypeSymbol Context, Document ContextDocument, TypeDeclarationSyntax Declaration)> targets = [];
    foreach (INamedTypeSymbol contextType in FindContexts(semanticModel.Compilation.Assembly.GlobalNamespace, jsonSerializerContext))
    {
      foreach (SyntaxReference reference in contextType.DeclaringSyntaxReferences)
      {
        Document? contextDocument = solution.GetDocument(reference.SyntaxTree);
        if (contextDocument is null)
          continue; // generated (for example the System.Text.Json half of the partial)

        if (await reference.GetSyntaxAsync(context.CancellationToken).ConfigureAwait(false) is TypeDeclarationSyntax declaration)
        {
          targets.Add((contextType, contextDocument, declaration));
          break;
        }
      }
    }

    if (targets.Count == 0)
    {
      context.RegisterCodeFix
      (
        CodeAction.Create
        (
          $"Create {NewContextName} with [JsonSerializable(typeof({stateShortName}))]",
          cancellationToken => CreateContextAsync(document, state, stateTypeName, cancellationToken),
          equivalenceKey: nameof(CreateContextAsync)
        ),
        diagnostic
      );
      return;
    }

    foreach ((INamedTypeSymbol contextType, Document contextDocument, TypeDeclarationSyntax declaration) in targets.OrderBy(target => target.Context.Name, StringComparer.Ordinal))
    {
      context.RegisterCodeFix
      (
        CodeAction.Create
        (
          $"Add [JsonSerializable(typeof({stateShortName}))] to {contextType.Name}",
          cancellationToken => AddAttributeAsync(contextDocument, declaration.Span, stateTypeName, cancellationToken),
          equivalenceKey: nameof(AddAttributeAsync) + contextType.ToDisplayString()
        ),
        diagnostic
      );
    }
  }

  private static async Task<Document> AddAttributeAsync
  (
    Document contextDocument,
    TextSpan declarationSpan,
    string stateTypeName,
    CancellationToken cancellationToken
  )
  {
    SyntaxNode? root = await contextDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
    TypeDeclarationSyntax? declaration = root?.FindNode(declarationSpan).AncestorsAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault();
    if (root is null || declaration is null)
      return contextDocument;

    SyntaxTrivia endOfLine = FindEndOfLine(root);
    AttributeListSyntax attributeList = CreateAttributeList(stateTypeName);
    TypeDeclarationSyntax newDeclaration;

    if (declaration.AttributeLists.Count > 0)
    {
      AttributeListSyntax last = declaration.AttributeLists.Last();
      attributeList = attributeList
        .WithLeadingTrivia(Indentation(last.GetLeadingTrivia()))
        .WithTrailingTrivia(last.GetTrailingTrivia());
      newDeclaration = declaration.WithAttributeLists(declaration.AttributeLists.Add(attributeList));
    }
    else
    {
      // The declaration's leading trivia (doc comment, indentation) moves to the new first line.
      SyntaxTriviaList leading = declaration.GetLeadingTrivia();
      attributeList = attributeList
        .WithLeadingTrivia(leading)
        .WithTrailingTrivia(endOfLine);
      newDeclaration = declaration
        .WithoutLeadingTrivia()
        .WithLeadingTrivia(Indentation(leading))
        .WithAttributeLists(SyntaxFactory.SingletonList(attributeList));
    }

    return contextDocument.WithSyntaxRoot(root.ReplaceNode(declaration, newDeclaration));
  }

  private static async Task<Solution> CreateContextAsync
  (
    Document stateDocument,
    INamedTypeSymbol state,
    string stateTypeName,
    CancellationToken cancellationToken
  )
  {
    SyntaxNode? root = await stateDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
    string newLine = root is null ? "\n" : FindEndOfLine(root).ToFullString();
    bool fileScopedNamespace = stateDocument.Project.ParseOptions is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp10 };
    string? namespaceName = state.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace
      ? containingNamespace.ToDisplayString()
      : null;

    string indent = namespaceName is not null && !fileScopedNamespace ? "  " : "";
    StringBuilder text = new();
    text.Append("using System.Text.Json.Serialization;").Append(newLine).Append(newLine);
    if (namespaceName is not null)
    {
      text.Append("namespace ").Append(namespaceName);
      text.Append(fileScopedNamespace ? ";" + newLine + newLine : newLine + "{" + newLine);
    }

    text.Append(indent).Append("/// <summary>").Append(newLine);
    text.Append(indent).Append("/// Source-generated JSON type info for TimeWarp.State persistence and telemetry snapshots. Wire it with").Append(newLine);
    text.Append(indent).Append("/// <c>AddTimeWarpState(options => options.JsonSerializerOptions.TypeInfoResolver = ").Append(NewContextName).Append(".Default)</c>.").Append(newLine);
    text.Append(indent).Append("/// </summary>").Append(newLine);
    text.Append(indent).Append("[JsonSerializable(typeof(").Append(stateTypeName).Append("))]").Append(newLine);
    // A public state gets a public context so a host project (hosted WASM server, Auto/prerender server) can wire it as
    // TypeInfoResolver. A non-public state keeps the context internal: a public context exposing JsonTypeInfo for an
    // internal type would not compile.
    string accessibility = IsEffectivelyPublic(state) ? "public" : "internal";
    text.Append(indent).Append(accessibility).Append(" sealed partial class ").Append(NewContextName).Append(" : JsonSerializerContext").Append(newLine);
    text.Append(indent).Append('{').Append(newLine);
    text.Append(indent).Append('}').Append(newLine);
    if (namespaceName is not null && !fileScopedNamespace)
      text.Append('}').Append(newLine);

    Project project = stateDocument.Project;
    string fileName = UniqueFileName(project, stateDocument.Folders);
    string? filePath = stateDocument.FilePath is { } stateFilePath && System.IO.Path.GetDirectoryName(stateFilePath) is { } directory
      ? System.IO.Path.Combine(directory, fileName)
      : null;

    Document newDocument = project.AddDocument(fileName, text.ToString(), stateDocument.Folders, filePath);
    SyntaxNode? newRoot = await newDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
    if (newRoot is null)
      return newDocument.Project.Solution;

    // Shorten typeof(global::Ns.State) to what the new file's namespace allows.
    SyntaxNode annotatedRoot = newRoot.ReplaceNodes
    (
      newRoot.DescendantNodes().OfType<TypeOfExpressionSyntax>(),
      (_, typeOf) => typeOf.WithType(typeOf.Type.WithAdditionalAnnotations(Simplifier.Annotation))
    );
    return newDocument.WithSyntaxRoot(annotatedRoot).Project.Solution;
  }

  private static bool IsEffectivelyPublic(INamedTypeSymbol state)
  {
    for (INamedTypeSymbol? type = state; type is not null; type = type.ContainingType)
    {
      if (type.DeclaredAccessibility != Accessibility.Public)
        return false;
    }

    return true;
  }

  private static string UniqueFileName(Project project, IReadOnlyList<string> folders)
  {
    HashSet<string> existing = new
    (
      project.Documents.Where(document => document.Folders.SequenceEqual(folders)).Select(document => document.Name),
      StringComparer.OrdinalIgnoreCase
    );

    string baseName = System.IO.Path.GetFileNameWithoutExtension(NewContextFileName);
    string fileName = NewContextFileName;
    for (int suffix = 2; existing.Contains(fileName); suffix++)
      fileName = $"{baseName}-{suffix}.cs";

    return fileName;
  }

  private static AttributeListSyntax CreateAttributeList(string stateTypeName)
  {
    AttributeSyntax attribute = SyntaxFactory.Attribute
    (
      SyntaxFactory.ParseName(JsonSerializableAttributeName).WithAdditionalAnnotations(Simplifier.Annotation),
      SyntaxFactory.AttributeArgumentList
      (
        SyntaxFactory.SingletonSeparatedList
        (
          SyntaxFactory.AttributeArgument
          (
            SyntaxFactory.TypeOfExpression
            (
              SyntaxFactory.ParseTypeName(stateTypeName).WithAdditionalAnnotations(Simplifier.Annotation)
            )
          )
        )
      )
    );

    return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));
  }

  private static SyntaxTriviaList Indentation(SyntaxTriviaList trivia)
  {
    SyntaxTrivia last = trivia.LastOrDefault();
    return last.IsKind(SyntaxKind.WhitespaceTrivia) ? SyntaxFactory.TriviaList(last) : SyntaxFactory.TriviaList();
  }

  private static SyntaxTrivia FindEndOfLine(SyntaxNode root)
  {
    SyntaxTrivia endOfLine = root.DescendantTrivia().FirstOrDefault(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    return endOfLine.IsKind(SyntaxKind.EndOfLineTrivia) ? endOfLine : SyntaxFactory.EndOfLine("\n");
  }

  private static IEnumerable<INamedTypeSymbol> FindContexts(INamespaceSymbol namespaceSymbol, INamedTypeSymbol jsonSerializerContext)
  {
    foreach (INamespaceOrTypeSymbol member in namespaceSymbol.GetMembers())
    {
      if (member is INamespaceSymbol childNamespace)
      {
        foreach (INamedTypeSymbol context in FindContexts(childNamespace, jsonSerializerContext))
          yield return context;
      }
      else if (member is INamedTypeSymbol type)
      {
        foreach (INamedTypeSymbol context in FindContextTypes(type, jsonSerializerContext))
          yield return context;
      }
    }
  }

  private static IEnumerable<INamedTypeSymbol> FindContextTypes(INamedTypeSymbol type, INamedTypeSymbol jsonSerializerContext)
  {
    for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current, jsonSerializerContext))
      {
        yield return type;
        break;
      }
    }

    foreach (INamedTypeSymbol nestedType in type.GetTypeMembers())
    {
      foreach (INamedTypeSymbol context in FindContextTypes(nestedType, jsonSerializerContext))
        yield return context;
    }
  }
}
