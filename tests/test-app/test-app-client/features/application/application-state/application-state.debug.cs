#region Purpose
// Test-only Initialize for ApplicationState.
#endregion

#region Design
// Initialize(name, exceptionMessage) is test-only, guarded by ThrowIfNotTestAssembly.
#endregion

namespace Test.App.Client.Features.Application;

public partial class ApplicationState
{
  internal void Initialize(string name, string exceptionMessage)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    Name = name;
    ExceptionMessage = exceptionMessage;
  }
}
