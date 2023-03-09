namespace TestApp.Server;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Hosting;
using System.Net.Mime;
using System.Reflection;

public class Program
{

  public static void Main(string[] aArgumentArray)
  {
    WebApplicationBuilder builder = WebApplication.CreateBuilder(aArgumentArray);

    ConfigureServices(builder.Services);

    WebApplication webApplication = builder.Build();

    Console.WriteLine($"EnvironmentName: {webApplication.Environment.EnvironmentName}");

    Configure(webApplication, webApplication.Environment);

    webApplication.Run();
  }

  public static void ConfigureServices(IServiceCollection aServiceCollection)
  {
    aServiceCollection.AddRazorPages();
    aServiceCollection.AddServerSideBlazor();
    aServiceCollection.AddMvc();
    aServiceCollection.Configure<ApiBehaviorOptions>
    (
      aApiBehaviorOptions => aApiBehaviorOptions.SuppressInferBindingSourcesForParameters = true
    );

    aServiceCollection.AddResponseCompression
    (
      aResponseCompressionOptions =>
        aResponseCompressionOptions.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat
        (
          new[] { MediaTypeNames.Application.Octet }
        )
    );

    Client.Program.ConfigureServices(aServiceCollection);

    aServiceCollection
      .AddMediatR
      (
        aMediatRServiceConfiguration =>
          aMediatRServiceConfiguration.RegisterServicesFromAssembly(typeof(Startup).GetTypeInfo().Assembly)
      );

  } 

  public static void Configure
  (
    IApplicationBuilder aApplicationBuilder,
    IWebHostEnvironment aWebHostEnvironment
  )
  {
    aApplicationBuilder.UseResponseCompression();

    if (aWebHostEnvironment.IsDevelopment())
    {
      aApplicationBuilder.UseDeveloperExceptionPage();
      aApplicationBuilder.UseWebAssemblyDebugging();
    }

    aApplicationBuilder.UseRouting();
    aApplicationBuilder.UseEndpoints
    (
      aEndpointRouteBuilder =>
      {
        aEndpointRouteBuilder.MapControllers();
        aEndpointRouteBuilder.MapBlazorHub();
        aEndpointRouteBuilder.MapFallbackToPage("/_Host");
      }
    );
    aApplicationBuilder.UseStaticFiles();
    aApplicationBuilder.UseBlazorFrameworkFiles();
  }
}
