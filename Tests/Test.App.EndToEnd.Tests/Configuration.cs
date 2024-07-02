namespace Test.App.EndToEnd.Tests;

public static class Configuration
{
  public static string GetSutBaseUrl()
  {
    string port = Environment.GetEnvironmentVariable("SutPort") ?? "7011";
    string sutBaseHttpsUrl = $"https://localhost:{port}/";
    return sutBaseHttpsUrl;
  }
}
