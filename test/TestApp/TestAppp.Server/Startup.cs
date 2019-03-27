namespace TestApp.Server
{
  using System.Reflection;
  using MediatR;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.DependencyInjection;
  using Newtonsoft.Json.Serialization;

  public class Startup
  {
    public void Configure(IApplicationBuilder aApplicationBuilder, IHostingEnvironment aHostingEnvironment)
    {
      aApplicationBuilder.UseResponseCompression();

      if (aHostingEnvironment.IsDevelopment())
      {
        aApplicationBuilder.UseDeveloperExceptionPage();
      }

      aApplicationBuilder.UseMvc();
      aApplicationBuilder.UseBlazorDualMode<Client.Startup>();
      aApplicationBuilder.UseBlazorDebugging();

    }

    public void ConfigureServices(IServiceCollection aServiceCollection)
    {
      aServiceCollection.AddMvc()
        .AddNewtonsoftJson(aOptions =>
           aOptions.SerializerSettings.ContractResolver =
              new DefaultContractResolver());

      aServiceCollection.AddRazorComponents<Client.Startup>();

      aServiceCollection.AddResponseCompression();
      aServiceCollection.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);
      aServiceCollection.Scan(aTypeSourceSelector => aTypeSourceSelector
        .FromAssemblyOf<Startup>()
        .AddClasses()
        .AsSelf()
        .WithScopedLifetime());
    }
  }
}