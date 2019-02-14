using BlazorState;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorStateSample.Client
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddBlazorState();
    }

    public void Configure(IComponentsApplicationBuilder app)
    {
      app.AddComponent<App>("app");
    }
  }
}
