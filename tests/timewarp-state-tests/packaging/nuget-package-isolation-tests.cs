#region Purpose
// Proves the pack guard flags a global-cache entry whose source is a local path.
#endregion

#region Design
// The checker is the same type dev pack runs. These tests build a fake global-packages
// folder so a regression fails without writing ~/.nuget/packages.
// DeleteVersionFolders is the same helper verify-samples uses to drop a same-version
// extract before restoring the nupkg just packed.
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

  public static void DeleteTheNamedVersionFolderAndLeaveOtherVersions()
  {
    using CacheFixture fixture = CacheFixture.Create(
      "timewarp.state",
      "12.0.0-beta.16",
      """{ "source": "/tmp/repo/artifacts/packages" }""");
    string otherVersion = Path.Combine(fixture.Root, "timewarp.state", "12.0.0-beta.15");
    string otherPackage = Path.Combine(fixture.Root, "timewarp.state.plus", "12.0.0-beta.16");
    Directory.CreateDirectory(otherVersion);
    Directory.CreateDirectory(otherPackage);
    File.WriteAllText(Path.Combine(otherVersion, ".nupkg.metadata"), """{ "source": "https://api.nuget.org/v3/index.json" }""");
    File.WriteAllText(Path.Combine(otherPackage, ".nupkg.metadata"), """{ "source": "/tmp/repo/artifacts/packages" }""");

    IReadOnlyList<string> deleted = DevCli.NuGetPackageIsolation.DeleteVersionFolders(
      fixture.Root,
      "12.0.0-beta.16",
      ["TimeWarp.State", "TimeWarp.State"]);

    deleted.Count.ShouldBe(1);
    deleted[0].ShouldBe(Path.Combine(fixture.Root, "timewarp.state", "12.0.0-beta.16"));
    Directory.Exists(deleted[0]).ShouldBeFalse();
    Directory.Exists(otherVersion).ShouldBeTrue();
    Directory.Exists(otherPackage).ShouldBeTrue();
  }

  public static void LeaveTheCacheUntouchedWhenTheVersionIsAbsent()
  {
    using CacheFixture fixture = CacheFixture.Create(
      "timewarp.state",
      "12.0.0-beta.16",
      """{ "source": "/tmp/repo/artifacts/packages" }""");

    IReadOnlyList<string> deleted = DevCli.NuGetPackageIsolation.DeleteVersionFolders(
      fixture.Root,
      "9.0.0",
      ["TimeWarp.State.Blazor"]);

    deleted.ShouldBeEmpty();
    Directory.Exists(Path.Combine(fixture.Root, "timewarp.state", "12.0.0-beta.16")).ShouldBeTrue();
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
