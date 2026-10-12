#region Purpose
// Test-only Initialize for ActionTrackingState.
#endregion

#region Design
// Initialize is guarded by ThrowIfNotTestAssembly.
#endregion

namespace TimeWarp.Features.ActionTracking;

public partial class ActionTrackingState
{
  internal void Initialize(List<IAction> processingList)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    ActiveActionList = processingList;
  }
}
