#region Purpose
// Decides whether a nested `Action` type is a State action set the generators should serve, and if so whether the
// generated State.Method can compile. Shared by the ActionSet method generator and the action catalog generator.
#endregion

#region Design
// Discovery is semantic. The Action is a class, record or struct named `Action`, directly nested in a class whose
// name ends in `ActionSet`, which is directly nested in a type that implements TimeWarp.State.IState (State<T> does).
// IState rather than State<T>: a hand-rolled IState that compiled with the generated method before must keep it.
// Anything else is not an action set: Analyze returns null and the generators stay silent. That covers a nested
// *ActionSet in a static docs class, which used to receive a `partial class Docs` that did not compile.
//
// When it is an action set but the generated method could not compile, Failure names the reason and the method
// generator reports TWSG003. The catalog generator skips the entry, so the user sees one error, not a TWSG003 plus a
// follow-on CS1061 in the catalog. Every Failure below used to surface as a compiler error in generated code.
//
// Generated method accessibility is public, except when the state is effectively public and a parameter type is only
// internal. That used to be CS0051, so internal is the only form that compiles and nothing that compiled changes.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed record ActionSetAnalysis
(
  INamedTypeSymbol State,
  INamedTypeSymbol ActionSet,
  INamedTypeSymbol Action,
  string MethodName,
  string MethodAccessibility,
  ActionConstruction Construction,
  string? Failure,
  bool MethodDeclaredByUser
);

internal static class ActionSetShape
{
  public const string StateInterfaceMetadataName = "TimeWarp.State.IState";
  public const string ExternalCancellationTokenName = "externalCancellationToken";

  public static bool IsCandidateSyntax(SyntaxNode node) =>
    node is TypeDeclarationSyntax { Identifier.ValueText: "Action", Parent: ClassDeclarationSyntax actionSet }
    && node is not InterfaceDeclarationSyntax
    && actionSet.Identifier.ValueText.EndsWith("ActionSet", StringComparison.Ordinal)
    && actionSet.Parent is TypeDeclarationSyntax;

  public static ActionSetAnalysis? Analyze(INamedTypeSymbol action, Compilation compilation, CancellationToken cancellationToken)
  {
    if (action.Name != "Action" || action.TypeKind is not (TypeKind.Class or TypeKind.Struct)) return null;
    if (action.ContainingType is not { TypeKind: TypeKind.Class } actionSet) return null;
    if (!actionSet.Name.EndsWith("ActionSet", StringComparison.Ordinal)) return null;
    if (actionSet.ContainingType is not { } state) return null;

    INamedTypeSymbol? stateInterface = compilation.GetTypeByMetadataName(StateInterfaceMetadataName);
    if (stateInterface is null || !state.AllInterfaces.Contains(stateInterface, SymbolEqualityComparer.Default)) return null;

    string methodName = actionSet.Name.Replace(oldValue: "ActionSet", newValue: "");
    ActionConstruction construction = methodName.Length == 0 || IsGeneric(action, actionSet)
      ? new ActionConstruction(null, [], [])
      : ActionSetConstructorParser.Parse(action, cancellationToken);

    bool declaredByUser = methodName.Length > 0 && IsDeclaredByUser(action, state, methodName, construction, compilation);
    string accessibility = "public";
    string? failure = declaredByUser ? null : FindFailure(state, actionSet, action, methodName, construction, ref accessibility);
    return new ActionSetAnalysis(state, actionSet, action, methodName, accessibility, construction, failure, declaredByUser);
  }

  // The state already declares the method by hand, so the generator emits nothing and the catalog calls the user's.
  // - A record or struct Action got no generated method before task 141, so apps wrote their own (the test app's
  //   ApplicationState.FiveSecondTask). Any user method with the name counts: adding a generated overload beside it
  //   could make an existing call ambiguous (CS0121), which would break code that compiles today.
  // - A plain class Action always had a generated method, so only an identical signature counts (that was CS0111).
  private static bool IsDeclaredByUser(INamedTypeSymbol action, INamedTypeSymbol state, string methodName, ActionConstruction construction, Compilation compilation)
  {
    bool servedBefore = action is { TypeKind: TypeKind.Class, IsRecord: false };
    if (!servedBefore && state.GetMembers(methodName).OfType<IMethodSymbol>().Any())
      return true;

    INamedTypeSymbol? nullable = compilation.GetSpecialType(SpecialType.System_Nullable_T) as INamedTypeSymbol;
    INamedTypeSymbol? token = compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
    if (nullable is null || token is null) return false;

    List<ITypeSymbol> generated = [.. construction.ParameterTypes, nullable.Construct(token)];
    return state.GetMembers(methodName)
      .OfType<IMethodSymbol>()
      .Any(method => method.Parameters.Length == generated.Count
        && method.Parameters.Select(parameter => parameter.Type).SequenceEqual(generated, SymbolEqualityComparer.Default));
  }

