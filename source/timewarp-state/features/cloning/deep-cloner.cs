#region Purpose
// Deep-copies object graphs (state snapshots for StateTransactionBehavior) without blocking waits.
#endregion

#region Design
// Replaces AnyClone 1.1.6 / TypeSupport 1.2.0. TypeSupport's ExtendedTypeCache guards its cache with
// SemaphoreSlim.Wait, and .NET 11 throws PlatformNotSupportedException for any potentially blocking wait on
// single-threaded browser WASM (dotnet/runtime#123329), so every action through StateTransactionBehavior failed
// in the browser. This cloner never waits: per-type plans live in a ConcurrentDictionary and a clone is one
// synchronous walk with a per-call reference map.
//
// Rules (kept in line with the AnyClone defaults the library used):
// - Fields marked, or auto-property backing fields whose property is marked, [IgnoreDataMember], [NonSerialized]
//   or [JsonIgnore] (matched by attribute name, any namespace) are not copied. A cloned class keeps what its
//   constructor assigned, which is how State<T>.Guid and the CancellationTokenSource stay unique per instance.
// - Class instances are created with the parameterless constructor (any accessibility), else the constructor with
//   the fewest parameters given default arguments, else uninitialized. Structs are copied memberwise.
// - Immutable or identity-bound values (primitives, string, enums, Guid, dates, Type/MemberInfo, delegates,
//   comparers, System.Threading primitives, tasks, Uri, Version, CultureInfo, Regex) are shared, not copied.
// - Arrays (any rank) and graphs with shared references or cycles are copied once per original instance.
// - Hash-based collections are copied field-for-field, so keys must use value equality (strings, numbers, records);
//   keys that rely on reference identity do not survive any deep clone.
#endregion

namespace TimeWarp.Features.Cloning;

/// <summary>
/// Called when a member cannot be cloned. The member keeps the value the clone's constructor gave it.
/// </summary>
/// <param name="exception">The failure.</param>
/// <param name="path">Member path from the root, for example <c>$.Items.[].Name</c>.</param>
public delegate void CloneErrorHandler(Exception exception, string path);

internal static class DeepCloner
{
  private static readonly string[] IgnoredAttributeNames =
  [
    "IgnoreDataMemberAttribute",
    "NonSerializedAttribute",
    "JsonIgnoreAttribute"
  ];

  private static readonly ConcurrentDictionary<Type, TypePlan> Plans = new();

  private static readonly Func<object, object> MemberwiseCloneFunc =
    typeof(object)
      .GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!
      .CreateDelegate<Func<object, object>>();

  internal static T Clone<T>(T source, CloneErrorHandler? onError)
  {
    if (source is null) return source;
    var context = new CloneContext(onError);
    return (T)CloneValue(source, context)!;
  }

  private static object? CloneValue(object? value, CloneContext context)
  {
    if (value is null) return null;

    TypePlan plan = GetPlan(value.GetType());
    if (plan.Kind == PlanKind.Share) return value;

    if (plan.Kind == PlanKind.Struct) return CloneStruct(value, plan, context);

    if (context.Visited.TryGetValue(value, out object? existing)) return existing;

    return plan.Kind == PlanKind.Array
      ? CloneArray((Array)value, plan, context)
      : CloneClass(value, plan, context);
  }

  private static object CloneClass(object source, TypePlan plan, CloneContext context)
  {
    object clone = plan.Factory!();
    context.Visited[source] = clone;
    CopyFields(source, clone, plan, context);
    return clone;
  }

  private static object CloneStruct(object source, TypePlan plan, CloneContext context)
  {
    // MemberwiseClone of a box is a new box, so the caller's boxed value is never mutated.
    object clone = MemberwiseCloneFunc(source);
    CopyFields(source, clone, plan, context);
    return clone;
  }

  private static void CopyFields(object source, object clone, TypePlan plan, CloneContext context)
  {
    foreach (FieldPlan field in plan.Fields)
    {
      context.Path.Add(field.Name);
      try
      {
        object? value = field.Info.GetValue(source);
        field.Info.SetValue(clone, field.CopyDirect ? value : CloneValue(value, context));
      }
      catch (Exception exception) when (context.OnError is not null)
      {
        context.OnError(exception, context.CurrentPath);
      }
      finally
      {
        context.Path.RemoveAt(context.Path.Count - 1);
      }
    }
  }

