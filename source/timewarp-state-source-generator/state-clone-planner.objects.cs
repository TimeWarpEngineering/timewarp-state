#region Purpose
// Clones ordinary objects by construction plus field writes.
#endregion

#region Design
// A value type with nothing to copy is shared. CloneShared members are assigned by reference and are not classified. Ignored members keep the constructor value. Metadata types are checked before any field is copied.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

  private void ClassifyInterface(Slot slot, INamedTypeSymbol type)
  {
    if (FindHiddenSubtype(type, implementations: true) is { } hidden)
    {
      Fail(slot, HiddenSubtypeDetail(hidden), LocationOf(hidden));
      return;
    }

    List<INamedTypeSymbol> implementations = FindImplementations(type);
    if (implementations.Count == 0 && !AssignableTo(type, "System.ICloneable"))
    {
      Fail(slot, $"interface or abstract type '{type.ToDisplayString()}' has no implementation the generator can see", LocationOf(type));
      return;
    }

    slot.Kind = SlotKind.Clone;
    slot.IsDispatch = true;
    foreach (INamedTypeSymbol implementation in implementations)
    {
      AddCase(slot, implementation, "typed", required: true, guard: false);
    }
  }

  private void ClassifyObject(Slot slot, INamedTypeSymbol type)
  {
    List<string> lines = [];
    string name = type.ToDisplayString(Format);
    if (!type.IsValueType)
    {
      lines.Add($"if (map.TryGet(source, out {name}? existing))");
      lines.Add("{");
      lines.Add("  return existing;");
      lines.Add("}");
      if (!TryEmitConstruction(type, lines, out string? problem))
      {
        Fail(slot, problem!, LocationOf(type));
        return;
      }

      lines.Add("map.Add(source, clone);");
    }
    else
    {
      lines.Add($"{name} clone = source;");
    }

    bool copied = AppendExtraFields(slot, type, lines, stopBefore: null);
    if (slot.Kind == SlotKind.Error)
    {
      return;
    }

    if (type.IsValueType && !copied)
    {
      slot.Kind = SlotKind.Share;
      slot.Statements = null;
      return;
    }

    slot.Kind = SlotKind.Clone;
    lines.Add("return clone;");
    slot.Statements = lines;
  }

  private bool AppendExtraFields(Slot slot, INamedTypeSymbol type, List<string> lines, INamedTypeSymbol? stopBefore)
  {
    foreach (INamedTypeSymbol current in EnumerateHierarchy(type, stopBefore))
    {
      if (IsFromSource(current))
      {
        continue;
      }

      string? problem = MetadataProblem(current);
      if (problem is not null)
      {
        Fail(slot, problem, SourceLocation(type));
        return false;
      }
    }

    bool copied = false;
    HashSet<ISymbol> copiedMembers = new(SymbolEqualityComparer.Default);
    foreach (IFieldSymbol field in EnumerateFields(type, stopBefore))
    {
      if (field.AssociatedSymbol is not null)
      {
        copiedMembers.Add(field.AssociatedSymbol);
      }

      bool shared = IsCloneShared(field);
      if (!shared && IsIgnored(field))
      {
        continue;
      }

      CloneMember member = new(field.Type, field.OriginalDefinition.Type, field.Name, field.ContainingType, FieldLocation(field));
      if (!AppendMember(slot, type, lines, member, shared, ref copied))
      {
        return copied;
      }
    }

    // Referenced assemblies do not expose <Prop>k__BackingField through GetMembers. The getter is still
    // CompilerGenerated, and the field name is fixed, so copy those properties the same way.
    foreach ((IPropertySymbol property, INamedTypeSymbol declaringType) in EnumerateAutoProperties(type, stopBefore))
    {
      if (!copiedMembers.Add(property))
      {
        continue;
      }

      bool shared = HasCloneSharedAttribute(property);
      if (!shared && HasIgnoreAttribute(property))
      {
        continue;
      }

      Location? location = property.Locations.FirstOrDefault(candidate => candidate.IsInSource);
      CloneMember member = new(property.Type, property.OriginalDefinition.Type, $"<{property.Name}>k__BackingField", declaringType, location);
      if (!AppendMember(slot, type, lines, member, shared, ref copied))
      {
        return copied;
      }
    }

    return copied;
  }

  private bool AppendMember(Slot slot, INamedTypeSymbol type, List<string> lines, CloneMember member, bool shared, ref bool copied) =>
    shared
      ? TryAppendSharedMember(slot, type, lines, member, ref copied)
      : TryAppendMember(slot, type, lines, member, ref copied);

  private bool TryAppendMember(Slot slot, INamedTypeSymbol type, List<string> lines, CloneMember member, ref bool copied)
  {
    Location? location = member.Location is { IsInSource: true } ? member.Location : SourceLocation(type);
    Slot fieldSlot = Build(member.Type);
    slot.Dependencies.Add(fieldSlot);
    if (fieldSlot.Kind == SlotKind.Error)
    {
      Fail(slot, $"member '{DisplayMemberName(member.FieldName)}' of type '{member.Type.ToDisplayString()}' cannot be cloned ({fieldSlot.Detail})", location, fieldSlot, member: true);
      return false;
    }

    if (type.IsValueType && fieldSlot.Kind == SlotKind.Share)
    {
      return true;
    }

    string? accessor = FieldAccessor(member.DeclaringType, member.FieldName, member.DefinitionType, out string? problem);
    if (accessor is null)
    {
      Fail(slot, $"member '{DisplayMemberName(member.FieldName)}' cannot be written ({problem})", location, member: true);
      return false;
    }

    string read = type.IsValueType ? $"{accessor}(ref source)" : $"{accessor}(source)";
    string write = type.IsValueType ? $"{accessor}(ref clone)" : $"{accessor}(clone)";
    lines.Add($"{write} = {CopyExpression(fieldSlot, read)};");
    copied = true;
    return true;
  }

  // Copy the reference (or the whole value) without classifying the member type. Classification is what rejects
  // an interface with no visible implementation and a framework type whose private fields cannot be seen.
  private bool TryAppendSharedMember(Slot slot, INamedTypeSymbol type, List<string> lines, CloneMember member, ref bool copied)
  {
    Location? location = member.Location is { IsInSource: true } ? member.Location : SourceLocation(type);
    string? accessor = FieldAccessor(member.DeclaringType, member.FieldName, member.DefinitionType, out string? problem);
    if (accessor is null)
    {
      Fail(slot, $"member '{DisplayMemberName(member.FieldName)}' cannot be written ({problem})", location, member: true);
      return false;
    }

    string read = type.IsValueType ? $"{accessor}(ref source)" : $"{accessor}(source)";
    string write = type.IsValueType ? $"{accessor}(ref clone)" : $"{accessor}(clone)";
    lines.Add($"{write} = {read};");
    copied = true;
    return true;
  }

  private static string DisplayMemberName(string fieldName)
  {
    int end = fieldName.IndexOf('>');
    return fieldName.Length > 0 && fieldName[0] == '<' && end > 1 ? fieldName.Substring(1, end - 1) : fieldName;
  }

}
