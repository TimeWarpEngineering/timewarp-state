#region Purpose
// Proves the packed-package dependency allow-list rejects Roslyn and JetBrains.Annotations, and that
// every public PackageReference of a packable project is on that project's list.
#endregion

#region Design
// The checker is the same type dev pack runs against nupkgs. These tests feed it dependency ids and a
// nuspec document so a regression fails in the test run, without packing. The Roslyn pin assertion
// keeps the analyzer and generator on the 5.12.0 consumer compiler floor, the Roslyn bundled in .NET SDK 11.0.100
// (GA), the first SDK that can build the net11.0 packages without preview support (task 157).
#endregion

namespace TimeWarp.State.Packaging;

public class NuspecDependencyAllowList_Should_
{
  public static void AcceptEachPackagesAllowList()
  {
    foreach ((string packageId, IReadOnlySet<string> allowed) in DevCli.NuspecDependencyAllowList.AllowedByPackageId)
    {
      IReadOnlyList<string> disallowed = DevCli.NuspecDependencyAllowList.FindDisallowed(packageId, allowed);
      disallowed.ShouldBeEmpty();
    }
  }

  public static void AllowEachPublicPackageReferenceOfEveryPackableProject()
  {
    string sourceDirectory = Path.Combine(FindRepoRoot(), "source");
    foreach (string projectPath in Directory.GetFiles(sourceDirectory, "*.csproj", SearchOption.AllDirectories))
    {
      System.Xml.Linq.XDocument project = System.Xml.Linq.XDocument.Load(projectPath);
      string? packageId = project
        .Descendants()
        .Where(element => element.Name.LocalName == "PackageId")
        .Select(element => element.Value.Trim())
        .FirstOrDefault();
      bool isPackable = project
        .Descendants()
        .Any(element => element.Name.LocalName == "IsPackable"
          && string.Equals(element.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase));
      if (packageId is null || !isPackable)
      {
        continue;
      }

      string[] publicPackageIds =
      [
        .. project
          .Descendants()
          .Where(element => element.Name.LocalName == "PackageReference" && !IsPrivate(element))
          .Select(element => (string?)element.Attribute("Include"))
          .Where(id => !string.IsNullOrWhiteSpace(id))
          .Select(id => id!),
      ];

      IReadOnlyList<string> disallowed = DevCli.NuspecDependencyAllowList.FindDisallowed(packageId, publicPackageIds);
      disallowed.ShouldBeEmpty($"{packageId} ({Path.GetFileName(projectPath)})");
    }
  }

  public static void RejectRoslynAndJetBrainsAnnotationsOnEveryPackage()
  {
    foreach (string packageId in DevCli.NuspecDependencyAllowList.AllowedByPackageId.Keys)
    {
      IReadOnlySet<string> allowed = DevCli.NuspecDependencyAllowList.AllowedByPackageId[packageId];
      string[] withCompileOnlyPackages =
      [
        .. allowed,
        "Microsoft.CodeAnalysis.CSharp",
        "JetBrains.Annotations",
      ];

      IReadOnlyList<string> disallowed = DevCli.NuspecDependencyAllowList.FindDisallowed(packageId, withCompileOnlyPackages);
      disallowed.ShouldBe(["JetBrains.Annotations", "Microsoft.CodeAnalysis.CSharp"]);
    }
  }

  public static void RejectAPackageThatHasNoAllowList()
  {
    IReadOnlyList<string> disallowed = DevCli.NuspecDependencyAllowList.FindDisallowed(
      "TimeWarp.State.Unexpected",
      ["TimeWarp.State"]);

    disallowed.ShouldBe(["<no allow-list for TimeWarp.State.Unexpected>"]);
  }

  public static void RejectAnUnexpectedDependencyInNuspecXml()
  {
    IReadOnlyList<string> dependencyIds = DevCli.NuspecDependencyAllowList.ReadDependencyIds(
      """
      <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
        <metadata>
          <dependencies>
            <group targetFramework="net11.0">
              <dependency id="TimeWarp.Mediator" version="15.0.0-beta.5" exclude="Build,Analyzers" />
              <dependency id="Microsoft.CodeAnalysis.CSharp" version="5.12.0" exclude="Build,Analyzers" />
            </group>
          </dependencies>
        </metadata>
      </package>
      """);
    IReadOnlyList<string> disallowed = DevCli.NuspecDependencyAllowList.FindDisallowed("TimeWarp.State", dependencyIds);
    disallowed.ShouldBe(["Microsoft.CodeAnalysis.CSharp"]);
  }

  public static void PinRoslynAtTheConsumerCompilerFloor()
  {
    string props = ReadRepoFile("Directory.Packages.props");
    props.ShouldContain("""<PackageVersion Include="Microsoft.CodeAnalysis.CSharp" Version="5.12.0" />""");

    string buildProps = ReadRepoFile("Directory.Build.props");
    System.Xml.Linq.XDocument.Parse(buildProps)
      .Descendants()
      .Where(element => element.Name.LocalName == "PackageReference")
      .Select(element => (string?)element.Attribute("Include"))
      .ShouldNotContain("Microsoft.CodeAnalysis.CSharp");
  }

  private static bool IsPrivate(System.Xml.Linq.XElement packageReference)
  {
    string? privateAssets = (string?)packageReference.Attribute("PrivateAssets")
      ?? packageReference.Elements().FirstOrDefault(element => element.Name.LocalName == "PrivateAssets")?.Value;
    return string.Equals(privateAssets?.Trim(), "all", StringComparison.OrdinalIgnoreCase);
  }

  private static string ReadRepoFile(string relativePath) =>
    File.ReadAllText(Path.Combine(FindRepoRoot(), relativePath));

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

    throw new InvalidOperationException("Could not find the repository root.");
  }
}
