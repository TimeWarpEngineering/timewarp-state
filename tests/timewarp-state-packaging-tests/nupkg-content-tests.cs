#region Purpose
// Opens every nupkg from dev pack and fails when a required layout entry is missing.
#endregion

#region Design
// dev pack runs this project after the nuspec allow-list. dev test does not, because the
// workflow tests before any pack and an empty feed would fail the suite for the wrong reason.
// Package id comes from the nuspec, not the prerelease file name. Zip paths are normalized to
// forward slashes. Static web assets must pair the script with its BasePath on one element or
// JSON object. A file that merely mentions both strings is not that pair. TimeWarp.State.Blazor
// keeps _content/TimeWarp.State (the SDK default would be _content/TimeWarp.State.Blazor).
// TimeWarp.State.Plus must pair both document-title.js and download-file.js with
// _content/TimeWarp.State.Plus. Either script alone is a failure.
// JSON pairing reads static web asset manifests only: build/, buildTransitive/, and
// names ending in .PackageAssets.json. contentFiles tsconfig is JSONC and is not parsed.
// Analyzer and source-generator assemblies are packed inside TimeWarp.State under analyzers/dotnet/cs.
// The Blazor module is fingerprinted in the nupkg (a hash sits before .lib.module.js), so the
// assertion is that suffix plus BasePath _content/TimeWarp.State on the same asset object.
#endregion

namespace TimeWarp.State.Packaging.Tests;

public class NupkgContent_Should_
{
  private const string BlazorModuleScript = ".lib.module.js";
  private const string BlazorContentBase = "_content/TimeWarp.State";
  private const string PlusContentBase = "_content/TimeWarp.State.Plus";
  private const string PlusDocumentTitleScript = "document-title.js";
  private const string PlusDownloadFileScript = "download-file.js";

