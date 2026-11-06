#region Purpose
// The [Reducer] rule shared as linked source by the State generator (which emits the Handler) and ReducerAnalyzer
// (TWS0014, which reports a [Reducer] the generator will not serve and says why).
#endregion

#region Design
// Task 144, Option 3b. One rule in one file, so the analyzer and the generator cannot drift (task 142). The analyzer
// project compiles it via <Compile Include ... Link>: only Roslyn and BCL types, only the global usings both projects
// share, internal visibility.
// The generator emits <ActionSet>.<HandlerName> : StateActionHandler<Action> that calls the action set's
//   static void Reduce(TState state, Action action)
// The shape, checked in this order (the first failure is the reason):
//   1. Placement: a class named Action, directly in a class whose name ends in ActionSet, directly in a type that
//      implements TimeWarp.State.IState. ActionSetShape uses the same nesting, so a fact exists for every accepted
//      action and the generator needs no second scan.
//   2. Nothing generic, and the state is not abstract (Store.GetState<T> could not create it).
//   3. The Action implements TimeWarp.Mediator.IAction (StateActionHandler<TAction> requires it).
//   4. The action set, the state and every containing type are partial, so the emitted partial merges.
//   5. The Action is public or internal. The Handler takes the same accessibility, so its base type is never less
//      accessible than the Handler (CS0060) and a public Action in a library still gets a public Handler.
//   6. The handler name (the attribute's handlerTypeName argument, default Handler) is a simple identifier. Mediator
//      resolves it relative to the Action's containing type, so a dotted name would point outside the action set.
//   7. The action set has no member of that name already, apart from the one this generator emitted. The analyzer
//      runs on the final compilation and sees the generated Handler, so a type carrying
//      [GeneratedCode("TimeWarp.State.SourceGenerator", ...)] does not count.
//   8. Exactly one static, non-generic, void Reduce(TState, Action) with no ref parameters is declared in the action
//      set. Any accessibility works, because the Handler is nested in the action set.
// Known limit, shared with the catalog fact: the generator's transform reruns only when the Action's own tree changes.
// A Reduce method declared in another partial file shows up on that tree's next edit.
#endregion

namespace TimeWarp.State.Facts;

internal sealed class ReducerPlan
{
  public ReducerPlan(INamedTypeSymbol state, INamedTypeSymbol actionSet, INamedTypeSymbol action, string handlerName, string accessibility)
  {
    State = state;
    ActionSet = actionSet;
    Action = action;
    HandlerName = handlerName;
    Accessibility = accessibility;
  }

  public INamedTypeSymbol State { get; }
  public INamedTypeSymbol ActionSet { get; }
  public INamedTypeSymbol Action { get; }
  public string HandlerName { get; }
  public string Accessibility { get; }
}

internal static class ReducerFacts
{
  public const string AttributeMetadataName = "TimeWarp.State.ReducerAttribute";
  public const string ActionInterfaceMetadataName = "TimeWarp.Mediator.IAction";
  public const string GeneratedCodeAttributeMetadataName = "System.CodeDom.Compiler.GeneratedCodeAttribute";
  public const string GeneratorToolName = "TimeWarp.State.SourceGenerator";
  public const string HandlerTypeNameParameter = "handlerTypeName";
  public const string DefaultHandlerName = "Handler";
  public const string ReduceMethodName = "Reduce";