  private static string? FindFailure
  (
    INamedTypeSymbol state,
    INamedTypeSymbol actionSet,
    INamedTypeSymbol action,
    string methodName,
    ActionConstruction construction,
    ref string accessibility
  )
  {
    if (methodName.Length == 0)
      return $"the action set '{actionSet.Name}' has no name before 'ActionSet' to name the method";

    if (IsGeneric(action, actionSet))
      return "the Action or its ActionSet is generic, so the method cannot name the Action";

    for (INamedTypeSymbol? current = state; current is not null; current = current.ContainingType)
    {
      if (!IsPartial(current))
        return $"'{current.ToDisplayString()}' is not partial; add the partial modifier so the generated method can join it";
    }

    if (action.IsAbstract)
      return "the Action is abstract and cannot be constructed";

    if (!IsAccessibleFromState(action.DeclaredAccessibility))
      return "the Action is private or protected inside its ActionSet, so the state cannot construct it";

    if (construction.Constructor is { } constructor && !IsAccessibleFromState(constructor.DeclaredAccessibility))
      return "the Action constructor is private or protected, so the state cannot call it";

    HashSet<string> names = new(StringComparer.Ordinal);
    foreach (ActionParameterModel parameter in construction.Parameters)
    {
      if (parameter.CatalogName == ExternalCancellationTokenName)
        return $"parameter '{parameter.CatalogName}' collides with the generated '{ExternalCancellationTokenName}' parameter";
      if (!names.Add(parameter.CatalogName))
        return $"two parameters would both be named '{parameter.CatalogName}' (a required member and a constructor parameter)";
    }

    ISymbol? clash = state.GetMembers(methodName).FirstOrDefault(member => member.Kind != SymbolKind.Method);
    if (clash is not null)
      return $"'{state.ToDisplayString()}' already has a {clash.Kind.ToString().ToLowerInvariant()} named '{methodName}'";

    int stateRank = EffectiveRank(state);
    int parameterRank = construction.Constructor?.Parameters.Select(parameter => EffectiveRank(parameter.Type)).DefaultIfEmpty(PublicRank).Min() ?? PublicRank;
    parameterRank = Math.Min(parameterRank, RequiredMemberRank(action));
    if (parameterRank < InternalRank)
      return "a parameter type is private or protected, so a method on the state cannot expose it";
    if (stateRank == PublicRank && parameterRank == InternalRank)
      accessibility = "internal";

    return null;
  }

  private const int PublicRank = 2;
  private const int InternalRank = 1;

  private static int RequiredMemberRank(INamedTypeSymbol action)
  {
    int rank = PublicRank;
    for (INamedTypeSymbol? current = action; current is not null; current = current.BaseType)
    {
      foreach (ISymbol member in current.GetMembers())
      {
        ITypeSymbol? type = member switch
        {
          IPropertySymbol { IsRequired: true } property => property.Type,
          IFieldSymbol { IsRequired: true } field => field.Type,
          _ => null
        };
        if (type is not null) rank = Math.Min(rank, EffectiveRank(type));
      }
    }

    return rank;
  }

  // Only the Action's and ActionSet's own type parameters matter: inside a generic state, `new XActionSet.Action()`
  // binds through the state's type parameters already.
  private static bool IsGeneric(INamedTypeSymbol action, INamedTypeSymbol actionSet) =>
    action.TypeParameters.Length > 0 || actionSet.TypeParameters.Length > 0;

  private static bool IsAccessibleFromState(Accessibility accessibility) =>
    accessibility is not (Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal);

  private static bool IsPartial(INamedTypeSymbol type) =>
    type.DeclaringSyntaxReferences.Length > 0
    && type.DeclaringSyntaxReferences.All(reference =>
      reference.GetSyntax() is TypeDeclarationSyntax declaration
      && declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)));

  private static int Rank(Accessibility accessibility) => accessibility switch
  {
    Accessibility.Public or Accessibility.NotApplicable => PublicRank,
    Accessibility.Internal or Accessibility.ProtectedOrInternal => InternalRank,
    _ => 0
  };

  internal static int EffectiveRank(ITypeSymbol type)
  {
    switch (type)
    {
      case IArrayTypeSymbol array:
        return EffectiveRank(array.ElementType);
      case IPointerTypeSymbol pointer:
        return EffectiveRank(pointer.PointedAtType);
      case INamedTypeSymbol named:
        int rank = PublicRank;
        for (INamedTypeSymbol? current = named; current is not null; current = current.ContainingType)
          rank = Math.Min(rank, Rank(current.DeclaredAccessibility));
        foreach (ITypeSymbol argument in named.TypeArguments)
          rank = Math.Min(rank, EffectiveRank(argument));
        return rank;
      default:
        return PublicRank;
    }
  }
}
