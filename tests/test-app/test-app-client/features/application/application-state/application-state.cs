#region Purpose
// ApplicationState: the app name, its assembly version, and the last exception message.
#endregion

#region Design
// Name is set in Initialize. ExceptionMessage is written by ExceptionNotificationHandler so integration tests can
// assert that a failed action was reported. Version is computed from the assembly, not stored.
#endregion

namespace Test.App.Client.Features.Application;

public sealed partial class ApplicationState : State<ApplicationState>
{
  public string Name { get; private set; } = null!;
  public string? ExceptionMessage { get; private set; }

  public string? Version => GetType().Assembly.GetName().Version?.ToString();

  public override void Initialize() => Name = "TimeWarp.State Test App";
}
