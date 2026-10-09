#region Purpose
// Builds the base URL of the system under test for the E2E tests.
#endregion

#region Design
// Port comes from SutPort (default 7011) and the scheme from UseHttp ("true" means http), so CI and local runs can
// point at different hosts. Logs the values it used.
#endregion

namespace Test.App.EndToEnd.Tests;

public static class Configuration
{
  public static string GetSutBaseUrl()
  {
    string port = Environment.GetEnvironmentVariable("SutPort") ?? "7011";
    string? useHttpEnv = Environment.GetEnvironmentVariable("UseHttp");
    Console.WriteLine($"DEBUG: UseHttp environment variable = '{useHttpEnv}'");
    string protocol = useHttpEnv == "true" ? "http" : "https";
    string sutBaseUrl = $"{protocol}://localhost:{port}";
    Console.WriteLine($"DEBUG: Using base URL = {sutBaseUrl}");
    return sutBaseUrl;
  }
}
