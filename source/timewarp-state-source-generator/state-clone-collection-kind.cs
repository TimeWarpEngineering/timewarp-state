#region Purpose
// Names each BCL collection shape the planner rebuilds.
#endregion

#region Design
// None means the type is not a known collection. Interface kinds dispatch on the runtime type and materialize a fallback.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal enum CollectionKind
{
  None,
  List,
  Dictionary,
  HashSet,
  SortedSet,
  SortedDictionary,
  SortedList,
  LinkedList,
  Stack,
  Queue,
  ConcurrentDictionary,
  Collection,
  ObservableCollection,
  ReadOnlyCollection,
  ReadOnlyDictionary,
  ImmutableArray,
  ImmutableList,
  ImmutableDictionary,
  ImmutableSortedDictionary,
  ImmutableHashSet,
  ImmutableSortedSet,
  ImmutableQueue,
  ImmutableStack,
  FrozenDictionary,
  FrozenSet,
  EnumerableInterface,
  DictionaryInterface
}

