namespace BlazorState.Pipeline.ReduxDevTools;


/// <summary>
/// Redux Dev Tools Options (see docs)
/// </summary>
/// <remarks>
/// https://github.com/reduxjs/redux-devtools/blob/f3ead32ebaca555002a92176c97dec9c27f2449c/extension/docs/API/Arguments.md
/// </remarks>
public sealed class ReduxDevToolsOptions
{
  public string Name { get; set; }
  public int Latency { get; set; }
  public int MaxAge { get; set; }
  public bool Trace { get; set; }
  public int TraceLimit { get; set; }
  // serialize is not implemented thus will use JSON.stringy
  // actionSanitizer
  // stateSanitizer
  // actionsDenylist
  // actionsAllowlist
  // predicate
  // shouldRecordChanges
  // pauseActionType
  // autoPause
  // shouldStartLocked
  // shouldHotReload
  // shouldCatchErrors
  public TFeatures Features {get; set;}

  /// <summary>
  /// 
  /// </summary>
  /// <param name="Pause">start/pause recording of dispatched actions</param>
  /// <param name="Lock">lock/unlock dispatching actions and side effects</param>
  /// <param name="Persist">persist states on page reloading</param>
  /// <param name="Export">export history of actions in a file</param>
  /// <param name="Import">import history of actions from a file</param>
  /// <param name="Jump">jump back and forth (time travelling)</param>
  /// <param name="Skip">skip (cancel) actions</param>
  /// <param name="Reorder">drag and drop actions in the history list</param>
  /// <param name="Dispatch">dispatch custom actions or action creators</param>
  /// <param name="Test">generate tests for the selected actions</param>
  public record TFeatures
  (
    bool Pause,
    bool Lock,
    bool Persist,
    bool Export,
    string Import,
    bool Jump,
    bool Skip,
    bool Reorder,
    bool Dispatch,
    bool Test
  );  
}
