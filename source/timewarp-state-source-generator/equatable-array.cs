#region Purpose
// Value-equality wrapper over ImmutableArray so incremental generator models cache correctly.
#endregion

#region Design
// Readonly struct comparing by sequence with an order-sensitive hash; default and empty count as equal. Used by
// the action catalog generator's records.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
  where T : IEquatable<T>
{
  private readonly ImmutableArray<T> Items;

  public EquatableArray(IEnumerable<T> items) => Items = items.ToImmutableArray();

  public int Count => Items.IsDefault ? 0 : Items.Length;

  public T this[int index] => Items[index];

  public bool Equals(EquatableArray<T> other) =>
    Items.IsDefaultOrEmpty ? other.Items.IsDefaultOrEmpty : !other.Items.IsDefault && Items.SequenceEqual(other.Items);

  public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

  public override int GetHashCode()
  {
    if (Items.IsDefault) return 0;
    int hash = 17;
    foreach (T item in Items) hash = unchecked(hash * 31 + item.GetHashCode());
    return hash;
  }

  public IEnumerator<T> GetEnumerator() =>
    (Items.IsDefault ? ImmutableArray<T>.Empty : Items).AsEnumerable().GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
