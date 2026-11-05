#region Purpose
// Constructor, record, and polymorphism member shapes compile with no generator diagnostic and no compiler error.
#endregion

#region Design
// Each [Input] names one entry in StateCloneShapeSources.ConstructorShapes. A failure reports the shape name.
// The guard in state-clone-shape-guard-tests.cs fails when an Input and a dictionary key drift apart.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Compile_Constructor_And_Record_Shape
{
  [Input("Polymorphic")]
  [Input("AmbiguousConstructor")]
  [Input("PrivateConstructor")]
  [Input("RequiredMembers")]
  [Input("Records")]
  [Input("NameCollision")]
  [Input("DependencyConstructorState")]
  public static void Given_Shape(string shape) =>
    StateCloneShapeSources.AssertCompiles(StateCloneShapeSources.ConstructorShapes[shape]);
}