  public static void MatchTheLayoutOfEveryPackedNupkg()
  {
    string repoRoot = FindRepoRoot();
    string feed = Path.Combine(repoRoot, "artifacts", "packages");
    Directory.Exists(feed).ShouldBeTrue($"dev pack did not create {feed}.");

    string[] nupkgPaths = Directory
      .GetFiles(feed, "*.nupkg")
      .Where(path => path.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
      .OrderBy(path => path, StringComparer.Ordinal)
      .ToArray();

    nupkgPaths.ShouldNotBeEmpty("dev pack produced no .nupkg files.");

    foreach (string nupkgPath in nupkgPaths)
    {
      using ZipArchive archive = ZipFile.OpenRead(nupkgPath);
      string packageId = ReadPackageId(archive);
      string[] entries = archive.Entries
        .Select(entry => Normalize(entry.FullName))
        .Where(path => path.Length > 0 && !path.EndsWith('/'))
        .ToArray();

      AssertLayout(packageId, archive, entries);
    }
  }

  public static void IgnoreACommentInContentFilesTsconfig()
  {
    using ZipArchive archive = Archive(
      ("contentFiles/any/net11.0/tsconfig.json", """{ /* comment */ "compilerOptions": {} }"""));

    HasAssetPair(archive, PlusDocumentTitleScript, PlusContentBase).ShouldBeFalse();
  }

  public static void PairAManifestWhenThePackageAlsoContainsJsonc()
  {
    using ZipArchive archive = Archive(
      ("contentFiles/any/net11.0/tsconfig.json", """{ /* comment */ "compilerOptions": {} }"""),
      (
        "build/TimeWarp.State.Plus.PackageAssets.json",
        """
        {
          "Assets": {
            "staticwebassets/js/document-title.js": {
              "RelativePath": "js/document-title.js",
              "BasePath": "_content/TimeWarp.State.Plus"
            }
          }
        }
        """));

    HasAssetPair(archive, PlusDocumentTitleScript, PlusContentBase).ShouldBeTrue();
  }

  public static void FailPlusWhenDownloadFileIsMissing()
  {
    using ZipArchive archive = Archive(
      ("staticwebassets/js/document-title.js", "script"),
      ("lib/net11.0/timewarp-state-plus.dll", "dll"),
      (
        "build/TimeWarp.State.Plus.PackageAssets.json",
        """
        {
          "Assets": {
            "staticwebassets/js/document-title.js": {
              "RelativePath": "js/document-title.js",
              "BasePath": "_content/TimeWarp.State.Plus"
            }
          }
        }
        """));
    string[] entries = archive.Entries
      .Select(entry => Normalize(entry.FullName))
      .Where(path => path.Length > 0 && !path.EndsWith('/'))
      .ToArray();

    Should.Throw<ShouldAssertException>(() => AssertLayout("TimeWarp.State.Plus", archive, entries));
  }

  private static ZipArchive Archive(params (string Path, string Text)[] files)
  {
    MemoryStream stream = new();
    using (ZipArchive writing = new(stream, ZipArchiveMode.Create, leaveOpen: true))
    {
      foreach ((string path, string text) in files)
      {
        ZipArchiveEntry entry = writing.CreateEntry(path);
        using Stream entryStream = entry.Open();
        using StreamWriter writer = new(entryStream);
        writer.Write(text);
      }
    }

    stream.Position = 0;
    return new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
  }

  private static void AssertLayout(string packageId, ZipArchive archive, string[] entries)
  {
    switch (packageId)
    {
      case "TimeWarp.State":
        entries.ShouldContain("analyzers/dotnet/cs/timewarp-state-analyzer.dll");
        entries.ShouldContain("analyzers/dotnet/cs/timewarp-state-source-generator.dll");
        entries.ShouldContain("lib/net11.0/TimeWarp.State.dll");
        break;

      case "TimeWarp.State.Blazor":
        AssertStaticWebAsset(packageId, archive, entries, BlazorModuleScript, BlazorContentBase);
        entries.ShouldContain("lib/net11.0/TimeWarp.State.Blazor.dll");
        break;

      case "TimeWarp.State.Plus":
        AssertStaticWebAsset(packageId, archive, entries, PlusDocumentTitleScript, PlusContentBase);
        AssertStaticWebAsset(packageId, archive, entries, PlusDownloadFileScript, PlusContentBase);
        entries.ShouldContain("lib/net11.0/timewarp-state-plus.dll");
        break;

      case "TimeWarp.State.DevTools.App":
        entries.ShouldContain("tools/wwwroot/logo.png");
        (
          entries.Contains("build/TimeWarp.State.DevTools.App.targets")
          || entries.Contains("buildTransitive/TimeWarp.State.DevTools.App.targets")
        ).ShouldBeTrue($"{packageId} is missing its build targets.");
        entries.Any(entry => entry.StartsWith("lib/", StringComparison.Ordinal))
          .ShouldBeFalse($"{packageId} sets IncludeBuildOutput=false and must not ship lib/.");
        break;

      case "TimeWarp.State.Policies":
      case "TimeWarp.State.Telemetry":
      case "TimeWarp.State.DevTools.Hosting":
      case "TimeWarp.State.DevTools.Export":
      case "TimeWarp.State.DevTools.Wasm":
        entries.ShouldContain($"lib/net11.0/{packageId}.dll");
        break;

      default:
        throw new InvalidOperationException($"no expected layout for {packageId}");
    }
  }

  private static void AssertStaticWebAsset(
    string packageId,
    ZipArchive archive,
    string[] entries,
    string scriptName,
    string basePath)
  {
    entries.Any(entry =>
      entry.Contains("staticwebassets/", StringComparison.Ordinal)
      && entry.EndsWith(scriptName, StringComparison.Ordinal))
      .ShouldBeTrue($"{packageId} is missing a staticwebassets entry ending in {scriptName}.");
    HasAssetPair(archive, scriptName, basePath)
      .ShouldBeTrue(PairFailure(packageId, scriptName, basePath, archive));
  }

  private static string PairFailure(string packageId, string scriptName, string basePath, ZipArchive archive)
  {
    string bases = string.Join(", ", ReadBasePaths(archive).Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal));
    if (bases.Length == 0)
    {
      bases = "(none)";
    }

    return $"{packageId} has no {scriptName} asset paired with BasePath {basePath}. Base paths seen: {bases}.";
  }

  private static bool HasAssetPair(ZipArchive archive, string scriptName, string basePath)
  {
    foreach (ZipArchiveEntry entry in archive.Entries)
    {
      string name = Normalize(entry.FullName);
      if (IsStaticWebAssetManifest(name))
      {
        using Stream stream = entry.Open();
        using JsonDocument document = JsonDocument.Parse(stream);
        if (JsonHasPair(document.RootElement, scriptName, basePath))
        {
          return true;
        }
      }
      else if (name.EndsWith(".props", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".targets", StringComparison.OrdinalIgnoreCase))
      {
        using Stream stream = entry.Open();
        XDocument document = XDocument.Load(stream);
        if (XmlHasPair(document, scriptName, basePath))
        {
          return true;
        }
      }
    }

    return false;
  }

