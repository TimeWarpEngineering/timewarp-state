#region Purpose
// Fails when a shape dictionary and the [Input] list of the test that reads it drift apart.
#endregion

#region Design
// Fixie [Input] lists and the shape dictionaries are maintained by hand in separate places. A key with no Input is a
// shape that never runs; an Input with no key throws KeyNotFoundException only when that case runs. Both directions
// are checked here by reading the [Input] attributes of each Given_Shape method. Keys must also be unique across the
// supported groups, since StateCloneShapeSources.Find resolves a key without naming its group.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Keep_Shape_Tables_And_Inputs_In_Sync
{
  private static List<string> InputsOf(Type testClass)
  {
    MethodInfo method = testClass.GetMethod("Given_Shape") ?? throw new InvalidOperationException($"{testClass.Name} has no Given_Shape.");
    return method.GetCustomAttributes<InputAttribute>()
      .Select(input => (string)input.Parameters[0]!)
      .ToList();
  }

  private static List<string> Duplicates(IEnumerable<string> values) =>
    values.GroupBy(value => value, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

  private static void AssertInSync(string table, IEnumerable<string> keys, Type testClass)
  {
    List<string> inputs = InputsOf(testClass);
    Duplicates(inputs).ShouldBeEmpty($"{testClass.Name} repeats an Input");
    inputs.Except(keys).ShouldBeEmpty($"{testClass.Name} has Inputs with no key in {table}");
    keys.Except(inputs).ShouldBeEmpty($"{table} has keys with no Input on {testClass.Name}.Given_Shape");
  }

  public static void Given_Generic_Shapes() =>
    AssertInSync(nameof(StateCloneShapeSources.GenericShapes), StateCloneShapeSources.GenericShapes.Keys, typeof(Should_Compile_Generic_Shape));

  public static void Given_Collection_Shapes() =>
    AssertInSync(nameof(StateCloneShapeSources.CollectionShapes), StateCloneShapeSources.CollectionShapes.Keys, typeof(Should_Compile_Collection_Shape));

  public static void Given_Constructor_And_Record_Shapes() =>
    AssertInSync(nameof(StateCloneShapeSources.ConstructorShapes), StateCloneShapeSources.ConstructorShapes.Keys, typeof(Should_Compile_Constructor_And_Record_Shape));

  public static void Given_CloneShared_And_Cloneable_Shapes() =>
    AssertInSync(nameof(StateCloneShapeSources.SharedShapes), StateCloneShapeSources.SharedShapes.Keys, typeof(Should_Compile_CloneShared_And_Cloneable_Shape));

  public static void Given_TWSG002_Shapes() =>
    AssertInSync(nameof(Should_Report_TWSG002_At_Source.Shapes), Should_Report_TWSG002_At_Source.Shapes.Keys, typeof(Should_Report_TWSG002_At_Source));

  public static void Given_Supported_Groups_Have_No_Duplicate_Keys()
  {
    List<string> keys =
      StateCloneShapeSources.GenericShapes.Keys
        .Concat(StateCloneShapeSources.CollectionShapes.Keys)
        .Concat(StateCloneShapeSources.ConstructorShapes.Keys)
        .Concat(StateCloneShapeSources.SharedShapes.Keys)
        .ToList();
    Duplicates(keys).ShouldBeEmpty();
    StateCloneShapeSources.All.Count.ShouldBe(keys.Count);
  }

  // The guard itself: a TWSG002 case with no message fragments would assert nothing about why it failed.
  public static void Given_Every_TWSG002_Shape_Names_Something()
  {
    foreach (KeyValuePair<string, (string Source, string[] MessageFragments)> shape in Should_Report_TWSG002_At_Source.Shapes)
    {
      shape.Value.MessageFragments.ShouldNotBeEmpty(shape.Key);
    }
  }
}
