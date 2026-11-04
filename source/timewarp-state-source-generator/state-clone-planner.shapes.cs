#region Purpose
// Classifies arrays, ICloneable, nullable values, KeyValuePair, ValueTuple, and StringBuilder.
#endregion

#region Design
// Arrays copy elementwise and remember the clone in the map. ICloneable calls Clone and casts back to the declared type, so it is never wrapped in a runtime-type dispatch. A ValueTuple whose elements are all shared is itself shared.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

  private void ClassifyArray(Slot slot, IArrayTypeSymbol array)
  {
    Slot element = Build(array.ElementType);
    slot.Dependencies.Add(element);
    if (element.Kind == SlotKind.Error)
    {
      Fail(slot, $"array element '{array.ElementType.ToDisplayString()}' cannot be cloned", LocationOf(array), element);
      return;
    }

    slot.Kind = SlotKind.Clone;
    string arrayName = array.ToDisplayString(Format);
    int rank = array.Rank;
    List<string> lines = [];
    lines.Add($"if (map.TryGet(source, out {arrayName}? existing))");
    lines.Add("{");
    lines.Add("  return existing;");
    lines.Add("}");
    for (int dimension = 0; dimension < rank; dimension++)
    {
      lines.Add($"int length{dimension} = source.GetLength({dimension});");
    }

    string lengths = string.Join(", ", Enumerable.Range(0, rank).Select(dimension => $"length{dimension}"));
    lines.Add($"{arrayName} clone = {NewArrayExpression(array, lengths)};");
    lines.Add("map.Add(source, clone);");
    if (element.Kind == SlotKind.Share)
    {
      lines.Add("global::System.Array.Copy(source, clone, source.Length);");
    }
    else
    {
      for (int dimension = 0; dimension < rank; dimension++)
      {
        lines.Add($"{new string(' ', dimension * 2)}for (int index{dimension} = 0; index{dimension} < length{dimension}; index{dimension}++)");
        lines.Add($"{new string(' ', dimension * 2)}{{");
      }

      string indexes = string.Join(", ", Enumerable.Range(0, rank).Select(dimension => $"index{dimension}"));
      string indent = new(' ', rank * 2);
      lines.Add($"{indent}clone[{indexes}] = {CopyExpression(element, $"source[{indexes}]")};");
      for (int dimension = rank - 1; dimension >= 0; dimension--)
      {
        lines.Add($"{new string(' ', dimension * 2)}}}");
      }
    }

    lines.Add("return clone;");
    slot.Statements = lines;
  }

  // new T[n] for a jagged element type T = U[] must be written new U[n][], not new U[][n].
  private static string NewArrayExpression(IArrayTypeSymbol array, string lengths)
  {
    ITypeSymbol element = array.ElementType;
    StringBuilder suffix = new();
    while (element is IArrayTypeSymbol inner)
    {
      suffix.Append('[').Append(new string(',', inner.Rank - 1)).Append(']');
      element = inner.ElementType;
    }

    return $"new {element.ToDisplayString(Format)}[{lengths}]{suffix}";
  }

  // ICloneable.Clone() is the type's own virtual dispatch, so the slot is never wrapped for the runtime type, and the
  // result is cast to the declared type: Clone() may return the base type for a subclass that does not override it.
  private void ClassifyCloneable(Slot slot, INamedTypeSymbol type)
  {
    slot.Kind = SlotKind.Clone;
    slot.IsCloneable = true;
    string name = type.ToDisplayString(Format);
    if (type.IsValueType)
    {
      slot.Statements =
      [
        $"return ({name})((global::System.ICloneable)source).Clone();"
      ];
      return;
    }

    slot.Statements =
    [
      $"if (map.TryGet(source, out {name}? existing))",
      "{",
      "  return existing;",
      "}",
      "object? cloned = ((global::System.ICloneable)source).Clone();",
      "if (cloned is null)",
      "{",
      "  return null!;",
      "}",
      $"{name} clone = ({name})cloned;",
      "map.Add(source, clone);",
      "return clone;"
    ];
  }

  private void ClassifyNullable(Slot slot, INamedTypeSymbol type)
  {
    Slot inner = Build(type.TypeArguments[0]);
    slot.Dependencies.Add(inner);
    if (inner.Kind == SlotKind.Error)
    {
      Fail(slot, $"'{type.TypeArguments[0].ToDisplayString()}' cannot be cloned", LocationOf(type), inner);
      return;
    }

    if (inner.Kind == SlotKind.Share)
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    slot.Kind = SlotKind.Clone;
    slot.Statements =
    [
      "if (!source.HasValue)",
      "{",
      "  return null;",
      "}",
      $"return {CopyExpression(inner, "source.GetValueOrDefault()")};"
    ];
  }

  private void ClassifyKeyValuePair(Slot slot, INamedTypeSymbol type)
  {
    Slot key = Build(type.TypeArguments[0]);
    Slot value = Build(type.TypeArguments[1]);
    slot.Dependencies.Add(key);
    slot.Dependencies.Add(value);
    if (key.Kind == SlotKind.Error || value.Kind == SlotKind.Error)
    {
      Fail(slot, "a key or value cannot be cloned", LocationOf(type), key.Kind == SlotKind.Error ? key : value);
      return;
    }

    if (key.Kind == SlotKind.Share && value.Kind == SlotKind.Share)
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    slot.Kind = SlotKind.Clone;
    slot.Statements =
    [
      $"return new {type.ToDisplayString(Format)}({CopyExpression(key, "source.Key")}, {CopyExpression(value, "source.Value")});"
    ];
  }

  private bool IsValueTuple(INamedTypeSymbol type) => Catalog.IsValueTuple(type);

  // ValueTuple fields are public and mutable, so they are assigned directly; no accessor is needed.
  private void ClassifyTuple(Slot slot, INamedTypeSymbol type)
  {
    List<string> lines = [$"{type.ToDisplayString(Format)} clone = source;"];
    for (int index = 0; index < type.TypeArguments.Length; index++)
    {
      string field = index == 7 ? "Rest" : $"Item{index + 1}";
      Slot item = Build(type.TypeArguments[index]);
      slot.Dependencies.Add(item);
      if (item.Kind == SlotKind.Error)
      {
        Fail(slot, $"tuple element {field} cannot be cloned", LocationOf(type), item);
        return;
      }

      if (item.Kind != SlotKind.Share)
      {
        lines.Add($"clone.{field} = {CopyExpression(item, $"source.{field}")};");
      }
    }

    if (lines.Count == 1)
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    lines.Add("return clone;");
    slot.Kind = SlotKind.Clone;
    slot.Statements = lines;
  }

  private static void ClassifyStringBuilder(Slot slot, INamedTypeSymbol type)
  {
    string name = type.ToDisplayString(Format);
    slot.Kind = SlotKind.Clone;
    slot.Statements =
    [
      $"if (map.TryGet(source, out {name}? existing))",
      "{",
      "  return existing;",
      "}",
      $"{name} clone = new(source.ToString(), source.Capacity);",
      "map.Add(source, clone);",
      "return clone;"
    ];
  }

}
