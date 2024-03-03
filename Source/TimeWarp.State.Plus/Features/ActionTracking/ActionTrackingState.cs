namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState : State<ActionTrackingState>
{
  private List<IAction> ActiveActionsList = [];
  public bool IsActive => ActiveActionsList.Count > 0;
  public bool IsAnyActive(params Type[] actionTypes) => 
    ActiveActionsList.Any(action => actionTypes.Any(type => type.IsInstanceOfType(action)));

  public IReadOnlyList<IAction> ActiveActions => ActiveActionsList.AsReadOnly();

  public override void Initialize() => ActiveActionsList = [];
}
