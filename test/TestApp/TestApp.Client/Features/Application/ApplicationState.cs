namespace TestApp.Client.Features.Application
{
  public partial class ApplicationState
  {
    public string Name { get; private set; }

    public string Version => GetType().Assembly.GetName().Version.ToString();
  }
}