  private static bool IsStaticWebAssetManifest(string name)
  {
    if (!name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    if (name.EndsWith(".PackageAssets.json", StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    int slash = name.LastIndexOf('/');
    string directory = slash < 0 ? "" : name[..slash];
    return directory.Equals("build", StringComparison.OrdinalIgnoreCase)
      || directory.Equals("buildTransitive", StringComparison.OrdinalIgnoreCase)
      || directory.EndsWith("/build", StringComparison.OrdinalIgnoreCase)
      || directory.EndsWith("/buildTransitive", StringComparison.OrdinalIgnoreCase);
  }

  private static bool JsonHasPair(JsonElement element, string scriptName, string basePath)
  {
    switch (element.ValueKind)
    {
      case JsonValueKind.Object:
        if (ObjectProvesPair(element, scriptName, basePath))
        {
          return true;
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
          if (JsonHasPair(property.Value, scriptName, basePath))
          {
            return true;
          }
        }

        return false;

      case JsonValueKind.Array:
        foreach (JsonElement item in element.EnumerateArray())
        {
          if (JsonHasPair(item, scriptName, basePath))
          {
            return true;
          }
        }

        return false;

      default:
        return false;
    }
  }

  private static bool ObjectProvesPair(JsonElement element, string scriptName, string basePath)
  {
    string? relativePath = ReadString(element, "RelativePath") ?? ReadString(element, "relativePath");
    string? declaredBase = ReadString(element, "BasePath") ?? ReadString(element, "basePath");
    if (relativePath is not null
      && declaredBase is not null
      && Normalize(relativePath).EndsWith(scriptName, StringComparison.Ordinal)
      && string.Equals(Normalize(declaredBase), basePath, StringComparison.Ordinal))
    {
      return true;
    }

    bool sawScript = false;
    bool sawBase = false;
    foreach (JsonProperty property in element.EnumerateObject())
    {
      if (property.Value.ValueKind != JsonValueKind.String)
      {
        continue;
      }

      string value = Normalize(property.Value.GetString() ?? "");
      if (value.EndsWith(scriptName, StringComparison.Ordinal) && value.StartsWith(basePath + "/", StringComparison.Ordinal))
      {
        return true;
      }

      if (value.EndsWith(scriptName, StringComparison.Ordinal))
      {
        sawScript = true;
      }

      if (string.Equals(value, basePath, StringComparison.Ordinal))
      {
        sawBase = true;
      }
    }

    return sawScript && sawBase;
  }

  private static string? ReadString(JsonElement element, string name)
  {
    if (!element.TryGetProperty(name, out JsonElement property) || property.ValueKind != JsonValueKind.String)
    {
      return null;
    }

    return property.GetString();
  }

  private static bool XmlHasPair(XDocument document, string scriptName, string basePath)
  {
    foreach (XElement element in document.Descendants())
    {
      string? relativePath = ChildOrAttribute(element, "RelativePath");
      string? declaredBase = ChildOrAttribute(element, "BasePath");
      if (relativePath is null || declaredBase is null)
      {
        continue;
      }

      if (Normalize(relativePath).EndsWith(scriptName, StringComparison.Ordinal)
        && string.Equals(Normalize(declaredBase), basePath, StringComparison.Ordinal))
      {
        return true;
      }
    }

    return false;
  }

  private static string? ChildOrAttribute(XElement element, string localName)
  {
    string? attribute = element.Attributes().FirstOrDefault(candidate => candidate.Name.LocalName == localName)?.Value;
    if (!string.IsNullOrWhiteSpace(attribute))
    {
      return attribute;
    }

    return element.Elements().FirstOrDefault(candidate => candidate.Name.LocalName == localName)?.Value;
  }

  private static IEnumerable<string> ReadBasePaths(ZipArchive archive)
  {
    foreach (ZipArchiveEntry entry in archive.Entries)
    {
      string name = Normalize(entry.FullName);
      if (name.EndsWith(".props", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".targets", StringComparison.OrdinalIgnoreCase))
      {
        using Stream stream = entry.Open();
        XDocument document = XDocument.Load(stream);
        foreach (XElement element in document.Descendants())
        {
          string? declaredBase = ChildOrAttribute(element, "BasePath");
          if (!string.IsNullOrWhiteSpace(declaredBase))
          {
            yield return Normalize(declaredBase);
          }
        }
      }
    }
  }

  private static string ReadPackageId(ZipArchive archive)
  {
    ZipArchiveEntry? nuspec = archive.Entries.FirstOrDefault(entry =>
      entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
    nuspec.ShouldNotBeNull();
    using Stream stream = nuspec.Open();
    XDocument document = XDocument.Load(stream);
    string? packageId = document.Descendants().FirstOrDefault(element => element.Name.LocalName == "id")?.Value.Trim();
    packageId.ShouldNotBeNullOrWhiteSpace();
    return packageId;
  }

  private static string FindRepoRoot()
  {
    DirectoryInfo? directory = new(AppContext.BaseDirectory);
    while (directory is not null)
    {
      if (File.Exists(Path.Combine(directory.FullName, "timewarp-state.slnx")))
      {
        return directory.FullName;
      }

      directory = directory.Parent;
    }

    throw new InvalidOperationException("Could not find timewarp-state.slnx walking up from the test assembly.");
  }

  private static string Normalize(string path) => path.Replace('\\', '/');
}
