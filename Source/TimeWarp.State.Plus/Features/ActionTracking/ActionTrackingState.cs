namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState : State<ActionTrackingState>
{
  private List<IAction> ActiveActionsList = [];
  public bool IsProcessing => ActiveActionsList.Count > 0;
  public bool IsProcessingAny(params Type[] actionTypes) => 
    ActiveActionsList.Any(action => actionTypes.Any(type => type.IsInstanceOfType(action)));

  public IReadOnlyList<IAction> ActiveActions => ActiveActionsList.AsReadOnly();

  public override void Initialize() => ActiveActionsList = [];
}
