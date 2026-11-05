#region Purpose
// Reads the TimeWarp.State version from the MSBuild props files and asserts the two copies agree.
#endregion

#region Design
// source/Directory.Build.props <Version> drives pack and check-version; msbuild/repository.props
// <TimeWarpStateVersion> drives CPM and the samples. AssertVersionSsot returns the aligned version, or null
// after printing why. A malformed props file prints a clear "not well-formed XML" error and reads as missing.
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

  public string? AssertVersionSsot()
  {
    string? packVersion = TryReadMsBuildProperty(SourcePropsPath, "Version");
    string? cpmVersion = TryReadMsBuildProperty(RepositoryPropsPath, "TimeWarpStateVersion");

    if (packVersion is not null
        && cpmVersion is not null
        && string.Equals(packVersion, cpmVersion, StringComparison.Ordinal))
    {
      Terminal.WriteLine($"Version SSOT aligned: {packVersion} (source Version == TimeWarpStateVersion)");
      return packVersion;
    }

    string packDisplay = packVersion ?? "(missing)";
    string cpmDisplay = cpmVersion ?? "(missing)";
    Terminal.WriteErrorLine(
      $"Version SSOT mismatch: source/Directory.Build.props Version='{packDisplay}', msbuild/repository.props TimeWarpStateVersion='{cpmDisplay}'. Align both.".Red());
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