  private static Array CloneArray(Array source, TypePlan plan, CloneContext context)
  {
    Type elementType = plan.ElementType!;
    int rank = source.Rank;
    var lengths = new int[rank];
    var lowerBounds = new int[rank];
    for (int dimension = 0; dimension < rank; dimension++)
    {
      lengths[dimension] = source.GetLength(dimension);
      lowerBounds[dimension] = source.GetLowerBound(dimension);
    }

    Array clone = rank == 1 && lowerBounds[0] == 0
      ? Array.CreateInstance(elementType, lengths[0])
      : Array.CreateInstance(elementType, lengths, lowerBounds);

    context.Visited[source] = clone;

    if (source.Length == 0) return clone;

    if (plan.ElementsCopyDirect)
    {
      Array.Copy(source, clone, source.Length);
      return clone;
    }

    context.Path.Add("[]");
    try
    {
      if (rank == 1)
      {
        int lower = lowerBounds[0];
        for (int index = lower; index < lower + lengths[0]; index++)
        {
          CloneElement(source, clone, context, index);
        }
      }
      else
      {
        var indices = (int[])lowerBounds.Clone();
        for (int count = 0; count < source.Length; count++)
        {
          CloneElement(source, clone, context, indices);
          for (int dimension = rank - 1; dimension >= 0; dimension--)
          {
            if (++indices[dimension] < lowerBounds[dimension] + lengths[dimension]) break;
            indices[dimension] = lowerBounds[dimension];
          }
        }
      }
    }
    finally
    {
      context.Path.RemoveAt(context.Path.Count - 1);
    }

    return clone;
  }

  private static void CloneElement(Array source, Array clone, CloneContext context, int index)
  {
    try
    {
      clone.SetValue(CloneValue(source.GetValue(index), context), index);
    }
    catch (Exception exception) when (context.OnError is not null)
    {
      context.OnError(exception, context.CurrentPath);
    }
  }

  private static void CloneElement(Array source, Array clone, CloneContext context, int[] indices)
  {
    try
    {
      clone.SetValue(CloneValue(source.GetValue(indices), context), indices);
    }
    catch (Exception exception) when (context.OnError is not null)
    {
      context.OnError(exception, context.CurrentPath);
    }
  }

  private static TypePlan GetPlan(Type type) => Plans.GetOrAdd(type, CreatePlan);

  private static TypePlan CreatePlan(Type type)
  {
    if (IsShared(type)) return TypePlan.Share;

    if (type.IsArray)
    {
      Type elementType = type.GetElementType()!;
      return new TypePlan(PlanKind.Array)
      {
        ElementType = elementType,
        ElementsCopyDirect = IsCopyDirect(elementType)
      };
    }

    if (type.IsValueType)
    {
      FieldPlan[] structFields = GetInstanceFields(type)
        .Where(field => !IsCopyDirect(field.FieldType))
        .Select(field => new FieldPlan(field, copyDirect: false))
        .ToArray();

      // A struct whose fields are all shareable is copied by boxing alone.
      return structFields.Length == 0 ? TypePlan.Share : new TypePlan(PlanKind.Struct) { Fields = structFields };
    }

    FieldPlan[] fields = GetInstanceFields(type)
      .Where(field => !IsIgnored(field))
      .Select(field => new FieldPlan(field, IsCopyDirect(field.FieldType)))
      .ToArray();

    return new TypePlan(PlanKind.Class)
    {
      Fields = fields,
      Factory = CreateFactory(type)
    };
  }

  /// <summary>
  /// True when a value statically typed as <paramref name="declaredType"/> never needs a deep copy.
  /// Only value types and sealed classes qualify; any other declared type can hold a mutable subtype.
  /// </summary>
  private static bool IsCopyDirect(Type declaredType)
  {
    if (declaredType.IsPointer || declaredType.IsByRef) return true;
    if (declaredType.IsValueType) return GetPlan(declaredType).Kind == PlanKind.Share;
    return declaredType.IsSealed && !declaredType.IsArray && IsShared(declaredType);
  }

  private static bool IsShared(Type type)
  {
    if (type.IsPrimitive || type.IsEnum || type.IsPointer || type.IsByRef || type.IsCOMObject) return true;

    if
    (
      type == typeof(string) ||
      type == typeof(decimal) ||
      type == typeof(DateTime) ||
      type == typeof(DateTimeOffset) ||
      type == typeof(TimeSpan) ||
      type == typeof(DateOnly) ||
      type == typeof(TimeOnly) ||
      type == typeof(Guid) ||
      type == typeof(Half) ||
      type == typeof(Int128) ||
      type == typeof(UInt128) ||
      type == typeof(Uri) ||
      type == typeof(Version) ||
      type == typeof(DBNull) ||
      type == typeof(object)
    )
    {
      return true;
    }

    if
    (
      typeof(Delegate).IsAssignableFrom(type) ||
      typeof(MemberInfo).IsAssignableFrom(type) ||
      typeof(Assembly).IsAssignableFrom(type) ||
      typeof(Module).IsAssignableFrom(type) ||
      typeof(ParameterInfo).IsAssignableFrom(type) ||
      typeof(System.Globalization.CultureInfo).IsAssignableFrom(type) ||
      typeof(Regex).IsAssignableFrom(type) ||
      typeof(Encoding).IsAssignableFrom(type) ||
      typeof(IServiceProvider).IsAssignableFrom(type)
    )
    {
      return true;
    }

    // Synchronization primitives, timers, tasks and cancellation sources are identity-bound.
    if (!type.IsValueType && type.Namespace is "System.Threading" or "System.Threading.Tasks") return true;

    return IsComparer(type);
  }

