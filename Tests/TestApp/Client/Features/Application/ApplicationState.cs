namespace TestApp.Client.Features.Application
{
  using BlazorState;

  internal partial class ApplicationState : State<ApplicationState>
  {
    public string Name { get; private set; }
    public string ExceptionMessage { get; private set; }

    public string Version => GetType().Assembly.GetName().Version.ToString();

    public ApplicationState() { }

    public override void Initialize() => Name = "Blazor State Demo Application";
  }
}