namespace Test.App.Client.Features.Application;

public partial class ApplicationState : State<ApplicationState>
{
  public string Name { get; private set; } = null!;
  public string? ExceptionMessage { get; private set; }

  public string? Version => GetType().Assembly.GetName().Version?.ToString();

  public override void Initialize() => Name = "TimeWarp.State Test App";
}
