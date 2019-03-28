namespace TestApp.Client.Features.Application
{
  using BlazorState;

  public partial class ApplicationState : State<ApplicationState>
  {
    public ApplicationState() { }

    protected ApplicationState(ApplicationState aState) : this()
    {
      Name = aState.Name;
    }

    public string Name { get; private set; }

    public string Version => GetType().Assembly.GetName().Version.ToString();


    public override object Clone() => new ApplicationState(this);

    protected override void Initialize() => Name = "Blazor State Demo Application";
  }
}