  public static AttributeData? FindAttribute(INamedTypeSymbol action, INamedTypeSymbol reducerAttribute)
  {
    foreach (AttributeData attribute in action.GetAttributes())
    {
      if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, reducerAttribute))
        return attribute;
    }

    return null;
  }

  /// <summary>
  /// The plan the generator emits from, or the reason it emits nothing. Exactly one of the two is non-null.
  /// </summary>
  public static ReducerPlan? Analyze(INamedTypeSymbol action, AttributeData attribute, Compilation compilation, out string? failure)
  {
    failure = null;
    if (action.Name != "Action" || action.TypeKind != TypeKind.Class
        || action.ContainingType is not { TypeKind: TypeKind.Class } actionSet
        || !actionSet.Name.EndsWith("ActionSet", StringComparison.Ordinal)
        || actionSet.ContainingType is not { } state
        || compilation.GetTypeByMetadataName(StateSymbolFacts.StateInterfaceMetadataName) is not { } stateInterface
        || !StateSymbolFacts.ImplementsState(state, stateInterface))
    {
      failure = "it is not the nested Action class of an *ActionSet inside a state";
      return null;
    }

    for (INamedTypeSymbol? current = action; current is not null; current = current.ContainingType)
    {
      if (current.IsGenericType)
      {
        failure = $"'{current.ToDisplayString()}' is generic, so Mediator cannot name the generated handler";
        return null;
      }
    }

    if (state.IsAbstract)
    {
      failure = $"the state '{state.ToDisplayString()}' is abstract, so the store cannot provide it to Reduce";
      return null;
    }

    INamedTypeSymbol? actionInterface = compilation.GetTypeByMetadataName(ActionInterfaceMetadataName);
    if (actionInterface is null || !action.AllInterfaces.Contains(actionInterface, SymbolEqualityComparer.Default))
    {
      failure = "the Action does not implement IAction";
      return null;
    }

    for (INamedTypeSymbol? current = actionSet; current is not null; current = current.ContainingType)
    {
      if (!IsPartial(current))
      {
        failure = $"'{current.ToDisplayString()}' is not partial; add the partial modifier so the generated handler can join it";
        return null;
      }
    }

    string accessibility;
    switch (action.DeclaredAccessibility)
    {
      case Microsoft.CodeAnalysis.Accessibility.Public:
        accessibility = "public";
        break;
      case Microsoft.CodeAnalysis.Accessibility.Internal:
      case Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal:
        accessibility = "internal";
        break;
      default:
        failure = "the Action is private or protected, so the generated handler and Mediator cannot reach it";
        return null;
    }

    string? handlerName = ReadHandlerName(attribute);
    if (handlerName is null || !SyntaxFacts.IsValidIdentifier(handlerName))
    {
      failure = $"the handler name '{handlerName}' is not a simple type name; the generator emits one type nested in the action set";
      return null;
    }

    INamedTypeSymbol? generatedCode = compilation.GetTypeByMetadataName(GeneratedCodeAttributeMetadataName);
    foreach (ISymbol member in actionSet.GetMembers(handlerName))
    {
      if (!IsEmittedByThisGenerator(member, generatedCode))
      {
        failure = $"the action set already declares '{handlerName}'; remove it, or remove [Reducer] and keep the hand-written handler";
        return null;
      }
    }

    int reducers = actionSet.GetMembers(ReduceMethodName)
      .OfType<IMethodSymbol>()
      .Count(method => IsReducer(method, state, action));
    if (reducers != 1)
    {
      failure = $"the action set does not declare 'static void {ReduceMethodName}({state.Name} state, Action action)'";
      return null;
    }

    return new ReducerPlan(state, actionSet, action, handlerName, accessibility);
  }

  /// <summary>The handlerTypeName constructor argument, including its default. Null when it is not a string.</summary>
  public static string? ReadHandlerName(AttributeData attribute)
  {
    if (attribute.AttributeConstructor is not { } constructor) return DefaultHandlerName;

    int count = Math.Min(constructor.Parameters.Length, attribute.ConstructorArguments.Length);
    for (int index = 0; index < count; index++)
    {
      if (constructor.Parameters[index].Name != HandlerTypeNameParameter) continue;
      return attribute.ConstructorArguments[index].Value as string;
    }

    return DefaultHandlerName;
  }

  private static bool IsReducer(IMethodSymbol method, INamedTypeSymbol state, INamedTypeSymbol action) =>
    method.IsStatic
    && method.ReturnsVoid
    && !method.IsGenericMethod
    && method.Parameters.Length == 2
    && method.Parameters.All(parameter => parameter.RefKind == RefKind.None)
    && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, state)
    && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, action);

  private static bool IsEmittedByThisGenerator(ISymbol member, INamedTypeSymbol? generatedCode) =>
    generatedCode is not null
    && member is INamedTypeSymbol
    && member.GetAttributes().Any(attribute =>
      SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, generatedCode)
      && attribute.ConstructorArguments.Length > 0
      && attribute.ConstructorArguments[0].Value as string == GeneratorToolName);

  private static bool IsPartial(INamedTypeSymbol type) =>
    type.DeclaringSyntaxReferences.Length > 0
    && type.DeclaringSyntaxReferences.All(reference =>
      reference.GetSyntax() is TypeDeclarationSyntax declaration
      && declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)));
}
