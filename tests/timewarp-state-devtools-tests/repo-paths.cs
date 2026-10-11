#region Purpose
// Finds the repository root from a test output directory.
#endregion

#region Design
// Walks parents for timewarp-state.slnx. Tests resolve the DevTools csproj and the sample the same way an AppHost does.
#endregion

namespace TimeWarp.State.DevTools.Tests;

static class RepoPaths
{
  public static string Root()
  {
    DirectoryInfo? directory = new(AppContext.BaseDirectory);
    while (directory is not null)
    {
      if (File.Exists(Path.Combine(directory.FullName, "timewarp-state.slnx")))
      {
        return directory.FullName;
      }

      directory = directory.Parent;
    }

    throw new InvalidOperationException("Repository root not found from " + AppContext.BaseDirectory);
  }
}
