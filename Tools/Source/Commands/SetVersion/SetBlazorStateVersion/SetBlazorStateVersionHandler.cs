namespace Tools.Commands.SetVersion
{
  using System.IO;
  using System.Text.RegularExpressions;
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;
  using Tools.Services;

  internal class SetBlazorStateVersionHandler : IRequestHandler<SetBlazorStateVersionRequest>
  {
    private string BasePath { get; set; }
    private GitService GitService { get; }
    private YmlTools YmlTools { get; }

    public SetBlazorStateVersionHandler(GitService aGitService, YmlTools aYmlTools)
    {
      GitService = aGitService;
      YmlTools = aYmlTools;
    }

    public Task<Unit> Handle(SetBlazorStateVersionRequest aSetBlazorStateVersionRequest, CancellationToken aCancellationToken)
    {
      DirectoryInfo gitDirectory = GitService.GitRootDirectoryInfo();
      BasePath = gitDirectory.FullName;
      UpdateAzurePipeLinesYml(aSetBlazorStateVersionRequest, @"\Build\blazor-state.yml");
      UpdateVersionPrefix(aSetBlazorStateVersionRequest);
      UpdatePackageVersion(aSetBlazorStateVersionRequest);
      UpdatePackageReference(aSetBlazorStateVersionRequest);

      return Unit.Task;
    }

    private void UpdateAzurePipeLinesYml(SetBlazorStateVersionRequest aSetBlazorStateVersionRequest, string aRelativePath)
    {
      YmlTools.UpdateAzurePipeLinesYml(
        aSetBlazorStateVersionRequest.Major,
        aSetBlazorStateVersionRequest.Minor,
        aSetBlazorStateVersionRequest.Patch,
        aRelativePath
        );
    }

    private void UpdatePackageReference(SetBlazorStateVersionRequest aSetBlazorStateVersionRequest)
    {
      string filename = $@"{BasePath}\source\TimeWarp.AspNetCore.Blazor.Templates\content\TimeWarp.BlazorHosted-CSharp\Source\BlazorHosted-CSharp.Client\BlazorHosted-CSharp.Client.csproj";
      string regex = @"<PackageReference Include=""Blazor-State"" Version="".+"" />";
      string replacement = $@"<PackageReference Include=""Blazor-State"" Version=""{ aSetBlazorStateVersionRequest.Major}.{aSetBlazorStateVersionRequest.Minor}.{aSetBlazorStateVersionRequest.Patch}"" />";
      string text = File.ReadAllText(filename);
      text = Regex.Replace(text, regex, replacement);
      File.WriteAllText(filename, text, System.Text.Encoding.UTF8);
    }

    private void UpdatePackageVersion(SetBlazorStateVersionRequest aSetBlazorStateVersionRequest)
    {
      string filename = $@"{BasePath}\source\BlazorState\BlazorState.csproj";
      string regex = @"<PackageVersion>.+</PackageVersion>";
      string replacement = $@"<PackageVersion>{aSetBlazorStateVersionRequest.Major}.{aSetBlazorStateVersionRequest.Minor}.{aSetBlazorStateVersionRequest.Patch}</PackageVersion>";
      string text = File.ReadAllText(filename);
      text = Regex.Replace(text, regex, replacement);
      File.WriteAllText(filename, text, System.Text.Encoding.UTF8);
    }

    private void UpdateVersionPrefix(SetBlazorStateVersionRequest aSetBlazorStateVersionRequest)
    {
      string filename = $@"{BasePath}\source\Directory.Build.props";
      string regex = @"<VersionPrefix>.+</VersionPrefix>";
      string replacement = $@"<VersionPrefix>{aSetBlazorStateVersionRequest.Major}.{aSetBlazorStateVersionRequest.Minor}.{aSetBlazorStateVersionRequest.Patch}</VersionPrefix>";
      string text = File.ReadAllText(filename);
      text = Regex.Replace(text, regex, replacement);
      File.WriteAllText(filename, text, System.Text.Encoding.UTF8);
    }
  }
}