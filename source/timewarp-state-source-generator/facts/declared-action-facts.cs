#region Purpose
// Reads TimeWarp.Mediator's GeneratedRequestPatternAttribute (and attributes derived from it) and turns each declared
// request that lives under a State type into a declared action: the state, the action set, and the expanded request,
// handler and request-interface names. Shared as linked source by the State generator (dispatch method, catalog entry,
// manifest row) and DeclaredActionAnalyzer (TWS0015).
#endregion

#region Design
// Roslyn generators cannot see each other's output. Another generator may emit State.XActionSet.Action and its
// handler; Mediator task 043 (15.0.0-beta.10) lets a source type declare those requests by constant name templates
// expanded once per contract type. This file is State's copy of that expansion. Mediator ships its reader inside its
// analyzer assembly, which State cannot reference, so the rules are mirrored here and pinned by
// DeclaredActionFactsTests against Mediator's documented set:
// - Templates come from a constructor parameter named requestTypeName / handlerTypeName / requestInterfaceName (a
//   default value is enough) or from a const string field RequestTypeName / HandlerTypeName / RequestInterfaceName
//   on the attribute class chain. Contracts come from a constructor parameter named contracts. Properties are not
//   read: a getter body does not survive in a reference assembly.
// - Placeholders: {Host} (the attributed type's dotted full name), {Contract} (the contract's simple name),
//   {ContractType} (the contract's dotted full name). Anything else, an unmatched brace, a generic host or contract,
//   or a name that is not a dotted type name makes that pattern (or contract) expand to nothing here.
// Failures are silent on purpose. Mediator's TWM013 already reports a pattern it cannot read or expand, and two
// diagnostics for one typo help nobody. State only adds TWS0015 for its own fact: a declared action the final
// compilation does not contain.
// Placement (State's own rule, the same shape a source action must have): the request is
// <container>.<Name>ActionSet.Action, where <Name> is not empty and <container> resolves to a non-generic type in
// this assembly that implements TimeWarp.State.IState. Any other declared request is Mediator's business only.
// Templates and name resolution: declared-action-facts.templates.cs.
#endregion

namespace TimeWarp.State.Facts;

/// <summary>One request a GeneratedRequestPattern declares under a State type.</summary>
internal sealed class DeclaredAction
{
  public DeclaredAction
  (
    INamedTypeSymbol host,
    INamedTypeSymbol contract,
    INamedTypeSymbol state,
    string actionSetName,
    string requestName,
    string handlerName,
    string requestInterfaceName,
    AttributeData attribute
  )
  {
    Host = host;
    Contract = contract;
    State = state;
    ActionSetName = actionSetName;
    RequestName = requestName;
    HandlerName = handlerName;
    RequestInterfaceName = requestInterfaceName;
    Attribute = attribute;
  }

  /// <summary>The type that carries the pattern attribute.</summary>
  public INamedTypeSymbol Host { get; }

  public INamedTypeSymbol Contract { get; }

  /// <summary>The state the declared action lives under.</summary>
  public INamedTypeSymbol State { get; }

  /// <summary>The action set's simple name, for example <c>CreateRoleActionSet</c>.</summary>
  public string ActionSetName { get; }

  /// <summary>Dotted request name without <c>global::</c>, for example <c>App.RoleState.CreateRoleActionSet.Action</c>.</summary>
  public string RequestName { get; }

  /// <summary>Dotted handler name without <c>global::</c>.</summary>
  public string HandlerName { get; }

  /// <summary>The expanded request interface text, for example <c>TimeWarp.Mediator.IRequest&lt;App.CreateRole.Response&gt;</c>.</summary>
  public string RequestInterfaceName { get; }

  public AttributeData Attribute { get; }

  /// <summary>The dispatch method name: the action set name without its ActionSet suffix.</summary>
  public string MethodName => ActionSetName.Substring(0, ActionSetName.Length - "ActionSet".Length);
}

internal static partial class DeclaredActionFacts
{
  public const string PatternAttributeMetadataName = "TimeWarp.Mediator.GeneratedRequestPatternAttribute";

  public static INamedTypeSymbol? GetPatternAttribute(Compilation compilation) =>
    compilation.GetTypeByMetadataName(PatternAttributeMetadataName);

