namespace TimeWarp.Features.Processing;

public partial class ProcessingState : State<ProcessingState>
{
  // TODO: why not just keep the IAction in the list?
  private List<string> ActiveActionsList = [];
  public bool IsProcessing => ActiveActionsList.Count > 0;
  public bool IsProcessingAny(params string[] aActions) => aActions.Intersect(ActiveActionsList).Any();
  public IReadOnlyList<string> ActiveActions => ActiveActionsList.AsReadOnly();

  public override void Initialize() => ActiveActionsList = [];
}
