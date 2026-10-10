#region Purpose
// Allow-list of NuGet dependency ids that packed TimeWarp.State packages may declare.
#endregion

#region Design
// Direct nuspec dependencies are the consumer restore graph. Roslyn and JetBrains.Annotations are
// compile-time only and must not appear. A package id with no entry fails closed. The check compares
// ids only, so a version bump of an allowed dependency stays green. dev pack reads each nupkg; the
// unit tests feed the same check a nuspec that adds a dependency outside the list.
#endregion

namespace DevCli;

internal static class NuspecDependencyAllowList
{
  internal static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> AllowedByPackageId =
    new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
    {
      ["TimeWarp.State"] = new HashSet<string>(StringComparer.Ordinal)
      {
        "TimeWarp.Mediator.Contracts",
      },
      ["TimeWarp.State.Blazor"] = new HashSet<string>(StringComparer.Ordinal)
      {
        "Microsoft.AspNetCore.Components.Web",
        "TimeWarp.State",
      },
      ["TimeWarp.State.Plus"] = new HashSet<string>(StringComparer.Ordinal)
      {
        "Blazored.LocalStorage",
        "Blazored.SessionStorage",
        "Microsoft.AspNetCore.Components.Web",
        "TimeWarp.State",
        "TimeWarp.State.Blazor",
      },
      ["TimeWarp.State.Telemetry"] = new HashSet<string>(StringComparer.Ordinal)
      {
        "TimeWarp.Mediator.Contracts",
        "TimeWarp.State",
      },
      ["TimeWarp.State.Policies"] = new HashSet<string>(StringComparer.Ordinal)
      {
        "NetArchTest.eNhancedEdition",
        "Shouldly",
        "TimeWarp.State",
      },
    };

  internal static IReadOnlyList<string> ReadDependencyIds(string nuspecXml) =>
    ReadDependencyIds(System.Xml.Linq.XDocument.Parse(nuspecXml));

  internal static IReadOnlyList<string> ReadDependencyIds(System.Xml.Linq.XDocument document)
  {
    return document
      .Descendants()
      .Where(element => element.Name.LocalName == "dependency")
      .Select(element => (string?)element.Attribute("id"))
      .Where(id => !string.IsNullOrWhiteSpace(id))
      .Select(id => id!)
      .Distinct(StringComparer.Ordinal)
      .OrderBy(id => id, StringComparer.Ordinal)
      .ToArray();
  }

  internal static IReadOnlyList<string> FindDisallowed(string packageId, IEnumerable<string> dependencyIds)
  {
    if (!AllowedByPackageId.TryGetValue(packageId, out IReadOnlySet<string>? allowed))
    {
      return [$"<no allow-list for {packageId}>"];
    }

    return dependencyIds
      .Where(id => !allowed.Contains(id))
      .Distinct(StringComparer.Ordinal)
      .OrderBy(id => id, StringComparer.Ordinal)
      .ToArray();
  }

  internal static IReadOnlyList<string> ReadDependencyIdsFromNupkg(string nupkgPath)
  {
    using System.IO.Compression.ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(nupkgPath);
    System.IO.Compression.ZipArchiveEntry? entry = archive.Entries.FirstOrDefault(candidate =>
      candidate.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase)
      && !candidate.FullName.Contains('/', StringComparison.Ordinal));

    if (entry is null)
    {
      throw new InvalidOperationException($"No nuspec in {nupkgPath}.");
    }

    using Stream stream = entry.Open();
    System.Xml.Linq.XDocument document = System.Xml.Linq.XDocument.Load(stream);
    return ReadDependencyIds(document);
  }
}
