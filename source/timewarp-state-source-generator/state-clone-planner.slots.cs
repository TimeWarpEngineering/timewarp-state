#region Purpose
// Builds one slot per normalized type and classifies it.
#endregion

#region Design
// Normalize drops tuple underlying types and nullable annotations so one slot serves both. Classification order is shared, pointer, array, ICloneable, nullable, KeyValuePair, ValueTuple, StringBuilder, collection, interface, unmanaged, then object. A non-sealed class wraps its clone in a runtime-type dispatch.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

  private Slot Build(ITypeSymbol type)
  {
    type = Normalize(type);
    if (Slots.TryGetValue(type, out Slot? existing))
    {
      return existing;
    }

    Slot slot = GetOrCreate(type);
    Classify(slot, type);
    if (slot.Kind == SlotKind.Clone && !slot.IsDispatch && !slot.IsCloneable && type is INamedTypeSymbol named)
    {
      WrapForRuntimeType(slot, named);
    }

    return slot;
  }

  private static ITypeSymbol Normalize(ITypeSymbol type)
  {
    if (type is INamedTypeSymbol { IsTupleType: true, TupleUnderlyingType: { } underlying })
    {
      type = underlying;
    }

    return type.WithNullableAnnotation(NullableAnnotation.None);
  }

  private Slot GetOrCreate(ITypeSymbol type)
  {
    type = Normalize(type);
    if (Slots.TryGetValue(type, out Slot? existing))
    {
      return existing;
    }

    string fullyQualified = type.ToDisplayString(Format);
    Slot slot = new()
    {
      Type = type,
      FullyQualified = fullyQualified,
      MethodName = UniqueName("Clone_" + Sanitize(type.ToDisplayString()), fullyQualified),
      IsValueType = type.IsValueType,
      Kind = SlotKind.Pending
    };
    Slots.Add(type, slot);
    return slot;
  }

  private void Classify(Slot slot, ITypeSymbol type)
  {
    if (type is IErrorTypeSymbol || type.TypeKind == TypeKind.Error)
    {
      Fail(slot, "the type could not be resolved", LocationOf(type));
      return;
    }

    if (type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer)
    {
      Fail(slot, "pointer types are not supported", LocationOf(type));
      return;
    }

    if (IsShared(type))
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    if (type is IArrayTypeSymbol array)
    {
      ClassifyArray(slot, array);
      return;
    }

    if (type is not INamedTypeSymbol named)
    {
      Fail(slot, $"type '{type.ToDisplayString()}' is not supported", LocationOf(type));
      return;
    }

    if (!CanNameDeep(named))
    {
      Fail(slot, $"'{named.ToDisplayString()}' is not accessible to generated code", LocationOf(named));
      return;
    }

    if (ImplementsICloneable(named))
    {
      ClassifyCloneable(slot, named);
      return;
    }

    if (named.SpecialType == SpecialType.System_Object)
    {
      Fail(slot, "System.Object can hold any runtime value", LocationOf(named));
      return;
    }

    if (named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
    {
      ClassifyNullable(slot, named);
      return;
    }

    if (SymbolEquals(named.OriginalDefinition, "System.Collections.Generic.KeyValuePair`2"))
    {
      ClassifyKeyValuePair(slot, named);
      return;
    }

    if (IsValueTuple(named))
    {
      ClassifyTuple(slot, named);
      return;
    }

    if (SymbolEquals(named, "System.Text.StringBuilder"))
    {
      ClassifyStringBuilder(slot, named);
      return;
    }

    CollectionKind collectionKind = CollectionKindOf(named);
    if (collectionKind != CollectionKind.None)
    {
      ClassifyCollection(slot, named, collectionKind);
      return;
    }

    if (named.TypeKind == TypeKind.Interface || named.IsAbstract)
    {
      ClassifyInterface(slot, named);
      return;
    }

    if (named.IsRefLikeType)
    {
      Fail(slot, "ref structs are not supported", LocationOf(named));
      return;
    }

    if (named.IsValueType && named.IsUnmanagedType)
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    ClassifyObject(slot, named);
  }

  // A non-sealed class can hold a subtype at run time. The old reflection cloner cloned by runtime type, so the
  // generated cloner dispatches: known derived types, then the exact cloner. A known BCL collection keeps
  // materializing unknown subclasses into itself; any other known subtype the switch cannot name is TWSG002, and a
  // subtype the generator never saw throws.
  private void WrapForRuntimeType(Slot slot, INamedTypeSymbol type)
  {
    if (type.TypeKind != TypeKind.Class || type.IsSealed || type.IsAbstract || type.IsStatic)
    {
      return;
    }

    bool knownCollection = MatchConcrete(type) != CollectionKind.None;
    if (!knownCollection && FindHiddenSubtype(type, implementations: false) is { } hidden)
    {
      Fail(slot, HiddenSubtypeDetail(hidden), LocationOf(hidden));
      return;
    }

    List<INamedTypeSymbol> derived = FindDerived(type);
    if (knownCollection && derived.Count == 0)
    {
      return;
    }

    string exactName = slot.MethodName + "_Exact";
    UsedNames.Add(exactName);
    slot.Exact = new Slot
    {
      Type = type,
      FullyQualified = slot.FullyQualified,
      MethodName = exactName,
      Kind = SlotKind.Clone,
      Statements = slot.Statements
    };
    slot.ExactIsFallback = knownCollection;
    slot.IsDispatch = true;
    slot.Statements = null;
    foreach (INamedTypeSymbol subtype in derived)
    {
      AddCase(slot, subtype, "typed", required: true, guard: false);
    }
  }

}
