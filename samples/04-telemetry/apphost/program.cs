#region Purpose
// Aspire AppHost that launches the telemetry sample so action spans appear in the dashboard.
#endregion

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.sample_04_server>("sample-04-server");
builder.Build().Run();
