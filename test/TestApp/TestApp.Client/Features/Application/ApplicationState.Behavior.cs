namespace TestApp.Client.Features.Application
{
  using BlazorState;

  public partial class ApplicationState : State<ApplicationState>
  {
    public ApplicationState() { }

    private ApplicationState(ApplicationState aState) : this()
    {
      Name = aState.Name;
    }

    public override object Clone() => new ApplicationState(this);

    protected override void Initialize() => Name = "Blazor State Demo Application";
  }
}