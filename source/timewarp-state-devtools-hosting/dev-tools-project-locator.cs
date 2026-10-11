#region Purpose
// Finds the DevTools Blazor project for AddProject.
#endregion

#region Design
// Walks parents of the AppHost directory for the in-repo layout. TIMEWARP_STATE_DEVTOOLS_PROJECT
// is the out-of-tree override. The DevTools app is not packed inside this package: Aspire builds
// the csproj it is given.
#endregion

namespace TimeWarp.State.DevTools.Hosting;

/// <summary>
/// Locates <c>timewarp-state-devtools.csproj</c>.
/// </summary>
public static class DevToolsProjectLocator
{
  /// <summary>Project file name.</summary>
  public const string ProjectFileName = "timewarp-state-devtools.csproj";

  /// <summary>Path relative to the repository root.</summary>
  public const string RelativeProjectPath = "source/timewarp-state-devtools/timewarp-state-devtools.csproj";

  /// <summary>
  /// Absolute path to the DevTools csproj. When set, the directory walk is skipped.
  /// </summary>
  public const string ProjectPathEnvironmentVariable = "TIMEWARP_STATE_DEVTOOLS_PROJECT";

  /// <summary>
  /// Resolves the DevTools project file.
  /// </summary>
  /// <param name="appHostDirectory">Aspire AppHost directory.</param>
  /// <returns>Absolute path to the csproj.</returns>
  /// <exception cref="InvalidOperationException">The project file cannot be found.</exception>
  public static string Resolve(string appHostDirectory)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(appHostDirectory);

    string? fromEnvironment = Environment.GetEnvironmentVariable(ProjectPathEnvironmentVariable);
    if (!string.IsNullOrWhiteSpace(fromEnvironment))
    {
      string fullPath = Path.GetFullPath(fromEnvironment);
      if (!File.Exists(fullPath))
      {
        throw new InvalidOperationException
        (
          $"{ProjectPathEnvironmentVariable} does not point at a file: '{fullPath}'."
        );
      }

      return fullPath;
    }

    DirectoryInfo? directory = new(Path.GetFullPath(appHostDirectory));
    while (directory is not null)
    {
      string candidate = Path.Combine(directory.FullName, RelativeProjectPath);
      if (File.Exists(candidate))
      {
        return Path.GetFullPath(candidate);
      }

      directory = directory.Parent;
    }

    throw new InvalidOperationException
    (
      $"Cannot find '{RelativeProjectPath}' by walking up from '{appHostDirectory}'. " +
      $"Set {ProjectPathEnvironmentVariable} to the DevTools csproj."
    );
  }
}
