#region Purpose
// Decides whether a type from another assembly can be cloned field by field.
#endregion

#region Design
// TimeWarp.State assemblies are trusted. An implementation assembly is re-imported with MetadataImportOptions.All. A reference assembly is accepted only when every instance property is an auto-property and every instance method is a constructor, an accessor, or compiler-generated.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

  private bool IsFromSource(INamedTypeSymbol type) =>
    SymbolEqualityComparer.Default.Equals(type.OriginalDefinition.ContainingAssembly, Compilation.Assembly);

  // A type from another assembly is cloned field by field only when its full field list is known. TimeWarp.State's
  // own bases are trusted. An implementation assembly is re-imported with private members to check for hidden state.
  // A reference assembly strips private class fields, and only keeps placeholders for struct fields, so its classes
  // and managed structs are accepted only when nothing visible could hold hidden state (ReferenceAssemblyProblem:
  // auto-properties, constructors, accessors and compiler-generated record members); the rest are TWSG002.
  private string? MetadataProblem(INamedTypeSymbol type)
  {
    INamedTypeSymbol definition = type.OriginalDefinition;
    if (MetadataProblems.TryGetValue(definition, out string? cached))
    {
      return cached;
    }

    string? problem = FindMetadataProblem(definition);
    MetadataProblems.Add(definition, problem);
    return problem;
  }

  private string? FindMetadataProblem(INamedTypeSymbol definition)
  {
    IAssemblySymbol assembly = definition.ContainingAssembly;
    if (IsTimeWarpStateAssembly(assembly))
    {
      return null;
    }

    if (StateCloneTypeCatalog.HasReferenceAssemblyAttribute(assembly))
    {
      return ReferenceAssemblyProblem(definition, assembly);
    }

    INamedTypeSymbol? full = FullView(definition);
    if (full is null)
    {
      return $"the fields of '{definition.ToDisplayString()}' in '{assembly.Name}' could not be inspected";
    }

    HashSet<string> visibleFields = new(
      definition.GetMembers().OfType<IFieldSymbol>().Select(field => field.Name),
      StringComparer.Ordinal);
    HashSet<string> events = new(full.GetMembers().OfType<IEventSymbol>().Select(item => item.Name), StringComparer.Ordinal);
    foreach (IFieldSymbol field in full.GetMembers().OfType<IFieldSymbol>())
    {
      if (field.IsStatic || field.IsConst || visibleFields.Contains(field.Name) || events.Contains(field.Name))
      {
        continue;
      }

      string propertyName = DisplayMemberName(field.Name);
      IPropertySymbol? hidden = propertyName != field.Name
        ? full.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault()
        : null;

      // The generated clone never enumerates a hidden field, so CloneShared on it cannot be honored. Report that
      // instead of dropping it silently (CloneShared wins over an ignore attribute) or advising CloneShared again.
      if (HasCloneSharedAttribute(field) || (hidden is not null && HasCloneSharedAttribute(hidden)))
      {
        return $"'{definition.ToDisplayString()}' has private member '{propertyName}' marked [CloneShared] that generated code cannot see";
      }

      if (HasIgnoreAttribute(field))
      {
        continue;
      }

      if (propertyName != field.Name)
      {
        IPropertySymbol? property = definition.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();
        if (property is not null && IsAutoProperty(property))
        {
          continue;
        }

        if (hidden is not null && HasIgnoreAttribute(hidden))
        {
          continue;
        }
      }

      return $"'{definition.ToDisplayString()}' has private field '{field.Name}' that generated code cannot see";
    }

    return null;
  }

  // TimeWarp.State's own bases (State<T>, TimeWarpCacheableState<T>, ...) keep only ignored private state.
  private static readonly HashSet<string> TimeWarpStateAssemblies = new(StringComparer.Ordinal)
  {
    "TimeWarp.State",
    "TimeWarp.State.Blazor",
    "TimeWarp.State.Plus",
    "TimeWarp.State.Policies",
    "TimeWarp.State.Telemetry",
    "timewarp-state-plus"
  };

  private static bool IsTimeWarpStateAssembly(IAssemblySymbol assembly) => TimeWarpStateAssemblies.Contains(assembly.Name);

  // A reference assembly (a project reference, or a framework reference pack) hides private class fields. A type is
  // accepted only when nothing visible could hold or use hidden state: every instance property is an auto-property and
  // every instance method is a constructor, an accessor, or compiler-generated (records).
  private static string? ReferenceAssemblyProblem(INamedTypeSymbol definition, IAssemblySymbol assembly)
  {
    string? offending = null;
    foreach (ISymbol member in definition.GetMembers())
    {
      if (member.IsStatic || member.IsImplicitlyDeclared)
      {
        continue;
      }

      // Synthesized record EqualityContract has a compiler-generated getter and no backing field.
      if (member is IPropertySymbol equality && IsSynthesizedEqualityContract(equality))
      {
        continue;
      }

      if (member is IPropertySymbol property && !IsAutoProperty(property))
      {
        offending = $"property '{property.Name}' is not an auto-property";
        break;
      }

      if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.ExplicitInterfaceImplementation or MethodKind.Destructor } method
          && !HasCompilerGenerated(method))
      {
        offending = $"method '{method.Name}' may use them";
        break;
      }
    }

    if (offending is null)
    {
      return null;
    }

    string problem = $"'{definition.ToDisplayString()}' comes from reference assembly '{assembly.Name}', which hides private fields, " +
      $"and {offending}";
    return IsFrameworkAssembly(assembly)
      ? problem
      : problem + $" (build '{assembly.Name}' with ProduceReferenceAssembly=false so the generator can inspect its fields)";
  }

  private INamedTypeSymbol? FullView(INamedTypeSymbol definition)
  {
    if (Compilation.GetMetadataReference(definition.ContainingAssembly) is not { } reference)
    {
      return null;
    }

    FullCompilation ??= Compilation.WithOptions(Compilation.Options.WithMetadataImportOptions(MetadataImportOptions.All));
    if (FullCompilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
    {
      return null;
    }

    return assembly.GetTypeByMetadataName(MetadataName(definition));
  }

  private static string MetadataName(INamedTypeSymbol type)
  {
    if (type.ContainingType is { } containing)
    {
      return MetadataName(containing) + "+" + type.MetadataName;
    }

    return type.ContainingNamespace is null || type.ContainingNamespace.IsGlobalNamespace
      ? type.MetadataName
      : type.ContainingNamespace.ToDisplayString() + "." + type.MetadataName;
  }

}
