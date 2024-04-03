namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState : State<ActionTrackingState>, ICloneable
{
  private List<IAction> ActiveActionList = [];
  public bool IsActive => ActiveActionList.Count > 0;
  public bool IsAnyActive(params Type[] actionTypes) => 
    ActiveActionList.Any(action => actionTypes.Any(type => type.IsInstanceOfType(action)));

  public IReadOnlyList<IAction> ActiveActions => ActiveActionList.AsReadOnly();

  public override void Initialize() => ActiveActionList = [];
  public object Clone()
  {
    var clonedState = new ActionTrackingState
    {
      // Use the existing list's constructor to ensure the list itself is cloned,
      // but the references to the IAction objects within it remain the same.
      // Intentionally not doing a deep clone here.
      ActiveActionList = [..this.ActiveActionList]
    };
    return clonedState;
  }
}
