#region Purpose
// Keeps local-feed restores out of the machine-wide NuGet global-packages folder.
#endregion

#region Design
// artifacts/nuget-packages is the only packages folder this repo restores into.
// nuget.config globalPackagesFolder and RestorePackagesPath both name it.
// Apply sets NUGET_PACKAGES for child dotnet processes that do not read the MSBuild property.
// The first Apply wins so a later call still remembers a shared cache the process inherited.
// The pack guard reads that shared cache (the default ~/.nuget/packages, plus a distinct
// inherited NUGET_PACKAGES) and never the isolated folder: a local-feed extract there is expected.
// A source that does not start with http:// or https:// is a local path.
#endregion

namespace DevCli;

internal static class NuGetPackageIsolation
{
  private static Isolation? Applied;

  internal readonly record struct Isolation(string IsolatedPackagesFolder, string? PreviousNuGetPackages);

  internal readonly record struct PoisonedCacheEntry(
    string PackageId,
    string Version,
    string MetadataPath,
    string Source);

  internal static Isolation Apply(string repoRoot)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(repoRoot);
    string isolated = IsolatedPackagesFolder(repoRoot);
    if (Applied is Isolation existing && PathsEqual(existing.IsolatedPackagesFolder, isolated))
    {
      return existing;
    }

    string? previous = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
    Directory.CreateDirectory(isolated);
    Environment.SetEnvironmentVariable("NUGET_PACKAGES", isolated);
    Isolation isolation = new(isolated, previous);
    Applied = isolation;
    return isolation;
  }

  internal static string MachineGlobalPackagesFolder()
  {
    string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    return Path.Combine(profile, ".nuget", "packages");
  }

  internal static IReadOnlyList<string> SharedCacheFolders(Isolation isolation)
  {
    List<string> folders = [];
    AddSharedFolder(folders, MachineGlobalPackagesFolder(), isolation.IsolatedPackagesFolder);
    if (!string.IsNullOrWhiteSpace(isolation.PreviousNuGetPackages))
    {
      AddSharedFolder(
        folders,
        Path.GetFullPath(isolation.PreviousNuGetPackages),
        isolation.IsolatedPackagesFolder);
    }

    return folders;
  }

  internal static IReadOnlyList<PoisonedCacheEntry> FindPoisonedEntries(
    IEnumerable<string> globalPackagesFolders,
    string version,
    IEnumerable<string> packageIds)
  {
    List<PoisonedCacheEntry> found = [];
    foreach (string folder in globalPackagesFolders)
    {
      found.AddRange(FindPoisonedEntries(folder, version, packageIds));
    }

    return found;
  }

  internal static IReadOnlyList<PoisonedCacheEntry> FindPoisonedEntries(
    string globalPackagesFolder,
    string version,
    IEnumerable<string> packageIds)
  {
    if (string.IsNullOrWhiteSpace(version) || !Directory.Exists(globalPackagesFolder))
    {
      return [];
    }

    List<PoisonedCacheEntry> found = [];
    foreach (string packageId in packageIds.Distinct(StringComparer.Ordinal))
    {
      if (string.IsNullOrWhiteSpace(packageId))
      {
        continue;
      }

      string? versionFolder = FindVersionFolder(
        Path.Combine(globalPackagesFolder, packageId.ToLowerInvariant()),
        version);
      if (versionFolder is null)
      {
        continue;
      }

      string metadataPath = Path.Combine(versionFolder, ".nupkg.metadata");
      if (!File.Exists(metadataPath))
      {
        continue;
      }

      if (!TryReadSource(metadataPath, out string source))
      {
        found.Add(new PoisonedCacheEntry(packageId, version, metadataPath, source));
        continue;
      }

      if (!IsRemotePackageSource(source))
      {
        found.Add(new PoisonedCacheEntry(packageId, version, metadataPath, source));
      }
    }

    return found;
  }

  internal static bool IsRemotePackageSource(string source)
  {
    if (string.IsNullOrWhiteSpace(source))
    {
      return false;
    }

    return source.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
      || source.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
  }

  internal static string? ReadProductVersion(string repoRoot)
  {
    string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");
    if (!File.Exists(propsPath))
    {
      return null;
    }

    System.Xml.Linq.XDocument document = System.Xml.Linq.XDocument.Load(propsPath);
    string? value = document.Descendants("Version").FirstOrDefault()?.Value.Trim();
    return string.IsNullOrWhiteSpace(value) ? null : value;
  }

  internal static string? ReadPackageId(string projectPath)
  {
    System.Xml.Linq.XDocument document = System.Xml.Linq.XDocument.Load(projectPath);
    string? packageId = document.Descendants("PackageId").FirstOrDefault()?.Value.Trim();
    return string.IsNullOrWhiteSpace(packageId) ? null : packageId;
  }

  internal static string FormatFailure(PoisonedCacheEntry entry)
  {
    return
      $"Global NuGet cache entry for {entry.PackageId} {entry.Version} came from a local source."
      + $"{Environment.NewLine}  {entry.MetadataPath}"
      + $"{Environment.NewLine}  source: {entry.Source}"
      + $"{Environment.NewLine}  Delete that version folder so other repos do not restore the local pack.";
  }

  internal static int ReportPoisonedEntries(
    IReadOnlyList<PoisonedCacheEntry> entries,
    Action<string> writeError)
  {
    if (entries.Count == 0)
    {
      return 0;
    }

    foreach (PoisonedCacheEntry entry in entries)
    {
      writeError(FormatFailure(entry));
    }

    return 1;
  }

  private static string IsolatedPackagesFolder(string repoRoot)
  {
    return Path.GetFullPath(Path.Combine(repoRoot, "artifacts", "nuget-packages"));
  }

  private static void AddSharedFolder(List<string> folders, string candidate, string isolated)
  {
    string full = Path.GetFullPath(candidate);
    if (PathsEqual(full, isolated))
    {
      return;
    }

    foreach (string existing in folders)
    {
      if (PathsEqual(existing, full))
      {
        return;
      }
    }

    folders.Add(full);
  }

  private static string? FindVersionFolder(string packageFolder, string version)
  {
    if (!Directory.Exists(packageFolder))
    {
      return null;
    }

    foreach (string candidate in Directory.EnumerateDirectories(packageFolder))
    {
      if (string.Equals(Path.GetFileName(candidate), version, StringComparison.OrdinalIgnoreCase))
      {
        return candidate;
      }
    }

    return null;
  }

  private static bool TryReadSource(string metadataPath, out string source)
  {
    try
    {
      using FileStream stream = File.OpenRead(metadataPath);
      using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(stream);
      if (!document.RootElement.TryGetProperty("source", out System.Text.Json.JsonElement sourceProperty)
        || sourceProperty.ValueKind != System.Text.Json.JsonValueKind.String)
      {
        source = "(metadata has no string source)";
        return false;
      }

      source = sourceProperty.GetString() ?? "";
      return true;
    }
    catch (Exception exception) when (exception is System.Text.Json.JsonException or IOException or UnauthorizedAccessException)
    {
      source = $"(unreadable metadata: {exception.Message})";
      return false;
    }
  }

  private static bool PathsEqual(string left, string right)
  {
    StringComparison comparison = OperatingSystem.IsWindows()
      ? StringComparison.OrdinalIgnoreCase
      : StringComparison.Ordinal;
    return string.Equals(TrimPath(left), TrimPath(right), comparison);
  }

  private static string TrimPath(string path)
  {
    return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
  }
}
