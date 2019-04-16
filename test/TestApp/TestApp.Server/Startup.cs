namespace TestApp.Server
{
  using BlazorState;
  using MediatR;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.ResponseCompression;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Newtonsoft.Json.Serialization;
  using System.Linq;
  using System.Reflection;

  public class Startup
  {
    public void Configure(IApplicationBuilder aApplicationBuilder, IWebHostEnvironment aWebHostEnvironment)
    {
      aApplicationBuilder.UseResponseCompression();

      if (aWebHostEnvironment.IsDevelopment())
      {
        aApplicationBuilder.UseDeveloperExceptionPage();
        aApplicationBuilder.UseBlazorDebugging();
      }

      aApplicationBuilder.UseStaticFiles();
      aApplicationBuilder.UseRouting();
      aApplicationBuilder.UseEndpoints(aEndpointRouteBuilder =>
      {
        aEndpointRouteBuilder.MapControllers(); // We use explicit attribute routing so dont need MapDefaultControllerRoute
        aEndpointRouteBuilder.MapBlazorHub();
        aEndpointRouteBuilder.MapFallbackToPage("/_Host");
      });
      //aApplicationBuilder.UseBlazor<Client.Startup>();
      //aApplicationBuilder.UseBlazorDualMode<Client.Startup>();

    }

    public void ConfigureServices(IServiceCollection aServiceCollection)
    {
      aServiceCollection.AddRazorPages();
      aServiceCollection.AddServerSideBlazor();

      // TODO: why do I need DefaultContractResolver??  I added for some reason is reason still valid now?
      aServiceCollection.AddMvc()
        .AddNewtonsoftJson(aOptions =>
           aOptions.SerializerSettings.ContractResolver =
              new DefaultContractResolver());

      aServiceCollection.AddResponseCompression(opts =>
      {
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                  new[] { "application/octet-stream" });
      });

      // TODO get working Client Side first then come back and try dual mode.
      //aServiceCollection.AddBlazorState((a) => a.Assemblies =
      // new Assembly[] { typeof(Startup).GetTypeInfo().Assembly, typeof(Client.Startup).GetTypeInfo().Assembly }
      //);
      
      aServiceCollection.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);
      aServiceCollection.Scan(aTypeSourceSelector => aTypeSourceSelector
        .FromAssemblyOf<Startup>()
        .AddClasses()
        .AsSelf()
        .WithScopedLifetime());
    }
  }
}