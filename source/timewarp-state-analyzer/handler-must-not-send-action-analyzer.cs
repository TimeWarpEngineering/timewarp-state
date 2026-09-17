#region Purpose
// TWS0002: state action handlers must not dispatch actions; TWS0003 marks AllowActionSend exemptions.
#endregion

#region Design
// Symbol-based (IInvocationOperation), not name matching: a type is a handler if its base chain
// includes StateActionHandler<> / TimeWarp.Mediator.ActionHandler<> / the legacy TimeWarp.State.ActionHandler<>
// or it implements IActionHandler<>. That covers app bases (BaseHandler, DefaultApiHandler) without
// looking up those names.
// Two dispatch shapes: ISender/IMediator.Send of IAction, and ActionSet entry methods on IState
// (XState.FetchX). Architecture has no literal Send in handlers — generated instance methods are
// the hits. Identify those by a nested {MethodName}ActionSet on the containing IState type, which
// is user source, so the generator does not have to run for the rule to fire. Generators still run
// before analyzers in the same compilation, so calls to generated wrappers resolve.
// IAction is TimeWarp.Mediator.IAction in 12.0.0-beta.3+ and TimeWarp.State.IAction on older packages.
// GeneratedCodeAnalysisFlags.None: Sender.Send inside the generated wrapper is not a handler body
// and is not flagged. Publish/INotification, own-state mutation, and NavigationManager are ignored.
#endregion

namespace TimeWarp.State.Analyzer;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HandlerMustNotSendActionAnalyzer : DiagnosticAnalyzer
{
  public const string HandlerMustNotSendActionDiagnosticId = "TWS0002";
  public const string AllowActionSendDiagnosticId = "TWS0003";

  private const string Category = "Design";

  private const string StateActionHandlerMetadataName = "TimeWarp.State.StateActionHandler`1";
  private const string LegacyStateActionHandlerMetadataName = "TimeWarp.State.ActionHandler`1";
  private const string MediatorActionHandlerMetadataName = "TimeWarp.Mediator.ActionHandler`1";
  private const string IActionHandlerMetadataName = "TimeWarp.Mediator.IActionHandler`1";
  private const string IActionMetadataName = "TimeWarp.Mediator.IAction";
  private const string LegacyIActionMetadataName = "TimeWarp.State.IAction";
  private const string IStateMetadataName = "TimeWarp.State.IState";
  private const string ISenderMetadataName = "TimeWarp.Mediator.ISender";
  private const string ISenderGenericMetadataName = "TimeWarp.Mediator.ISender`1";
  private const string IMediatorMetadataName = "TimeWarp.Mediator.IMediator";
  private const string AllowActionSendMetadataName = "TimeWarp.State.AllowActionSendAttribute";

  private static readonly SymbolDisplayFormat ActionDisplayFormat =
    new
    (
      typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
      genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
    );

  private static readonly LocalizableString SendTitle = "Action handler must not send an action";
  private static readonly LocalizableString SendMessageFormat =
    "Action handler '{0}' sends action '{1}'. Handlers must not dispatch actions; publish a notification or sequence the action from the caller.";
  private static readonly LocalizableString SendDescription =
    "A State is an isolated feature boundary. Its action handlers do their own work and may publish a notification; they never send an action. Orchestration lives outside the state. Default severity is Warning; promote to Error with dotnet_diagnostic.TWS0002.severity = error in .editorconfig.";

  private static readonly DiagnosticDescriptor SendRule =
    new
    (
      HandlerMustNotSendActionDiagnosticId,
      SendTitle,
      SendMessageFormat,
      Category,
      DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: SendDescription
    );

  private static readonly LocalizableString AllowTitle = "AllowActionSend exemption is present";
  private static readonly LocalizableString AllowMessageFormat =
    "Action handler '{0}' is exempt from TWS0002 ({1}). Convert the dispatch to a notification and remove AllowActionSend.";
  private static readonly LocalizableString AllowDescription =
    "AllowActionSend is a temporary escape hatch. TWS0003 keeps the exemption visible as an info diagnostic until the handler publishes a notification instead of sending an action.";

