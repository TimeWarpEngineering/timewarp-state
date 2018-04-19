namespace BlazorState.Client.State
{
  using BlazorState.State;

  public class ApplicationState : State<ApplicationState>
  {
    public ApplicationState() { }

    protected ApplicationState(ApplicationState aState) : this()
    {
      Name = aState.Name;
    }

    public string Name { get; set; }

    public override object Clone() => (ApplicationState)new ApplicationState(this);

    protected override void Initialize()
    {
      Name = "Blazor State Demo Application";
    }
  }
}