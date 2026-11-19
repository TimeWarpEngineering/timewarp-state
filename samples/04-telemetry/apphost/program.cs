#region Purpose
// Aspire AppHost that launches the telemetry sample so action spans appear in the dashboard.
#endregion

#region Design
// Adds the sample server with AddDotnetProject (path relative to this AppHost) and the DevTools
// Blazor app. WithTimeWarpStateDevTools publishes the DevTools origin so the server can export
// TimeWarp.State activities to the inspector.
#endregion

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
builder.AddDotnetProject("sample-04-server", "../server/sample-04-server/sample-04-server.csproj")
  .WithTimeWarpStateDevTools(devtools);
builder.Build().Run();
