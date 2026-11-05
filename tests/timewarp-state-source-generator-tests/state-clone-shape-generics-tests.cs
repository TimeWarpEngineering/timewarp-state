#region Purpose
// Generic and tuple member shapes compile with no generator diagnostic and no compiler error.
#endregion

#region Design
// Each [Input] names one entry in StateCloneShapeSources.GenericShapes. A failure reports the shape name.
// The guard in state-clone-shape-guard-tests.cs fails when an Input and a dictionary key drift apart.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Compile_Generic_Shape
{
  [Input("GenericWrapper")]
  [Input("GenericStateBase")]
  [Input("NestedGeneric")]
  [Input("Tuple")]
  [Input("KeyValuePair")]
  [Input("NullableStruct")]
  [Input("GenericSubclassOfGenericClass")]
  [Input("GenericSubclassOfAbstractGenericClass")]
  [Input("GenericImplementationOfGenericInterface")]
  public static void Given_Shape(string shape) =>
    StateCloneShapeSources.AssertCompiles(StateCloneShapeSources.GenericShapes[shape]);
}
