#region Purpose
// WithTrackingName step names for every State generator, shared with the incrementality tests.
#endregion

#region Design
// One name per pipeline step so a test can assert each step's run reason (Cached, Unchanged, Modified) after an
// edit. Names are prefixed by generator so tests that run several generators in one driver stay unambiguous.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal static class TrackingNames
{
  public const string ActionSetCandidates = "ActionSet.Candidates";
  public const string ActionSetModels = "ActionSet.Models";
  public const string ActionSetDiagnostics = "ActionSet.Diagnostics";

  public const string CatalogEntries = "Catalog.Entries";
  public const string CatalogAssemblyName = "Catalog.AssemblyName";
  public const string CatalogCollected = "Catalog.Collected";

  public const string PersistenceCandidates = "Persistence.Candidates";
  public const string PersistenceModels = "Persistence.Models";
  public const string PersistenceDiagnostics = "Persistence.Diagnostics";

  public const string ClonePlan = "Clone.Plan";
}
