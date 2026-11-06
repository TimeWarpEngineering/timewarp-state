#region Purpose
// Emits the Load() partial for a top-level [PersistentState] state.
#endregion

#region Design
// A pure function of one PersistenceModel (PersistenceFactExtractor builds it). The text is the pre-142 persistence
// generator's, byte for byte, which the golden tests pin. Load() sends the shared LoadPersistentStateRequest, and
// PersistentStateMethod is read at runtime. Generated code names Task, CancellationToken and CancellationTokenSource
// with global::, so it does not depend on the consumer's usings. The partial carries no accessibility modifier, so an
// internal state does not get CS0262.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal static class PersistenceEmitter
{
  public static void Emit(PersistenceModel model, SourceProductionContext context)
  {
    string generatedCode = GenerateLoadClassCode(model.NamespaceName, model.TypeName);
    context.AddSource(model.HintName, SourceText.From(generatedCode, Encoding.UTF8));
  }

  internal static string GenerateLoadClassCode(string? namespaceName, string typeName)
  {
    string namespaceLine = namespaceName is null ? string.Empty : $"namespace {namespaceName};\n";
    return $$$"""
      #nullable enable

      #pragma warning disable CS1591
      {{{namespaceLine}}}
      partial class {{{typeName}}}
      {
        /// <summary>
        /// (Re)loads this [PersistentState] state from its configured persistence store.
        /// </summary>
        public async global::System.Threading.Tasks.Task Load(global::System.Threading.CancellationToken? externalCancellationToken = null)
        {
          using global::System.Threading.CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
            ? global::System.Threading.CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
            : null;

          // Load is dispatched through a single hand-written Mediator request whose handler is
          // registered by Mediator's generator (a per-state generated handler would not be).
          await Sender.Send
          (
            new global::TimeWarp.State.Plus.PersistentState.LoadPersistentStateRequest(typeof({{{typeName}}})),
            linkedCts?.Token ?? CancellationToken
          );
        }
      }
      #pragma warning restore CS1591

      """;
  }
}

internal sealed record PersistenceModel
(
  string? NamespaceName,
  string TypeName,
  string HintName
);
