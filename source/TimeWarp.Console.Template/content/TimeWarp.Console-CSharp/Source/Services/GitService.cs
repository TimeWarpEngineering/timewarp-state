namespace Console_CSharp.Services
{
  using System;
  using System.IO;
  using System.Linq;

  internal class GitService
  {
    public DirectoryInfo GitRootDirectoryInfo()
    {
      var directory = new DirectoryInfo(Environment.CurrentDirectory);
      bool found = IsGitDirectory(directory);
      while (!found && directory.Parent != null)
      {
        directory = directory.Parent;
        found = IsGitDirectory(directory);
      }

      return directory;
    }

    public bool IsGitDirectory(DirectoryInfo aDirectoryInfo) =>
      aDirectoryInfo.GetDirectories(".git").Any();
  }
}