  /// <summary>True when <paramref name="attributeClass"/> is the pattern attribute or derives from it.</summary>
  public static bool IsPattern(INamedTypeSymbol? attributeClass, INamedTypeSymbol patternAttribute)
  {
    for (INamedTypeSymbol? current = attributeClass; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current, patternAttribute)) return true;
    }

    return false;
  }

  /// <summary>Every declared action the pattern attributes on <paramref name="host"/> place under a State type.</summary>
  public static List<DeclaredAction> Read
  (
    INamedTypeSymbol host,
    Compilation compilation,
    INamedTypeSymbol patternAttribute,
    CancellationToken cancellationToken
  )
  {
    List<DeclaredAction> actions = [];
    INamedTypeSymbol? stateInterface = compilation.GetTypeByMetadataName(StateSymbolFacts.StateInterfaceMetadataName);
    if (stateInterface is null) return actions;

    foreach (AttributeData attribute in host.GetAttributes())
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (!IsPattern(attribute.AttributeClass, patternAttribute)) continue;

      ReadOne(host, compilation, stateInterface, attribute, actions);
    }

    return actions;
  }

  private static void ReadOne
  (
    INamedTypeSymbol host,
    Compilation compilation,
    INamedTypeSymbol stateInterface,
    AttributeData attribute,
    List<DeclaredAction> actions
  )
  {
    if (!TryReadTemplate(attribute, "requestTypeName", "RequestTypeName", out string requestTemplate)) return;
    if (!TryReadTemplate(attribute, "handlerTypeName", "HandlerTypeName", out string handlerTemplate)) return;
    if (!TryReadTemplate(attribute, "requestInterfaceName", "RequestInterfaceName", out string interfaceTemplate)) return;
    if (!TryReadContracts(attribute, out ImmutableArray<TypedConstant> contracts)) return;

    string? hostName = DottedName(host);
    foreach (TypedConstant constant in contracts)
    {
      if (constant.Value is not INamedTypeSymbol contract || contract.TypeKind == TypeKind.Error) continue;
      if (DottedName(contract) is not { } contractName) continue;

      // Mediator stops at the first template that does not expand; so does State.
      if (!TryExpand(requestTemplate, hostName, contract.Name, contractName, out string requestName)
          || !TryExpand(handlerTemplate, hostName, contract.Name, contractName, out string handlerName)
          || !TryExpand(interfaceTemplate, hostName, contract.Name, contractName, out string interfaceName))
        return;

      if (!IsDottedTypeName(requestName) || !IsDottedTypeName(handlerName)) continue;

      requestName = StripGlobal(requestName);
      if (!TryPlace(compilation, stateInterface, requestName, out INamedTypeSymbol state, out string actionSetName)) continue;

      actions.Add
      (
        new DeclaredAction(host, contract, state, actionSetName, requestName, StripGlobal(handlerName), StripGlobal(interfaceName), attribute)
      );
    }
  }

  /// <summary>
  /// State's placement rule: <c>&lt;container&gt;.&lt;Name&gt;ActionSet.Action</c> with a non-generic State container
  /// declared in this assembly.
  /// </summary>
  public static bool TryPlace
  (
    Compilation compilation,
    INamedTypeSymbol stateInterface,
    string requestName,
    out INamedTypeSymbol state,
    out string actionSetName
  )
  {
    state = null!;
    actionSetName = string.Empty;
    string[] segments = requestName.Split('.');
    if (segments.Length < 3 || segments[segments.Length - 1] != "Action") return false;

    string actionSet = segments[segments.Length - 2];
    if (!actionSet.EndsWith("ActionSet", StringComparison.Ordinal) || actionSet.Length == "ActionSet".Length) return false;

    string containerName = string.Join(".", segments, 0, segments.Length - 2);
    if (ResolveDotted(compilation, containerName) is not { } container) return false;
    if (!SymbolEqualityComparer.Default.Equals(container.ContainingAssembly, compilation.Assembly)) return false;
    if (container.IsGenericType) return false;
    if (!StateSymbolFacts.ImplementsState(container, stateInterface)) return false;

    state = container;
    actionSetName = actionSet;
    return true;
  }
}
