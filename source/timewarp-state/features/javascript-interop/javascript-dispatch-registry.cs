#region Purpose
// The allow-list of request types that JavaScript may dispatch through JsonRequestHandler.
#endregion

#region Design
// Opt-in by explicit registration (AddJavaScriptDispatch(b => b.Allow<TAction>())), not by
// attribute or by [CatalogAction]:
// - The generic constraint (TAction : class, IAction) proves "is an action" at compile time,
//   so no analyzer and no runtime reflection scan are needed.
// - The action catalog is a user-facing palette/agent surface; JS dispatch is an interop
//   surface. Coupling them would expose every cataloged action to page script and force a
//   Description on purely technical interop actions.
// - An attribute would need a second source-generated registry for the same result.
//
// Names never reach Type.GetType. Each allowed type is indexed under its FullName,
// "FullName, AssemblyName", its AssemblyQualifiedName and an optional alias. Lookup tries the
// exact wire name, then the "FullName, AssemblyName" prefix of an assembly-qualified name, so
// the existing DispatchRequest("Ns.Type+Nested, Assembly, Version=…", …) call shape keeps
// working when the version differs.
//
// Redux DevTools requests are not actions. UseReduxDevTools adds them through the internal
// AllowReduxDevToolsRequests, so they are dispatchable only when DevTools is enabled.
//
// One instance per IServiceCollection: GetOrAdd finds the registered singleton instance so
// AddJavaScriptDispatch, AddTimeWarpState and UseReduxDevTools can run in any order.
#endregion

namespace TimeWarp.Features.JavaScriptInterop;

/// <summary>
/// The request types that JavaScript may dispatch via <c>timeWarpState.DispatchRequest</c>.
/// </summary>
/// <remarks>Populate it with <see cref="ServiceCollectionExtensions.AddJavaScriptDispatch"/>.</remarks>
public sealed class JavaScriptDispatchRegistry
{
  private readonly Dictionary<string, Type> TypesByName = new(StringComparer.Ordinal);

  /// <summary>The distinct allowed request types.</summary>
  public IReadOnlyCollection<Type> AllowedTypes => TypesByName.Values.Distinct().ToArray();

  internal void Add(Type requestType, string? alias = null)
  {
    ArgumentNullException.ThrowIfNull(requestType);
    if (requestType.IsAbstract || requestType.IsGenericTypeDefinition || requestType.FullName is null)
    {
      throw new ArgumentException($"'{requestType}' must be a concrete, closed type.", nameof(requestType));
    }

    AddName(requestType.FullName, requestType);
    AddName($"{requestType.FullName}, {requestType.Assembly.GetName().Name}", requestType);
    if (requestType.AssemblyQualifiedName is not null)
      AddName(requestType.AssemblyQualifiedName, requestType);

    if (alias is not null)
    {
      if (string.IsNullOrWhiteSpace(alias))
        throw new ArgumentException("Alias must not be empty or whitespace.", nameof(alias));

      AddName(alias, requestType);
    }
  }

  internal void AllowReduxDevToolsRequests()
  {
    Add(typeof(StartRequest));
    Add(typeof(CommitRequest));
  }

  /// <summary>Resolves a wire name sent from JavaScript against the allow-list only.</summary>
  public bool TryResolve(string requestTypeName, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Type? requestType)
  {
    requestType = null;
    if (string.IsNullOrWhiteSpace(requestTypeName)) return false;

    string name = requestTypeName.Trim();
    if (TypesByName.TryGetValue(name, out requestType)) return true;

    // "Ns.Type+Nested, Assembly, Version=…, Culture=…, PublicKeyToken=…" → "Ns.Type+Nested, Assembly".
    // Generic names contain nested brackets and commas; they only match exactly.
    if (name.Contains('[', StringComparison.Ordinal)) return false;

    string[] segments = name.Split(',', 3, StringSplitOptions.TrimEntries);
    if (segments.Length < 2) return false;

    return TypesByName.TryGetValue($"{segments[0]}, {segments[1]}", out requestType);
  }

  internal static JavaScriptDispatchRegistry GetOrAdd(IServiceCollection serviceCollection)
  {
    JavaScriptDispatchRegistry? registry = serviceCollection
      .Where(serviceDescriptor => serviceDescriptor.ServiceType == typeof(JavaScriptDispatchRegistry))
      .Select(serviceDescriptor => serviceDescriptor.ImplementationInstance)
      .OfType<JavaScriptDispatchRegistry>()
      .FirstOrDefault();

    if (registry is not null) return registry;

    registry = new JavaScriptDispatchRegistry();
    serviceCollection.AddSingleton(registry);
    return registry;
  }

  private void AddName(string name, Type requestType)
  {
    if (TypesByName.TryGetValue(name, out Type? existing) && existing != requestType)
    {
      throw new ArgumentException($"JavaScript dispatch name '{name}' is already allowed for '{existing.FullName}'.", nameof(name));
    }

    TypesByName[name] = requestType;
  }
}
