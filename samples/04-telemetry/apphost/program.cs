#region Purpose
// Aspire AppHost that launches the telemetry sample so action spans appear in the dashboard.
#endregion

#region Design
// Adds the sample server and the DevTools Blazor app. WithTimeWarpStateDevTools publishes the
// DevTools origin so the server can export TimeWarp.State activities to the inspector.
#endregion

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
builder.AddProject<Projects.sample_04_server>("sample-04-server")
  .WithTimeWarpStateDevTools(devtools);
builder.Build().Run();
