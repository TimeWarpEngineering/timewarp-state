#region Purpose
// Test-only Initialize for CloneableState.
#endregion

#region Design
// Initialize(count) is test-only, guarded by ThrowIfNotTestAssembly.
#endregion

namespace Test.App.Client.Features.CloneTest;

public partial class CloneableState
{
  /// <summary>
  /// Use in Tests ONLY, to initialize the State
  /// </summary>
  /// <param name="count"></param>
  public void Initialize(int count)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    Count = count;
  }
}
