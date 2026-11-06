#region Purpose
// Turns one state type declaration into an equatable StateFact for the State manifest.
#endregion

#region Design
// Candidates are class and record declarations with a base list, the same narrow syntax filter Mediator's host
// candidates use. The transform keeps concrete types that implement TimeWarp.State.IState. A partial state is
// reported once, from its first declaration that carries a base list.
// CloneKind: "cloneable" when the state implements ICloneable (the clone emitter skips it and ICloneable wins at
// runtime), otherwise "generated". The clone plan is still the authority on what is emitted. This is the manifest's
// summary only.
// Generic states are kept under their metadata name (BoxState`1), the name the runtime and telemetry use.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal static class StateFactExtractor
{
  public static bool IsCandidateSyntax(SyntaxNode node) =>
    node is ClassDeclarationSyntax { BaseList: not null } or RecordDeclarationSyntax { BaseList: not null, ClassOrStructKeyword.ValueText: not "struct" };

  public static StateFact? Extract(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    if (context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol type)
      return null;
    if (type.IsAbstract || type.TypeKind != TypeKind.Class) return null;

    INamedTypeSymbol? stateInterface = context.SemanticModel.Compilation.GetTypeByMetadataName(StateSymbolFacts.StateInterfaceMetadataName);
    if (stateInterface is null || !StateSymbolFacts.ImplementsState(type, stateInterface)) return null;

    SyntaxReference? first = type.DeclaringSyntaxReferences
      .FirstOrDefault(reference => reference.GetSyntax(cancellationToken) is TypeDeclarationSyntax { BaseList: not null });
    if (first is null || first.SyntaxTree != context.Node.SyntaxTree || first.Span != context.Node.Span) return null;

    bool cloneable = type.AllInterfaces.Any(candidate => candidate.ToDisplayString() == "System.ICloneable");
    return new StateFact(StateSymbolFacts.FullMetadataName(type), type.MetadataName, cloneable ? "cloneable" : "generated");
  }
}