  private static readonly DiagnosticDescriptor AllowRule =
    new
    (
      AllowActionSendDiagnosticId,
      AllowTitle,
      AllowMessageFormat,
      Category,
      DiagnosticSeverity.Info,
      isEnabledByDefault: true,
      description: AllowDescription
    );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SendRule, AllowRule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterCompilationStartAction(static compilationStartContext =>
    {
      Compilation compilation = compilationStartContext.Compilation;
      INamedTypeSymbol? iState = compilation.GetTypeByMetadataName(IStateMetadataName);
      if (iState is null)
        return;

      INamedTypeSymbol? iAction = compilation.GetTypeByMetadataName(IActionMetadataName)
        ?? compilation.GetTypeByMetadataName(LegacyIActionMetadataName);

      HandlerSymbols handlerSymbols = new
      (
        iAction,
        iState,
        compilation.GetTypeByMetadataName(StateActionHandlerMetadataName),
        compilation.GetTypeByMetadataName(LegacyStateActionHandlerMetadataName),
        compilation.GetTypeByMetadataName(MediatorActionHandlerMetadataName),
        compilation.GetTypeByMetadataName(IActionHandlerMetadataName),
        compilation.GetTypeByMetadataName(ISenderMetadataName),
        compilation.GetTypeByMetadataName(ISenderGenericMetadataName),
        compilation.GetTypeByMetadataName(IMediatorMetadataName),
        compilation.GetTypeByMetadataName(AllowActionSendMetadataName)
      );

      compilationStartContext.RegisterSymbolAction(
        symbolContext => AnalyzeNamedType(symbolContext, handlerSymbols),
        SymbolKind.NamedType);
      compilationStartContext.RegisterSymbolAction(
        symbolContext => AnalyzeMethod(symbolContext, handlerSymbols),
        SymbolKind.Method);
      compilationStartContext.RegisterOperationAction(
        operationContext => AnalyzeInvocation(operationContext, handlerSymbols),
        OperationKind.Invocation);
    });
  }

  private static void AnalyzeNamedType(SymbolAnalysisContext context, HandlerSymbols handlerSymbols)
  {
    INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
    if (!IsHandlerType(namedTypeSymbol, handlerSymbols))
      return;

    ReportAllowActionSendIfPresent(context, namedTypeSymbol, namedTypeSymbol, handlerSymbols);
  }

  private static void AnalyzeMethod(SymbolAnalysisContext context, HandlerSymbols handlerSymbols)
  {
    IMethodSymbol methodSymbol = (IMethodSymbol)context.Symbol;
    INamedTypeSymbol? containingType = methodSymbol.ContainingType;
    if (containingType is null || !IsHandlerType(containingType, handlerSymbols))
      return;

    ReportAllowActionSendIfPresent(context, methodSymbol, containingType, handlerSymbols);
  }

  private static void ReportAllowActionSendIfPresent
  (
    SymbolAnalysisContext context,
    ISymbol attributedSymbol,
    INamedTypeSymbol handlerType,
    HandlerSymbols handlerSymbols
  )
  {
    if (handlerSymbols.AllowActionSendAttribute is null)
      return;

    if (!TryGetDeclaredAllowActionSend(attributedSymbol, handlerSymbols.AllowActionSendAttribute, out AttributeData attributeData, out string reason))
      return;

    Location? location = attributeData.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
      ?? attributedSymbol.Locations.FirstOrDefault(static location => location.IsInSource);
    if (location is null)
      return;

    Diagnostic diagnostic = Diagnostic.Create(
      AllowRule,
      location,
      handlerType.Name,
      reason);
    context.ReportDiagnostic(diagnostic);
  }

  private static void AnalyzeInvocation(OperationAnalysisContext context, HandlerSymbols handlerSymbols)
  {
    if (context.Operation is not IInvocationOperation invocationOperation)
      return;

    INamedTypeSymbol? handlerType = GetContainingType(context.ContainingSymbol);
    if (handlerType is null || !IsHandlerType(handlerType, handlerSymbols))
      return;

    if (HasAllowActionSend(context.ContainingSymbol, handlerType, handlerSymbols))
      return;

    IMethodSymbol targetMethod = invocationOperation.TargetMethod;
    if (!TryGetDispatchedActionName(invocationOperation, targetMethod, handlerSymbols, out string actionName))
      return;

    Diagnostic diagnostic = Diagnostic.Create(
      SendRule,
      GetInvocationLocation(invocationOperation),
      handlerType.Name,
      actionName);
    context.ReportDiagnostic(diagnostic);
  }

  private static bool TryGetDispatchedActionName
  (
    IInvocationOperation invocationOperation,
    IMethodSymbol targetMethod,
    HandlerSymbols handlerSymbols,
    out string actionName
  )
  {
    if (handlerSymbols.IAction is not null
      && IsMediatorSend(targetMethod, handlerSymbols)
      && TryGetSentActionType(invocationOperation, handlerSymbols, out ITypeSymbol actionType))
    {
      actionName = actionType.ToDisplayString(ActionDisplayFormat);
      return true;
    }

    if (IsActionSetEntryMethod(targetMethod, handlerSymbols.IState))
    {
      actionName = targetMethod.Name;
      return true;
    }

    actionName = string.Empty;
    return false;
  }

  private static bool IsMediatorSend(IMethodSymbol methodSymbol, HandlerSymbols handlerSymbols)
  {
    if (methodSymbol.Name != "Send")
      return false;

    INamedTypeSymbol containingType = methodSymbol.ContainingType;
    return IsOrImplements(containingType, handlerSymbols.ISender)
      || IsOrImplements(containingType, handlerSymbols.ISenderGeneric)
      || IsOrImplements(containingType, handlerSymbols.IMediator);
  }

  private static bool TryGetSentActionType
  (
    IInvocationOperation invocationOperation,
    HandlerSymbols handlerSymbols,
    out ITypeSymbol actionType
  )
  {
    foreach (IArgumentOperation argumentOperation in invocationOperation.Arguments)
    {
      ITypeSymbol? argumentType = UnwrapConversion(argumentOperation.Value)?.Type;
      if (argumentType is null)
        continue;

      if (Implements(argumentType, handlerSymbols.IAction))
      {
        actionType = argumentType;
        return true;
      }
    }

    actionType = null!;
    return false;
  }

  private static IOperation? UnwrapConversion(IOperation operation)
  {
    IOperation current = operation;
    while (current is IConversionOperation conversionOperation)
    {
      current = conversionOperation.Operand;
    }

    return current;
  }

  private static bool IsActionSetEntryMethod(IMethodSymbol methodSymbol, INamedTypeSymbol iState)
  {
    INamedTypeSymbol containingType = methodSymbol.ContainingType;
    if (!Implements(containingType, iState))
      return false;

    string actionSetName = methodSymbol.Name + "ActionSet";
    for (INamedTypeSymbol? current = containingType; current is not null; current = current.BaseType)
    {
      if (current.GetTypeMembers(actionSetName).Length > 0)
        return true;
    }

    return false;
  }

  private static bool IsHandlerType(INamedTypeSymbol typeSymbol, HandlerSymbols handlerSymbols)
  {
    for (INamedTypeSymbol? current = typeSymbol; current is not null; current = current.BaseType)
    {
      INamedTypeSymbol originalDefinition = current.OriginalDefinition;
      if (SymbolEquals(originalDefinition, handlerSymbols.StateActionHandler)
        || SymbolEquals(originalDefinition, handlerSymbols.LegacyStateActionHandler)
        || SymbolEquals(originalDefinition, handlerSymbols.MediatorActionHandler))
      {
        return true;
      }
    }

    return Implements(typeSymbol, handlerSymbols.IActionHandler);
  }

  private static bool HasAllowActionSend
  (
    ISymbol containingSymbol,
    INamedTypeSymbol handlerType,
    HandlerSymbols handlerSymbols
  )
  {
    INamedTypeSymbol? allowAttribute = handlerSymbols.AllowActionSendAttribute;
    if (allowAttribute is null)
      return false;

    for (ISymbol? current = containingSymbol; current is not null; current = GetOverriddenSymbol(current))
    {
      if (HasAttribute(current, allowAttribute))
        return true;
    }

    for (INamedTypeSymbol? current = handlerType; current is not null; current = current.BaseType)
    {
      if (HasAttribute(current, allowAttribute))
        return true;
    }

    return false;
  }

  private static ISymbol? GetOverriddenSymbol(ISymbol symbol) =>
    symbol switch
    {
      IMethodSymbol { OverriddenMethod: { } overriddenMethod } => overriddenMethod,
      IPropertySymbol { OverriddenProperty: { } overriddenProperty } => overriddenProperty,
      _ => null
    };

  private static bool TryGetDeclaredAllowActionSend
  (
    ISymbol symbol,
    INamedTypeSymbol allowAttribute,
    out AttributeData attributeData,
    out string reason
  )
  {
    foreach (AttributeData candidate in symbol.GetAttributes())
    {
      if (!SymbolEquals(candidate.AttributeClass, allowAttribute))
        continue;

      attributeData = candidate;
      reason = candidate.ConstructorArguments.Length > 0
        ? candidate.ConstructorArguments[0].Value as string ?? string.Empty
        : string.Empty;
      return true;
    }

    attributeData = null!;
    reason = string.Empty;
    return false;
  }

  private static bool HasAttribute(ISymbol symbol, INamedTypeSymbol attributeType)
  {
    foreach (AttributeData attributeData in symbol.GetAttributes())
    {
      if (SymbolEquals(attributeData.AttributeClass, attributeType))
        return true;
    }

    return false;
  }

  private static bool Implements(ITypeSymbol typeSymbol, INamedTypeSymbol? interfaceType)
  {
    if (interfaceType is null)
      return false;

    if (SymbolEquals(typeSymbol.OriginalDefinition, interfaceType))
      return true;

    foreach (INamedTypeSymbol implemented in typeSymbol.AllInterfaces)
    {
      if (SymbolEquals(implemented.OriginalDefinition, interfaceType))
        return true;
    }

    return false;
  }

  private static bool IsOrImplements(ITypeSymbol typeSymbol, INamedTypeSymbol? targetType)
  {
    if (targetType is null)
      return false;

    if (SymbolEquals(typeSymbol.OriginalDefinition, targetType))
      return true;

    return Implements(typeSymbol, targetType);
  }

  private static bool SymbolEquals(ISymbol? left, ISymbol? right) =>
    left is not null
    && right is not null
    && SymbolEqualityComparer.Default.Equals(left, right);

  private static INamedTypeSymbol? GetContainingType(ISymbol? symbol)
  {
    ISymbol? current = symbol;
    while (current is not null)
    {
      if (current is INamedTypeSymbol namedTypeSymbol)
        return namedTypeSymbol;

      current = current.ContainingType ?? current.ContainingSymbol;
    }

    return null;
  }

  private static Location GetInvocationLocation(IInvocationOperation invocationOperation)
  {
    if (invocationOperation.Syntax is InvocationExpressionSyntax invocationExpression)
    {
      if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess)
        return memberAccess.Name.GetLocation();

      return invocationExpression.Expression.GetLocation();
    }

    return invocationOperation.Syntax.GetLocation();
  }

  private sealed class HandlerSymbols
  (
    INamedTypeSymbol? iAction,
    INamedTypeSymbol iState,
    INamedTypeSymbol? stateActionHandler,
    INamedTypeSymbol? legacyStateActionHandler,
    INamedTypeSymbol? mediatorActionHandler,
    INamedTypeSymbol? iActionHandler,
    INamedTypeSymbol? iSender,
    INamedTypeSymbol? iSenderGeneric,
    INamedTypeSymbol? iMediator,
    INamedTypeSymbol? allowActionSendAttribute
  )
  {
    public INamedTypeSymbol? IAction { get; } = iAction;
    public INamedTypeSymbol IState { get; } = iState;
    public INamedTypeSymbol? StateActionHandler { get; } = stateActionHandler;
    public INamedTypeSymbol? LegacyStateActionHandler { get; } = legacyStateActionHandler;
    public INamedTypeSymbol? MediatorActionHandler { get; } = mediatorActionHandler;
    public INamedTypeSymbol? IActionHandler { get; } = iActionHandler;
    public INamedTypeSymbol? ISender { get; } = iSender;
    public INamedTypeSymbol? ISenderGeneric { get; } = iSenderGeneric;
    public INamedTypeSymbol? IMediator { get; } = iMediator;
    public INamedTypeSymbol? AllowActionSendAttribute { get; } = allowActionSendAttribute;
  }
}
