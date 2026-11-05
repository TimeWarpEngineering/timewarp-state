#region Purpose
// WithTrackingName step names for TimeWarpStateGenerator, shared with the incrementality tests.
#endregion

#region Design
// One name per pipeline step, so a test can assert each step's run reason (Cached, Unchanged, Modified) after an
// edit. Fact steps feed State.Graph. Emit steps are projections of the graph, one per emitter.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal static class TrackingNames
{
  public const string Actions = "State.Actions";
  public const string Persistence = "State.Persistence";
  public const string States = "State.States";
  public const string Assembly = "State.Assembly";
  public const string ClonePlan = "State.ClonePlan";
  public const string Graph = "State.Graph";

  public const string EmitDispatch = "State.Emit.Dispatch";
  public const string EmitPersistence = "State.Emit.Persistence";
  public const string EmitCatalog = "State.Emit.Catalog";
  public const string EmitManifest = "State.Emit.Manifest";
  public const string EmitClone = "State.Emit.Clone";
  public const string EmitDiagnostics = "State.Emit.Diagnostics";
}