  private static bool IsComparer(Type type)
  {
    if (type.IsValueType) return false;
    if (typeof(IEqualityComparer).IsAssignableFrom(type) || typeof(IComparer).IsAssignableFrom(type)) return true;

    foreach (Type implemented in type.GetInterfaces())
    {
      if (!implemented.IsGenericType) continue;
      Type definition = implemented.GetGenericTypeDefinition();
      if (definition == typeof(IEqualityComparer<>) || definition == typeof(IComparer<>)) return true;
    }

    return false;
  }

  private static IEnumerable<FieldInfo> GetInstanceFields(Type type)
  {
    const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    for (Type? current = type; current is not null && current != typeof(object); current = current.BaseType)
    {
      foreach (FieldInfo field in current.GetFields(flags))
      {
        if (field.IsLiteral) continue;
        yield return field;
      }
    }
  }

  private static bool IsIgnored(FieldInfo field)
  {
    if (HasIgnoredAttribute(field)) return true;

    // Auto-property backing field: "<Name>k__BackingField". The attributes sit on the property.
    string name = field.Name;
    if (name.Length > 0 && name[0] == '<')
    {
      int end = name.IndexOf('>', StringComparison.Ordinal);
      if (end > 1)
      {
        PropertyInfo? property = field.DeclaringType?.GetProperty
        (
          name[1..end],
          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
        );
        if (property is not null && HasIgnoredAttribute(property)) return true;
      }
    }

    return false;
  }

  private static bool HasIgnoredAttribute(MemberInfo member)
  {
    if (member is FieldInfo { IsNotSerialized: true }) return true;

    foreach (CustomAttributeData attribute in member.GetCustomAttributesData())
    {
      if (Array.IndexOf(IgnoredAttributeNames, attribute.AttributeType.Name) >= 0) return true;
    }

    return false;
  }

  private static Func<object> CreateFactory(Type type)
  {
    if (type.IsAbstract) return () => throw new InvalidOperationException($"Cannot create an instance of abstract type {type}.");

    const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    ConstructorInfo? parameterless = type.GetConstructor(flags, Type.EmptyTypes);
    if (parameterless is not null)
    {
      return () => InvokeOrUninitialized(type, parameterless, []);
    }

    ConstructorInfo? constructor = type.GetConstructors(flags)
      .Where(candidate => candidate.GetParameters().All(parameter => !parameter.ParameterType.IsByRef && !parameter.ParameterType.IsPointer))
      .OrderBy(candidate => candidate.GetParameters().Length)
      .FirstOrDefault();

    if (constructor is not null)
    {
      object?[] arguments = constructor.GetParameters().Select(GetDefaultArgument).ToArray();
      return () => InvokeOrUninitialized(type, constructor, (object?[])arguments.Clone());
    }

    return () => RuntimeHelpers.GetUninitializedObject(type);
  }

  private static object? GetDefaultArgument(ParameterInfo parameter)
  {
    if (parameter.HasDefaultValue && parameter.DefaultValue is not DBNull) return parameter.DefaultValue;
    return parameter.ParameterType.IsValueType ? RuntimeHelpers.GetUninitializedObject(parameter.ParameterType) : null;
  }

  private static object InvokeOrUninitialized(Type type, ConstructorInfo constructor, object?[] arguments)
  {
    try
    {
      return constructor.Invoke(arguments);
    }
    catch (TargetInvocationException)
    {
      return RuntimeHelpers.GetUninitializedObject(type);
    }
  }

  private enum PlanKind
  {
    Share,
    Class,
    Struct,
    Array
  }

  private sealed class TypePlan(PlanKind kind)
  {
    public static readonly TypePlan Share = new(PlanKind.Share);

    public PlanKind Kind { get; } = kind;
    public FieldPlan[] Fields { get; init; } = [];
    public Func<object>? Factory { get; init; }
    public Type? ElementType { get; init; }
    public bool ElementsCopyDirect { get; init; }
  }

  private sealed class FieldPlan(FieldInfo info, bool copyDirect)
  {
    public FieldInfo Info { get; } = info;
    public string Name { get; } = info.Name;
    public bool CopyDirect { get; } = copyDirect;
  }

  private sealed class CloneContext(CloneErrorHandler? onError)
  {
    public CloneErrorHandler? OnError { get; } = onError;
    public Dictionary<object, object> Visited { get; } = new(ReferenceEqualityComparer.Instance);
    public List<string> Path { get; } = ["$"];
    public string CurrentPath => string.Join('.', Path);
  }
}
