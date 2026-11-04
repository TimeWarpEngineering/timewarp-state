#region Purpose
// Symbol predicates the planner shares: naming, attributes, and shared types.
#endregion

#region Design
// Metadata names go through StateCloneTypeCatalog, which resolves each name once per compilation.
// Namespace checks compare INamespaceSymbol values from that catalog. Collection kind matching lives in its own partial.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{
  private bool CanName(ISymbol symbol)
  {
    for (ISymbol? current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingSymbol)
    {
      // A file-local type is visible only in its own file, never in the generated one.
      if (current is INamedTypeSymbol { IsFileLocal: true })
      {
        return false;
      }

      // private protected is visible only to the container and its derived types. The generated file is neither.
      // protected internal is visible to the rest of this assembly, so it stays in the assembly check below.
      if (current.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal)
      {
        return false;
      }

      if (current.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal
          && !SymbolEqualityComparer.Default.Equals(current.ContainingAssembly, Compilation.Assembly))
      {
        return false;
      }
    }

    return true;
  }

  private bool CanNameDeep(ITypeSymbol type) =>
    type switch
    {
      ITypeParameterSymbol => true,
      IArrayTypeSymbol array => CanNameDeep(array.ElementType),
      IPointerTypeSymbol pointer => CanNameDeep(pointer.PointedAtType),
      INamedTypeSymbol named => CanName(named) && AllTypeArguments(named).All(CanNameDeep),
      _ => CanName(type)
    };

  private IEnumerable<(IPropertySymbol Property, INamedTypeSymbol DeclaringType)> EnumerateAutoProperties(
    INamedTypeSymbol type,
    INamedTypeSymbol? stopBefore)
  {
    foreach (INamedTypeSymbol current in EnumerateHierarchy(type, stopBefore))
    {
      foreach (IPropertySymbol property in current.GetMembers().OfType<IPropertySymbol>())
      {
        if (IsAutoProperty(property))
        {
          yield return (property, current);
        }
      }
    }
  }

  private static bool IsAutoProperty(IPropertySymbol property)
  {
    if (property.IsStatic || property.Parameters.Length > 0 || property.IsImplicitlyDeclared || IsSynthesizedEqualityContract(property))
    {
      return false;
    }

    return HasCompilerGenerated(property.GetMethod) || HasCompilerGenerated(property.SetMethod);
  }

  // Reference assemblies keep EqualityContract as a compiler-generated get-only property with no backing field.
  // Copying <EqualityContract>k__BackingField throws at run time. A source compilation marks the same property
  // implicit, which IsAutoProperty already rejects.
  private static bool IsSynthesizedEqualityContract(IPropertySymbol property) =>
    property.Name == "EqualityContract"
    && property.ContainingType.IsRecord
    && property.SetMethod is null
    && property.DeclaredAccessibility is Accessibility.Protected or Accessibility.Private
    && property.Type.ToDisplayString() == "System.Type"
    && (property.IsImplicitlyDeclared || HasCompilerGenerated(property.GetMethod));

  private static bool HasCompilerGenerated(IMethodSymbol? method) =>
    method?.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "CompilerGeneratedAttribute") == true;

  private static IEnumerable<INamedTypeSymbol> EnumerateHierarchy(INamedTypeSymbol type, INamedTypeSymbol? stopBefore)
  {
    for (INamedTypeSymbol? current = type; current is not null && current.SpecialType is not (SpecialType.System_Object or SpecialType.System_ValueType); current = current.BaseType)
    {
      if (stopBefore is not null && SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, stopBefore))
      {
        yield break;
      }

      yield return current;
    }
  }

  private IEnumerable<IFieldSymbol> EnumerateFields(INamedTypeSymbol type, INamedTypeSymbol? stopBefore)
  {
    foreach (INamedTypeSymbol current in EnumerateHierarchy(type, stopBefore))
    {
      foreach (ISymbol member in current.GetMembers())
      {
        // Implicit fields are auto-property and field-like event backing fields. DeepCloner copies those.
        // Ignore attributes on the associated property or event still drop the field in IsIgnored.
        // Metadata assemblies omit those fields from GetMembers; EnumerateAutoProperties covers that case.
        if (member is IFieldSymbol field && !field.IsStatic && !field.IsConst)
        {
          yield return field;
        }
      }
    }
  }

  private bool IsIgnored(IFieldSymbol field)
  {
    if (HasIgnoreAttribute(field))
    {
      return true;
    }

    return field.AssociatedSymbol is not null && HasIgnoreAttribute(field.AssociatedSymbol);
  }

  private static bool IsCloneShared(IFieldSymbol field)
  {
    if (HasCloneSharedAttribute(field))
    {
      return true;
    }

    return field.AssociatedSymbol is not null && HasCloneSharedAttribute(field.AssociatedSymbol);
  }

  private bool HasIgnoreAttribute(ISymbol symbol)
  {
    foreach (AttributeData attribute in symbol.GetAttributes())
    {
      if (attribute.AttributeClass?.Name is "IgnoreDataMemberAttribute" or "NonSerializedAttribute")
      {
        return true;
      }

      if (attribute.AttributeClass?.Name is "JsonIgnoreAttribute" && JsonIgnoreSkipsMember(attribute))
      {
        return true;
      }
    }

    return false;
  }

  // Parameterless [JsonIgnore] and JsonIgnoreCondition.Always skip the member. WhenWritingNull, WhenWritingDefault
  // and Never still clone: those conditions only affect the serializer.
  private bool JsonIgnoreSkipsMember(AttributeData attribute)
  {
    int? condition = null;
    foreach (KeyValuePair<string, TypedConstant> named in attribute.NamedArguments)
    {
      if (named.Key == "Condition" && named.Value.Value is int namedValue)
      {
        condition = namedValue;
      }
    }

    if (condition is null && attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0].Value is int constructorValue)
    {
      condition = constructorValue;
    }

    if (condition is null)
    {
      return true;
    }

    return condition.Value == JsonIgnoreAlwaysValue();
  }

  private int JsonIgnoreAlwaysValue()
  {
    if (!JsonIgnoreAlwaysResolved)
    {
      JsonIgnoreAlwaysResolved = true;
      INamedTypeSymbol? enumType = Catalog.JsonIgnoreCondition;
      IFieldSymbol? always = enumType?.GetMembers("Always").OfType<IFieldSymbol>().FirstOrDefault();
      if (always?.HasConstantValue == true && always.ConstantValue is int value)
      {
        JsonIgnoreAlways = value;
      }
    }

    return JsonIgnoreAlways ?? 1;
  }

  private static bool HasCloneSharedAttribute(ISymbol symbol)
  {
    foreach (AttributeData attribute in symbol.GetAttributes())
    {
      if (attribute.AttributeClass?.Name is "CloneShared" or "CloneSharedAttribute")
      {
        return true;
      }
    }

    return false;
  }

  private bool IsShared(ITypeSymbol type)
  {
    if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
    {
      return IsShared(nullable.TypeArguments[0]);
    }

    if (type.TypeKind is TypeKind.Enum or TypeKind.Delegate)
    {
      return true;
    }

    if (type.SpecialType is SpecialType.System_Boolean
        or SpecialType.System_Byte
        or SpecialType.System_SByte
        or SpecialType.System_Int16
        or SpecialType.System_UInt16
        or SpecialType.System_Int32
        or SpecialType.System_UInt32
        or SpecialType.System_Int64
        or SpecialType.System_UInt64
        or SpecialType.System_Single
        or SpecialType.System_Double
        or SpecialType.System_Char
        or SpecialType.System_String
        or SpecialType.System_Decimal
        or SpecialType.System_DateTime
        or SpecialType.System_IntPtr
        or SpecialType.System_UIntPtr)
    {
      return true;
    }

    foreach (KeyValuePair<string, INamedTypeSymbol?> named in Catalog.NamedTypes)
    {
      if (named.Value is not null && SymbolEqualityComparer.Default.Equals(type, named.Value))
      {
        return true;
      }
    }

    foreach (INamedTypeSymbol? assignable in Catalog.AssignableTypes)
    {
      if (assignable is not null && AssignableTo(type, assignable))
      {
        return true;
      }
    }

    // Synchronization primitives, timers, tasks, cancellation tokens and sources are identity-bound.
    if (Catalog.InThreadingNamespace(type))
    {
      return true;
    }

    return IsComparer(type);
  }

  private bool IsComparer(ITypeSymbol type)
  {
    if (type.IsValueType)
    {
      return false;
    }

    if (AssignableTo(type, Catalog.NonGenericEqualityComparer) || AssignableTo(type, Catalog.NonGenericComparer))
    {
      return true;
    }

    if (type is INamedTypeSymbol named)
    {
      if (SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, Catalog.GenericEqualityComparer)
          || SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, Catalog.GenericComparer))
      {
        return true;
      }
    }

    foreach (INamedTypeSymbol implemented in type.AllInterfaces)
    {
      if (SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, Catalog.GenericEqualityComparer)
          || SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, Catalog.GenericComparer))
      {
        return true;
      }
    }

    return false;
  }

  private bool AssignableTo(ITypeSymbol type, string metadataName) =>
    AssignableTo(type, Catalog.Resolve(metadataName));

  private static bool AssignableTo(ITypeSymbol type, INamedTypeSymbol? target)
  {
    if (target is null)
    {
      return false;
    }

    if (SymbolEqualityComparer.Default.Equals(type, target) || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, target))
    {
      return true;
    }

    foreach (INamedTypeSymbol implemented in type.AllInterfaces)
    {
      if (SymbolEqualityComparer.Default.Equals(implemented, target)
          || SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, target))
      {
        return true;
      }
    }

    for (ITypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current, target) || SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, target))
      {
        return true;
      }
    }

    return false;
  }

  private bool IsState(INamedTypeSymbol type)
  {
    if (StateType is null)
    {
      return false;
    }

    for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, StateType))
      {
        return true;
      }
    }

    return false;
  }

  private bool ImplementsICloneable(ITypeSymbol type) => Catalog.ImplementsICloneable(type);

  private static bool HasGenerateClone(ITypeSymbol type) =>
    type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name is "GenerateClone" or "GenerateCloneAttribute");

  private static string CopyExpression(Slot slot, string sourceExpression)
  {
    if (slot.Kind == SlotKind.Share)
    {
      return sourceExpression;
    }

    if (slot.IsValueType)
    {
      return $"{slot.MethodName}({sourceExpression}, map)";
    }

    return $"{sourceExpression} is null ? null! : {slot.MethodName}({sourceExpression}, map)";
  }
}
