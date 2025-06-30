namespace Test.App.EndToEnd.Tests;

public static class Configuration
{
  public static string GetSutBaseUrl()
  {
    string port = Environment.GetEnvironmentVariable("SutPort") ?? "7011";
    string protocol = Environment.GetEnvironmentVariable("UseHttp") == "true" ? "http" : "https";
    string sutBaseUrl = $"{protocol}://localhost:{port}";
    return sutBaseUrl;
  }
}
