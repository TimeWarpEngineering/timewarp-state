#!/usr/bin/env -S dotnet --
// ═══════════════════════════════════════════════════════════════════════════════
// DEV CLI - timewarp-state DEVELOPMENT TOOL
// ═══════════════════════════════════════════════════════════════════════════════
//
// Usage:
//   As runfile:  dotnet run --file tools/dev-cli/dev.cs -- <command>
//   As AOT:      ./bin/dev <command>
//
// Run `./bin/dev --help` for available commands.
//
// To bootstrap:
//   dotnet run --file tools/dev-cli/dev.cs -- self-install
//   direnv allow
//   dev --help
// ═══════════════════════════════════════════════════════════════════════════════

#region Purpose
// Entry point for the TimeWarp.State development CLI
#endregion
#region Design
// Thin Nuru wrapper. Shared endpoints (clean, self-install, check-version) come
// from TimeWarp.Nuru.DevCli. Local endpoints: build (source, tests, and
// project-referenced samples; sample 00 auto omitted), test, e2e, pack,
// verify-samples (packs, then restores sample 00 auto from the local feed),
// workflow (PR verifies samples, which packs; release promotes CI artifacts). Product under source/.
// Apply points NUGET_PACKAGES at artifacts/nuget-packages before any command runs
// so child dotnet restores do not fill ~/.nuget/packages. Root nuget.config does
// not list artifacts/packages.
#endregion

string? repoRoot = Git.FindRoot();
if (repoRoot is not null)
{
  NuGetPackageIsolation.Apply(repoRoot);
}

NuruApp app = NuruApp.CreateBuilder()
  .WithName("dev")
  .WithDescription("Development CLI for timewarp-state")
  .ConfigureServices(services =>
  {
    services.AddSingleton<IRepoCleanService, RepoCleanService>();
    services.AddSingleton<NuGetVersionService>();
    services.AddSingleton<IRepoConfigService, RepoConfigService>();
    services.AddSingleton<IPackableProjectService, PackableProjectService>();
  })
  .DiscoverEndpoints()
  .Build();

return await app.RunAsync(args);
