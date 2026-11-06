#region Purpose
// Equatable diagnostic data for TimeWarpStateGenerator, plus its descriptors: TWSG001 (nested [PersistentState]),
// TWSG002 (a clone cannot be generated) and TWSG003 (a member cannot be generated).
#endregion

#region Design
// A cached model must not hold a Location: it pins a SyntaxTree and compares by reference. LocationInfo keeps the
// file path, span and line span instead. Only failure results carry one, so a success model is not invalidated by
// an edit above it in the same file. ToLocation re-joins the real tree from the current compilation, so #pragma and
// .editorconfig severities apply (the Ramani and Mediator pattern). When the tree is gone it falls back to an
// external location with the same span.
// TWSG003 is an Error because every case it reports used to emit generated code that did not compile.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed record LocationInfo
(
  string FilePath,
  TextSpan TextSpan,
  LinePositionSpan LineSpan
)
{
  public static LocationInfo? From(Location? location)
  {
    if (location is null || !location.IsInSource) return null;
    FileLinePositionSpan lineSpan = location.GetLineSpan();
    return new LocationInfo(location.SourceTree?.FilePath ?? lineSpan.Path ?? string.Empty, location.SourceSpan, lineSpan.Span);
  }

  public Location ToLocation(Compilation? compilation)
  {
    if (compilation is not null)
    {
      foreach (SyntaxTree tree in compilation.SyntaxTrees)
      {
        if (tree.FilePath == FilePath && TextSpan.End <= tree.Length)
          return Location.Create(tree, TextSpan);
      }
    }

    return Location.Create(FilePath, TextSpan, LineSpan);
  }
}

internal sealed record GeneratorDiagnosticInfo
(
  string Id,
  LocationInfo? Location,
  EquatableArray<string> Arguments
)
{
  public Diagnostic ToDiagnostic(Compilation? compilation)
  {
    DiagnosticDescriptor descriptor = StateGeneratorDiagnostics.Find(Id);
    Location location = Location?.ToLocation(compilation) ?? Microsoft.CodeAnalysis.Location.None;
    return Diagnostic.Create(descriptor, location, Arguments.Cast<object>().ToArray());
  }
}

internal static class StateGeneratorDiagnostics
{
  public const string NestedPersistentStateId = "TWSG001";
  public const string CloneUnsupportedId = "TWSG002";
  public const string CannotGenerateMemberId = "TWSG003";

  public static readonly DiagnosticDescriptor NestedPersistentState =
    new
    (
      NestedPersistentStateId,
      title: "[PersistentState] is not supported on nested classes",
      messageFormat: "[PersistentState] is not supported on nested class '{0}'. Move the state to a top-level type.",
      category: "Persistence",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "Policies nest actions in states, not states in other types. Nested [PersistentState] would emit a top-level partial that does not merge with the nested type."
    );

  public static readonly DiagnosticDescriptor CloneUnsupported =
    new
    (
      CloneUnsupportedId,
      title: "State clone cannot be generated",
      messageFormat: "Cannot generate a clone for '{0}'{2} because {1}. Implement ICloneable on '{0}', mark the member [CloneShared] to copy it by reference, or change the unsupported member.",
      category: "Cloning",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "The state clone source generator could not emit a clone. Implement ICloneable, mark the member [CloneShared] to copy it by reference, or change the member named in the message."
    );

  public static readonly DiagnosticDescriptor CannotGenerateMember =
    new
    (
      CannotGenerateMemberId,
      title: "State member cannot be generated",
      messageFormat: "Cannot generate '{0}' for '{1}' because {2}",
      category: "Generation",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "A TimeWarp.State generator found an input it cannot turn into code that compiles. It reports this instead of emitting broken code. Change the declaration named in the message."
    );

  public static DiagnosticDescriptor Find(string id) => id switch
  {
    CannotGenerateMemberId => CannotGenerateMember,
    NestedPersistentStateId => NestedPersistentState,
    _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown generator diagnostic id.")
  };
}
