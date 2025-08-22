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
