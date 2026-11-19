#region Purpose
// Reads the TimeWarp.State package version and checks that the version CPM pins is derived from it.
#endregion

#region Design
// source/Directory.Build.props <Version> is the one literal version: pack, check-version and release read it.
// msbuild/repository.props derives <TimeWarpStateVersion> (the CPM pin for TimeWarp.State and
// TimeWarp.State.Blazor, used by sample 00 auto) from that element at evaluation time. AssertVersionSsot reads
// the literal, evaluates repository.props standalone with `dotnet msbuild -getProperty:TimeWarpStateVersion`,
// and returns the version when the derived value is non-empty and equal. Otherwise it prints why and returns
// null. A malformed props file prints a clear "not well-formed XML" error and reads as missing.
// why: task 165, one version literal (Steve 2026-11-19)
#endregion

namespace DevCli.Commands;

internal sealed class RepoVersionService
{
  private readonly ITerminal Terminal;
  private readonly string RepoRoot;

  public RepoVersionService(ITerminal terminal, string repoRoot)
  {
    Terminal = terminal;
    RepoRoot = repoRoot;
  }

  private string SourcePropsPath => Path.Combine(RepoRoot, "source", "Directory.Build.props");

  private string RepositoryPropsPath => Path.Combine(RepoRoot, "msbuild", "repository.props");

  public async Task<string?> AssertVersionSsot(CancellationToken ct)
  {
    string? packVersion = TryReadMsBuildProperty(SourcePropsPath, "Version");
    if (packVersion is null)
    {
      Terminal.WriteErrorLine("Version SSOT: source/Directory.Build.props has no <Version> value.".Red());
      return null;
    }

    CommandOutput evaluation = await DotNet.Builder()
      .WithArguments("msbuild", RepositoryPropsPath, "-nologo", "-getProperty:TimeWarpStateVersion")
      .WithNoValidation()
      .CaptureAsync(ct);
    string derivedVersion = evaluation.Success ? evaluation.Stdout.Trim() : string.Empty;

    if (string.Equals(packVersion, derivedVersion, StringComparison.Ordinal))
    {
      Terminal.WriteLine($"Version SSOT: {packVersion} (msbuild/repository.props TimeWarpStateVersion is derived from source Version)");
      return packVersion;
    }

    if (!evaluation.Success)
    {
      Terminal.WriteErrorLine(evaluation.Combined);
    }

    string derivedDisplay = derivedVersion.Length == 0 ? "(empty)" : derivedVersion;
    Terminal.WriteErrorLine(
      $"Version SSOT: source/Directory.Build.props Version='{packVersion}', but msbuild/repository.props evaluates TimeWarpStateVersion='{derivedDisplay}'. TimeWarpStateVersion must be read from <Version>, not typed.".Red());
    return null;
  }

  private string? TryReadMsBuildProperty(string propsPath, string propertyName)
  {
    if (!File.Exists(propsPath))
    {
      return null;
    }

    XDocument document;
    try
    {
      document = XDocument.Load(propsPath);
    }
    catch (System.Xml.XmlException exception)
    {
      Terminal.WriteErrorLine($"Error: {propsPath} is not well-formed XML (line {exception.LineNumber}): {exception.Message}".Red());
      return null;
    }

    XElement? element = document
      .Descendants()
      .FirstOrDefault(candidate => string.Equals(candidate.Name.LocalName, propertyName, StringComparison.Ordinal));
    string? value = element?.Value.Trim();
    return string.IsNullOrWhiteSpace(value) ? null : value;
  }
}
