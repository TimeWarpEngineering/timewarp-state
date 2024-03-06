namespace TimeWarp.State.Plus.Extensions;

[UsedImplicitly]
public static class AssemblyExtensions
{
  [UsedImplicitly]
  public static AssemblyInfo GetAssemblyInfo(this Assembly assembly) => new(assembly);
}

/// <summary>
/// Represents information extracted from an assembly's metadata attributes.
/// </summary>
public class AssemblyInfo
{
  private readonly Dictionary<string, string?> Metadata;

  public AssemblyInfo(Assembly assembly)
  {
    Metadata = assembly
      .GetCustomAttributes<AssemblyMetadataAttribute>()
      .ToDictionary(attr => attr.Key, attr => attr.Value);
  }

  private string? GetMetadata(string key) => Metadata.GetValueOrDefault(key);

  // Convenience properties
  [UsedImplicitly] public string? CommitHash => GetMetadata("CommitHash");
  [UsedImplicitly] public string? CommitDate => GetMetadata("CommitDate");
}
