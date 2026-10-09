#region Purpose
// Aspire AppHost that launches the telemetry sample so action spans appear in the dashboard.
#endregion

#region Design
// Top-level statements: adds the sample-04-server project (Projects.sample_04_server) to the distributed
// application and runs it.
#endregion

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.sample_04_server>("sample-04-server");
builder.Build().Run();
