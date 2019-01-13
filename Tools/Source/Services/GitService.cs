namespace Tools.Services
{
  using System;
  using System.IO;
  using System.Linq;

  /// <summary>
  /// Helper tools related to git
  /// </summary>
  public class GitService
  {
    /// <summary>
    /// Get the directory that contains ".git" directory starting from current directory and recursing up the parent directories.
    /// </summary>
    /// <returns>DirectoryInfo of the directory that contains the ".git" directory or returns null if not in a git repository</returns>
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

    /// <summary>
    /// Does this directory contain a ".git" directory?
    /// </summary>
    /// <param name="aDirectoryInfo">The directory to check</param>
    /// <returns>True if a ".git" directory exist false otherwise</returns>
    public bool IsGitDirectory(DirectoryInfo aDirectoryInfo) =>
      aDirectoryInfo.GetDirectories(".git").Any();
  }
}
