#region Purpose
// Turns finished slots into the generated clone source.
#endregion

#region Design
// Emit writes extension methods, clone methods, module initializers, and accessor holders. Method names carry a stable FNV-1a hash so they never collide across builds. ICloneable states are rooted for trimming and registered with TryAddTransient.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

  private void MaterializeDispatches()
  {
    foreach (Slot slot in Slots.Values)
    {
      if (!slot.IsDispatch || slot.Kind != SlotKind.Clone)
      {
        continue;
      }

      string name = slot.FullyQualified;
      List<string> lines =
      [
        $"if (map.TryGet(source, out {name}? existing))",
        "{",
        "  return existing;",
        "}"
      ];

      if (slot.Exact is not null && !slot.ExactIsFallback)
      {
        lines.Add($"if (source.GetType() == typeof({name}))");
        lines.Add("{");
        lines.Add($"  return {slot.Exact.MethodName}(source, map);");
        lines.Add("}");
      }

      lines.Add("switch (source)");
      lines.Add("{");
      HashSet<string> seen = new(StringComparer.Ordinal);
      List<CaseRequest> ordered = [];
      int typedIndex = 0;
      bool cloneableCase = !slot.ExactIsFallback;
      foreach (CaseRequest caseRequest in slot.Cases)
      {
        ITypeSymbol type = Normalize(caseRequest.Type);
        if (!Slots.TryGetValue(type, out Slot? target) || target.Kind is not (SlotKind.Clone or SlotKind.Share))
        {
          continue;
        }

        // An ICloneable subtype goes to the ICloneable case, which casts its Clone() result to this slot's type.
        if (target.IsCloneable)
        {
          cloneableCase = true;
          continue;
        }

        if (!seen.Add(type.ToDisplayString(Format)))
        {
          continue;
        }

        string variable = caseRequest.Variable == "typed" ? $"typed{typedIndex++}" : caseRequest.Variable;
        int depth = type is INamedTypeSymbol named ? Depth(named) : 1;
        ordered.Add(new CaseRequest(type, variable, depth, caseRequest.Guard));
      }

      foreach (CaseRequest caseRequest in ordered
        .OrderByDescending(item => item.Depth)
        .ThenBy(item => item.Variable, StringComparer.Ordinal))
      {
        Slot target = Slots[Normalize(caseRequest.Type)];
        string display = caseRequest.Type.ToDisplayString(Format);
        string guard = caseRequest.Guard ? $" when {caseRequest.Variable}.GetType() == typeof({display})" : string.Empty;
        lines.Add($"  case {display} {caseRequest.Variable}{guard}:");
        lines.Add(target.Kind == SlotKind.Share
          ? "    return source;"
          : $"    return {target.MethodName}({caseRequest.Variable}, map);");
      }

      if (cloneableCase)
      {
        lines.Add("  case global::System.ICloneable cloneable:");
        lines.Add("  {");
        lines.Add("    object? cloned = cloneable.Clone();");
        lines.Add("    if (cloned is null)");
        lines.Add("    {");
        lines.Add("      return null!;");
        lines.Add("    }");
        lines.Add($"    {name} typedClone = ({name})cloned;");
        lines.Add("    map.Add(source, typedClone);");
        lines.Add("    return typedClone;");
        lines.Add("  }");
      }

      if (slot.Exact is not null && slot.ExactIsFallback)
      {
        lines.Add("  default:");
        lines.Add($"    return {slot.Exact.MethodName}(source, map);");
        lines.Add("}");
        slot.Statements = lines;
        continue;
      }

      lines.Add("  default:");
      if (slot.Fallback is not null)
      {
        lines.AddRange(slot.Fallback.Select(line => "  " + line));
      }
      else
      {
        string message = $"Cannot clone \" + source.GetType().FullName + \" as {slot.Type.ToDisplayString().Replace("\"", "\\\"")}: " +
          "the clone source generator did not see that type at build time (it is declared in an assembly this project " +
          "does not reference, or in a framework assembly). Implement ICloneable on it.";
        lines.Add($"    throw new global::System.InvalidOperationException(\"{message}\");");
      }

      lines.Add("}");
      slot.Statements = lines;
    }
  }

  private string? Emit()
  {
    PlanningCancellation.ThrowIfCancellationRequested();
    List<Slot> methods = Slots.Values
      .Where(slot => slot.Kind == SlotKind.Clone && slot.Statements is not null)
      .SelectMany(slot => slot.Exact is null ? [slot] : new[] { slot, slot.Exact })
      .OrderBy(slot => slot.MethodName, StringComparer.Ordinal)
      .ToList();
    List<Slot> roots = Roots.Where(slot => slot.Kind == SlotKind.Clone).Distinct().ToList();
    List<INamedTypeSymbol> iCloneableStates = NamedICloneableStates();
    if (methods.Count == 0 && roots.Count == 0 && iCloneableStates.Count == 0)
    {
      return null;
    }

    const string Host = "global::TimeWarp.State.Cloning.Generated.TimeWarpStateClones";
    StringBuilder builder = new();
    builder.AppendLine("// <auto-generated/>");
    builder.AppendLine("#nullable enable");
    builder.AppendLine("#pragma warning disable CS1591");
    builder.AppendLine("#pragma warning disable CS8600");
    builder.AppendLine("#pragma warning disable CS8601");
    builder.AppendLine("#pragma warning disable CS8602");
    builder.AppendLine("#pragma warning disable CS8603");
    builder.AppendLine("#pragma warning disable CS8604");
    builder.AppendLine("#pragma warning disable CS8625");
    builder.AppendLine();
    builder.AppendLine("namespace TimeWarp.Features.Cloning");
    builder.AppendLine("{");
    // TimeWarp.Fixie discovers every public method. Clone has parameters, so each one shows up as a skipped test.
    // The attribute type is TimeWarp.Fixie.NotTest (no Attribute suffix).
    if (Catalog.NotTest is not null || Catalog.NotTestAttribute is not null)
    {
      builder.AppendLine("  [global::TimeWarp.Fixie.NotTest]");
    }

    builder.AppendLine("  public static class GeneratedCloneExtensions");
    builder.AppendLine("  {");
    foreach (Slot root in roots)
    {
      // A public extension cannot name an internal or nested non-public type (CS0050/CS0051).
      string visibility = IsPubliclyVisible(root.Type) ? "public" : "internal";
      if (root.IsValueType)
      {
        builder.AppendLine($"    {visibility} static {root.FullyQualified} Clone(this {root.FullyQualified} source) =>");
        builder.AppendLine($"      {Host}.{root.MethodName}(source, new global::TimeWarp.Features.Cloning.CloneMap());");
      }
      else
      {
        builder.AppendLine($"    {visibility} static {root.FullyQualified}? Clone(this {root.FullyQualified}? source) =>");
        builder.AppendLine($"      source is null ? null : {Host}.{root.MethodName}(source, new global::TimeWarp.Features.Cloning.CloneMap());");
      }

      builder.AppendLine();
    }

    builder.AppendLine("  }");
    builder.AppendLine("}");
    builder.AppendLine();
    builder.AppendLine("namespace TimeWarp.State.Cloning.Generated");
    builder.AppendLine("{");
    builder.AppendLine("  file static class TimeWarpStateClones");
    builder.AppendLine("  {");
    foreach (Slot slot in methods)
    {
      PlanningCancellation.ThrowIfCancellationRequested();
      string parameters = $"{slot.FullyQualified} source, global::TimeWarp.Features.Cloning.CloneMap map";
      builder.AppendLine($"    public static {slot.FullyQualified} {slot.MethodName}({parameters})");
      builder.AppendLine("    {");
      foreach (string statement in slot.Statements!)
      {
        builder.Append("      ");
        builder.AppendLine(statement);
      }

      builder.AppendLine("    }");
      builder.AppendLine();
    }

    List<Slot> registrations = roots.Where(slot => slot.RegisterState && !slot.IsValueType).ToList();
    if (registrations.Count > 0)
    {
      builder.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
      builder.AppendLine("    internal static void Register()");
      builder.AppendLine("    {");
      foreach (Slot slot in registrations)
      {
        builder.AppendLine($"      global::TimeWarp.Features.Cloning.StateCloneRegistry.Register<{slot.FullyQualified}>(static state => {slot.MethodName}(state, new global::TimeWarp.Features.Cloning.CloneMap()));");
      }

      builder.AppendLine("    }");
    }

    List<string> serviceTypes = ServiceTypes(registrations, iCloneableStates);
    if (iCloneableStates.Count > 0)
    {
      builder.AppendLine();
      // ICloneable states keep their own Clone(). TryAddTransient<T> names the DI constructor. Clone()
      // is still an interface call, so this root keeps the public methods for the trimmer.
      builder.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
      builder.AppendLine("    internal static void RootICloneableStates()");
      builder.AppendLine("    {");
      foreach (INamedTypeSymbol type in iCloneableStates)
      {
        PlanningCancellation.ThrowIfCancellationRequested();
        if (!CanNameFromGeneratedCode(type))
        {
          continue;
        }

        builder.AppendLine($"      RootStateForTrim<{type.ToDisplayString(Format)}>();");
      }

      builder.AppendLine("    }");
      builder.AppendLine();
      builder.AppendLine("    private static void RootStateForTrim<");
      builder.AppendLine("      [global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(");
      builder.AppendLine("        global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors");
      builder.AppendLine("        | global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicMethods)] T>()");
      builder.AppendLine("      where T : class");
      builder.AppendLine("    {");
      builder.AppendLine("    }");
    }

    if (serviceTypes.Count > 0)
    {
      builder.AppendLine();
      builder.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
      builder.AppendLine("    internal static void RegisterServices()");
      builder.AppendLine("    {");
      builder.AppendLine("      global::TimeWarp.State.StateRegistrationRegistry.Register");
      builder.AppendLine("      (");
      builder.AppendLine("        typeof(RegistrationAnchor).Assembly,");
      builder.AppendLine("        static services =>");
      builder.AppendLine("        {");
      foreach (string serviceType in serviceTypes)
      {
        builder.AppendLine($"          global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddTransient<{serviceType}>(services);");
      }

      builder.AppendLine("        }");
      builder.AppendLine("      );");
      builder.AppendLine("    }");
      builder.AppendLine();
      builder.AppendLine("    private sealed class RegistrationAnchor;");
    }

    builder.AppendLine("  }");
    foreach (Holder holder in Holders.Values
      .Where(holder => holder.Problem is null && holder.Members.Count > 0)
      .OrderBy(holder => holder.Name, StringComparer.Ordinal))
    {
      builder.AppendLine();
      builder.AppendLine($"  file static class {holder.Name}{holder.TypeParameters}");
      foreach (string constraint in holder.Constraints)
      {
        builder.AppendLine($"    {constraint}");
      }

      builder.AppendLine("  {");
      foreach (string member in holder.Members)
      {
        builder.AppendLine($"    {member}");
      }

      builder.AppendLine("  }");
    }

    builder.AppendLine("}");
    return builder.ToString();
  }

  private List<string> ServiceTypes(List<Slot> registrations, List<INamedTypeSymbol> iCloneableStates)
  {
    List<string> serviceTypes = [];
    foreach (Slot slot in registrations)
    {
      if (slot.Kind != SlotKind.Clone || slot.Type is not INamedTypeSymbol named || !CanNameFromGeneratedCode(named))
      {
        continue;
      }

      serviceTypes.Add(slot.FullyQualified);
    }

    foreach (INamedTypeSymbol type in iCloneableStates)
    {
      if (!CanNameFromGeneratedCode(type))
      {
        continue;
      }

      serviceTypes.Add(type.ToDisplayString(Format));
    }

    return serviceTypes;
  }

  private static bool IsPubliclyVisible(ITypeSymbol type)
  {
    switch (type)
    {
      case IArrayTypeSymbol array:
        return IsPubliclyVisible(array.ElementType);
      case IPointerTypeSymbol pointer:
        return IsPubliclyVisible(pointer.PointedAtType);
      case INamedTypeSymbol named:
        foreach (ITypeSymbol argument in named.TypeArguments)
        {
          if (!IsPubliclyVisible(argument))
          {
            return false;
          }
        }

        break;
    }

    for (ISymbol? symbol = type; symbol is not null and not INamespaceSymbol; symbol = symbol.ContainingSymbol)
    {
      if (symbol is INamedTypeSymbol { IsFileLocal: true })
      {
        return false;
      }

      if (symbol.DeclaredAccessibility != Accessibility.Public)
      {
        return false;
      }
    }

    return true;
  }

  private void RememberICloneableState(INamedTypeSymbol type)
  {
    if (type.IsGenericType && type.IsDefinition)
    {
      return;
    }

    if (!CanNameFromGeneratedCode(type))
    {
      return;
    }

    if (!ICloneableStateSet.Add(type))
    {
      return;
    }

    ICloneableStates.Add(type);
  }

  private List<INamedTypeSymbol> NamedICloneableStates() =>
    ICloneableStates
      .OrderBy(type => type.ToDisplayString(Format), StringComparer.Ordinal)
      .ToList();

  // File-local types, private, protected, and private protected types, internals from another assembly, and type
  // arguments of those shapes cannot appear in the generated file. CanNameDeep is the same check the field cloner uses.
  private bool CanNameFromGeneratedCode(ITypeSymbol type) => CanNameDeep(type);

  private string UniqueName(string prefix, string identity)
  {
    string trimmed = prefix.Length > 80 ? prefix.Substring(0, 80) : prefix;
    string name = $"{trimmed}_{StableHash(identity)}";
    string candidate = name;
    int suffix = 1;
    while (!UsedNames.Add(candidate))
    {
      candidate = $"{name}_{suffix++}";
    }

    return candidate;
  }

  // FNV-1a over the fully qualified name: stable across builds and machines, unlike string.GetHashCode.
  private static string StableHash(string text)
  {
    uint hash = 2166136261;
    foreach (char character in text)
    {
      hash ^= character;
      hash *= 16777619;
    }

    return hash.ToString("x8");
  }

  private static string Sanitize(string name)
  {
    StringBuilder builder = new(name.Length);
    foreach (char character in name)
    {
      builder.Append(char.IsLetterOrDigit(character) ? character : '_');
    }

    return builder.ToString();
  }
}

