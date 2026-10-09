#region Purpose
// Server host for the Auto render mode sample (interactive Server and WebAssembly).
#endregion

#region Design
// Registers Razor components with both interactive render modes and calls the client
// Program.ConfigureServices so server-rendered components get the same TimeWarp.State services. Maps App
// with the client assembly added so its pages are routable.
#endregion

namespace Sample00Auto;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
        
        Sample00Auto.Client.Program.ConfigureServices(builder.Services); // <=== Add this line.

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client.Program).Assembly);

        app.Run();
    }
}
