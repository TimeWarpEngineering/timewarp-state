namespace Tools.Commands.SetVersion
{
  using System.IO;
  using System.Text.RegularExpressions;
  using Tools.Services;

  internal class YmlTools
  {
    private string BasePath { get; set; }
    private GitService GitService { get; }

    public YmlTools(GitService aGitService)
    {
      GitService = aGitService;
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
