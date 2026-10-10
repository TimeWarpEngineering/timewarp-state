#region Purpose
// Proves the packed-package dependency allow-list rejects Roslyn and JetBrains.Annotations.
#endregion

#region Design
// The checker is the same type dev pack runs against nupkgs. These tests feed it dependency ids and a
// nuspec document so a regression fails in the test run, without packing. The Roslyn pin assertion
// keeps the analyzer and generator on the 4.14.0 consumer compiler floor.
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
              <dependency id="TimeWarp.Mediator.Contracts" version="14.0.0-beta.4" exclude="Build,Analyzers" />
              <dependency id="Microsoft.CodeAnalysis.CSharp" version="4.14.0" exclude="Build,Analyzers" />
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
    props.ShouldContain("""<PackageVersion Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" />""");

    string buildProps = ReadRepoFile("Directory.Build.props");
    buildProps.ShouldNotContain("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" />""");
    buildProps.ShouldNotContain("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp"/>""");
  }

  private static string ReadRepoFile(string relativePath)
  {
    DirectoryInfo? directory = new(AppContext.BaseDirectory);
    while (directory is not null)
    {
      string solution = Path.Combine(directory.FullName, "timewarp-state.slnx");
      if (File.Exists(solution))
      {
        return File.ReadAllText(Path.Combine(directory.FullName, relativePath));
      }

      directory = directory.Parent;
    }

    throw new InvalidOperationException("Could not find the repository root.");
  }
}
