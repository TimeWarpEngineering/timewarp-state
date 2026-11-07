#region Purpose
// Installs the Linux HTTPS development certificate before the end-to-end run.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include and calls it only when the suite runs over https. Trust is best
// effort: `dotnet dev-certs https --trust` exits 4 on Linux when OpenSSL trust needs SSL_CERT_DIR (GitHub runners),
// and a partial trust must not fail the run. `--clean` runs only in CI (IsCi in e2e-build.cs), because on a dev box
// it would delete the developer's own certificate. Outside CI, `--check --trust` runs first and a pass skips trust.
// Every dev-certs call gets empty stdin and a timeout, so a prompt fails fast instead of hanging. Exit codes are
// reported as warnings.
#endregion

static partial class App
{
  static readonly TimeSpan DevCertsTimeout = TimeSpan.FromMinutes(2);

  static async Task InstallLinuxDevCerts()
  {
    if (!OperatingSystem.IsLinux())
    {
      WriteLine("Skipping Linux development certificate installation (not running on Linux).");
      return;
    }

    if (IsCi())
    {
      int cleanExitCode = await RunDevCerts("--clean");
      if (cleanExitCode != 0)
      {
        WriteLine($"Warning: dev-certs clean exited with {cleanExitCode}; continuing.");
      }
    }
    else if (await RunDevCerts("--check", "--trust") == 0)
    {
      WriteLine("HTTPS development certificate is already trusted; skipping dev-certs trust.");
      return;
    }

    WriteLine("Installing Linux development certificates...");
    int trustExitCode = await RunDevCerts("--trust");
    if (trustExitCode == 0)
    {
      WriteLine("Linux development certificates installed successfully.");
    }
    else
    {
      WriteLine($"Warning: dev-certs trust exited with {trustExitCode}; continuing.");
    }
  }

  static Task<int> RunDevCerts(params string[] options) =>
    Shell.Builder("dotnet")
      .WithArguments(["dev-certs", "https", .. options])
      .WithStandardInput(string.Empty)
      .WithTimeout(DevCertsTimeout)
      .WithNoValidation()
      .RunAsync();
}
