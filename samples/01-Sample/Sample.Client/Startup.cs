using BlazorState;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Client
{
  public class Startup
  {
    #region ConfigureServices
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddBlazorState();
    }
    #endregion

    public void Configure(IBlazorApplicationBuilder app)
    {
      app.AddComponent<App>("app");
    }
  }
}
