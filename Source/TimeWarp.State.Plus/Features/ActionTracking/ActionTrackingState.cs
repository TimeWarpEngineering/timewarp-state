namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState : State<ActionTrackingState>
{
  private List<IAction> ActiveActionList = [];
  public bool IsActive => ActiveActionList.Count > 0;
  public bool IsAnyActive(params Type[] actionTypes) => 
    ActiveActionList.Any(action => actionTypes.Any(type => type.IsInstanceOfType(action)));

  public IReadOnlyList<IAction> ActiveActions => ActiveActionList.AsReadOnly();

  public override void Initialize() => ActiveActionList = [];
}
