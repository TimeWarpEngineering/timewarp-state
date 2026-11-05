#region Purpose
// [CloneShared] and ICloneable member shapes compile with no generator diagnostic and no compiler error.
#endregion

#region Design
// Each [Input] names one entry in StateCloneShapeSources.SharedShapes. A failure reports the shape name.
// The guard in state-clone-shape-guard-tests.cs fails when an Input and a dictionary key drift apart.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Compile_CloneShared_And_Cloneable_Shape
{
  [Input("CloneableNonSealedBase")]
  [Input("PrivateCloneableSubclass")]
  [Input("CloneSharedServices")]
  [Input("CloneSharedPrecedence")]
  public static void Given_Shape(string shape) =>
    StateCloneShapeSources.AssertCompiles(StateCloneShapeSources.SharedShapes[shape]);
}
