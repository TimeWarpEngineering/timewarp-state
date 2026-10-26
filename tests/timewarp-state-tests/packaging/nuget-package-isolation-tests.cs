#region Purpose
// Proves the pack guard flags a global-cache entry whose source is a local path.
#endregion

#region Design
// The checker is the same type dev pack runs. These tests build a fake global-packages
// folder so a regression fails without writing ~/.nuget/packages.
#endregion

namespace TimeWarp.State.Packaging;

public class NuGetPackageIsolation_Should_
{
  public static void FlagALocalPathSource()
  {
    using CacheFixture fixture = CacheFixture.Create(
      "timewarp.state",
      "12.0.0-beta.16",
      """{ "version": 2, "source": "/tmp/repo/artifacts/packages" }""");

    IReadOnlyList<DevCli.NuGetPackageIsolation.PoisonedCacheEntry> found = DevCli.NuGetPackageIsolation.FindPoisonedEntries(
      fixture.Root,
      "12.0.0-beta.16",
      ["TimeWarp.State"]);

    found.Count.ShouldBe(1);
    found[0].PackageId.ShouldBe("TimeWarp.State");
    found[0].Source.ShouldBe("/tmp/repo/artifacts/packages");
  }

  public static void AllowAnHttpsNugetOrgSource()
  {
    using CacheFixture fixture = CacheFixture.Create(
      "timewarp.state",
      "12.0.0-beta.16",
      """{ "version": 2, "source": "https://api.nuget.org/v3/index.json" }""");

    IReadOnlyList<DevCli.NuGetPackageIsolation.PoisonedCacheEntry> found = DevCli.NuGetPackageIsolation.FindPoisonedEntries(
      fixture.Root,
      "12.0.0-beta.16",
      ["TimeWarp.State"]);

    found.ShouldBeEmpty();
  }

  public static void FlagAFileUriSource()
  {
    using CacheFixture fixture = CacheFixture.Create(
      "timewarp.state.blazor",
      "12.0.0-beta.16",
      """{ "source": "file:///tmp/repo/artifacts/packages" }""");

    IReadOnlyList<DevCli.NuGetPackageIsolation.PoisonedCacheEntry> found = DevCli.NuGetPackageIsolation.FindPoisonedEntries(
      fixture.Root,
      "12.0.0-beta.16",
      ["TimeWarp.State.Blazor"]);

    found.Count.ShouldBe(1);
    found[0].Source.ShouldStartWith("file://");
  }

  public static void FlagMetadataThatHasNoSource()
  {
    using CacheFixture fixture = CacheFixture.Create(
      "timewarp.state.telemetry",
      "12.0.0-beta.16",
      """{ "version": 2 }""");

    IReadOnlyList<DevCli.NuGetPackageIsolation.PoisonedCacheEntry> found = DevCli.NuGetPackageIsolation.FindPoisonedEntries(
      fixture.Root,
      "12.0.0-beta.16",
      ["TimeWarp.State.Telemetry"]);

    found.Count.ShouldBe(1);
    found[0].Source.ShouldBe("(metadata has no string source)");
  }

  public static void IgnoreAVersionThatIsNotInTheCache()
  {
    using CacheFixture fixture = CacheFixture.Create(
      "timewarp.state",
      "12.0.0-beta.16",
      """{ "source": "/tmp/repo/artifacts/packages" }""");

    IReadOnlyList<DevCli.NuGetPackageIsolation.PoisonedCacheEntry> found = DevCli.NuGetPackageIsolation.FindPoisonedEntries(
      fixture.Root,
      "12.0.0-beta.15",
      ["TimeWarp.State"]);

    found.ShouldBeEmpty();
  }

  public static void SharedCacheFolders_SkipsTheIsolatedFolderAndKeepsADistinctPreviousCache()
  {
    string isolated = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "state-isolated-nuget"));
    string previous = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "state-custom-nuget"));
    DevCli.NuGetPackageIsolation.Isolation isolation = new(isolated, previous);

    IReadOnlyList<string> folders = DevCli.NuGetPackageIsolation.SharedCacheFolders(isolation);

    folders.ShouldContain(previous);
    folders.ShouldContain(DevCli.NuGetPackageIsolation.MachineGlobalPackagesFolder());
    folders.ShouldNotContain(isolated);
  }

  private sealed class CacheFixture : IDisposable
  {
    private CacheFixture(string root)
    {
      Root = root;
    }

    public string Root { get; }

    public static CacheFixture Create(string packageFolder, string version, string metadata)
    {
      string root = Path.Combine(Path.GetTempPath(), "nuget-guard-" + Guid.NewGuid().ToString("N"));
      string versionFolder = Path.Combine(root, packageFolder, version);
      Directory.CreateDirectory(versionFolder);
      File.WriteAllText(Path.Combine(versionFolder, ".nupkg.metadata"), metadata);
      return new CacheFixture(root);
    }

    public void Dispose()
    {
      if (Directory.Exists(Root))
      {
        Directory.Delete(Root, recursive: true);
      }
    }
  }
}
