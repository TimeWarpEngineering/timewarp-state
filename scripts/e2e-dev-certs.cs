#region Purpose
// Installs the Linux HTTPS development certificate before the end-to-end run.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include. Trust is best effort: the suite runs over http, and a partial trust must not fail the run.
#endregion

static partial class App
{
  static async Task InstallLinuxDevCerts()
{
    if (OperatingSystem.IsLinux())
    {
        WriteLine("Installing Linux development certificates...");
        // Best effort: a partially trusted cert must not fail the run.
        // `dotnet dev-certs https --trust` exits 4 on Linux when OpenSSL trust needs SSL_CERT_DIR (GitHub runners),
        // and this Amuru line throws on a non-zero exit instead of returning it.
        try
        {
            await DotNet.DevCerts().Https().WithClean().Build().RunAsync();
            var exitCode = await DotNet.DevCerts().Https().WithTrust().Build().RunAsync();
            if (exitCode == 0)
            {
                WriteLine("Linux development certificates installed successfully.");
            }
            else
            {
                WriteLine($"Warning: dev-certs trust exited with {exitCode}; continuing (E2E runs over http).");
            }
        }
        catch (Exception ex)
        {
            WriteLine($"Warning: dev-certs trust failed; continuing (E2E runs over http). {ex.Message.Split('\n')[0]}");
        }
    }
    else
    {
        WriteLine("Skipping Linux development certificate installation (not running on Linux).");
    }
  }
}
