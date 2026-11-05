#region Purpose
// Collection member shapes compile with no generator diagnostic and no compiler error.
#endregion

#region Design
// Each [Input] names one entry in StateCloneShapeSources.CollectionShapes. A failure reports the shape name.
// The guard in state-clone-shape-guard-tests.cs fails when an Input and a dictionary key drift apart.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Compile_Collection_Shape
{
  [Input("CollectionInterfaces")]
  [Input("ImmutableAndFrozen")]
  [Input("OtherBclCollections")]
  [Input("CollectionSubclass")]
  public static void Given_Shape(string shape) =>
    StateCloneShapeSources.AssertCompiles(StateCloneShapeSources.CollectionShapes[shape]);
}
