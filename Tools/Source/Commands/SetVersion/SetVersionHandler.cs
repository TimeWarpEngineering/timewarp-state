namespace Tools.Commands.SetVersion
{
  using System.IO;
  using System.Text.RegularExpressions;
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;

  internal class SetVersionHandler : IRequestHandler<SetVersionRequest>
  {
    private string BasePath { get; set; }

    public Task<Unit> Handle(SetVersionRequest aSetVersionRequest, CancellationToken aCancellationToken)
    {

      BasePath = aSetVersionRequest.BasePath ?? @"C:\git\github\blazor-state\";
      BasePath = BasePath.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) + Path.DirectorySeparatorChar;

      UpdateAzurePipeLinesYml(aSetVersionRequest, @"source\azure-pipelines-dev.yml");
      UpdateAzurePipeLinesYml(aSetVersionRequest, @"source\azure-pipelines.yml");
      UpdateVersionPrefix(aSetVersionRequest);
      UpdatePackageVersion(aSetVersionRequest);
      UpdatePackageReference(aSetVersionRequest);

      return Unit.Task;
    }

    private void UpdatePackageReference(SetVersionRequest aSetVersionRequest)
    {
      string filename = $@"{BasePath}source\TimeWarp.AspNetCore.Blazor.Templates\content\TimeWarp.BlazorHosted-CSharp\Source\BlazorHosted-CSharp.Client\BlazorHosted-CSharp.Client.csproj";
      string regex = @"<PackageReference Include=""Blazor-State"" Version="".+"" />";
      string replacement = $@"<PackageReference Include=""Blazor-State"" Version=""{ aSetVersionRequest.Major}.{aSetVersionRequest.Minor}.{aSetVersionRequest.Patch}"" />";
      string text = File.ReadAllText(filename);
      text = Regex.Replace(text, regex, replacement);
      File.WriteAllText(filename, text, System.Text.Encoding.UTF8);
    }

    private void UpdatePackageVersion(SetVersionRequest aSetVersionRequest)
    {
      string filename = $@"{BasePath}source\BlazorState\BlazorState.csproj";
      string regex = @"<PackageVersion>.+</PackageVersion>";
      string replacement = $@"<PackageVersion>{aSetVersionRequest.Major}.{aSetVersionRequest.Minor}.{aSetVersionRequest.Patch}</PackageVersion>";
      string text = File.ReadAllText(filename);
      text = Regex.Replace(text, regex, replacement);
      File.WriteAllText(filename, text, System.Text.Encoding.UTF8);
    }

    private void UpdateVersionPrefix(SetVersionRequest aSetVersionRequest)
    {
      string filename = $@"{BasePath}source\Directory.Build.props";
      string regex = @"<VersionPrefix>.+</VersionPrefix>";
      string replacement = $@"<VersionPrefix>{aSetVersionRequest.Major}.{aSetVersionRequest.Minor}.{aSetVersionRequest.Patch}</VersionPrefix>";
      string text = File.ReadAllText(filename);
      text = Regex.Replace(text, regex, replacement);
      File.WriteAllText(filename, text, System.Text.Encoding.UTF8);
    }

    private void UpdateAzurePipeLinesYml(SetVersionRequest aSetVersionRequest, string aRelativePath)
    {
      string filename = $@"{BasePath}{aRelativePath}";
      string text = File.ReadAllText(filename);
      text = Regex.Replace(text, @"Major: '.+'", $"Major: '{aSetVersionRequest.Major}'");
      text = Regex.Replace(text, @"Minor: '.+'", $"Minor: '{aSetVersionRequest.Minor}'");
      text = Regex.Replace(text, @"Patch: '.+'", $"Patch: '{aSetVersionRequest.Patch}'");
      File.WriteAllText(filename, text, System.Text.Encoding.UTF8);
    }
  }
}
