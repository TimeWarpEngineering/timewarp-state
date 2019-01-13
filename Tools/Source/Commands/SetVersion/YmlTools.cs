namespace Tools.Commands.SetVersion
{
  using System.IO;
  using System.Text.RegularExpressions;
  using Tools.Services;
  /// <summary>
  /// Reusable Tools to work on Yml Files for SetVersion Commands
  /// </summary>
  /// <remarks> This is local to this command and is not a generic service</remarks>
  internal class YmlTools
  {
    private string BasePath { get; set; }
    private GitService GitService { get; }

    public YmlTools(GitService aGitService)
    {
      GitService = aGitService;
      DirectoryInfo gitDirectory = GitService.GitRootDirectoryInfo();
      BasePath = gitDirectory.FullName;
    }

    internal void UpdateAzurePipeLinesYml(int aMajor, int aMinor, int aPatch, string aRelativePath)
    {
      string filename = $@"{BasePath}{aRelativePath}";
      string text = File.ReadAllText(filename);
      text = Regex.Replace(text, @"Major: '.+'", $"Major: '{aMajor}'");
      text = Regex.Replace(text, @"Minor: '.+'", $"Minor: '{aMinor}'");
      text = Regex.Replace(text, @"Patch: '.+'", $"Patch: '{aPatch}'");
      File.WriteAllText(filename, text, System.Text.Encoding.UTF8);
    }

  }
}
