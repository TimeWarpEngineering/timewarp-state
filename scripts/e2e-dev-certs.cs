#region Purpose
// Installs the Linux HTTPS development certificate before the end-to-end run.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include and calls it only when the suite runs over https. Trust is best
// effort: `dotnet dev-certs https --trust` exits 4 on Linux when OpenSSL trust needs SSL_CERT_DIR (GitHub runners),
// and a partial trust must not fail the run. Both the clean and the trust exit codes are reported.
#endregion

static partial class App
{
  static async Task InstallLinuxDevCerts()
  {
    if (!OperatingSystem.IsLinux())
    {
      WriteLine("Skipping Linux development certificate installation (not running on Linux).");
      return;
    }

    WriteLine("Installing Linux development certificates...");
    int cleanExitCode = await DotNet.DevCerts().Https().WithClean().Build().RunAsync();
    if (cleanExitCode != 0)
    {
      WriteLine($"Warning: dev-certs clean exited with {cleanExitCode}; continuing.");
    }

    int trustExitCode = await DotNet.DevCerts().Https().WithTrust().Build().RunAsync();
    if (trustExitCode == 0)
    {
      WriteLine("Linux development certificates installed successfully.");
    }
    else
    {
      WriteLine($"Warning: dev-certs trust exited with {trustExitCode}; continuing.");
    }
  }
}
