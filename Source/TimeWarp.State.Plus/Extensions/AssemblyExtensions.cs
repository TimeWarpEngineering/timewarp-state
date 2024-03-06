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

  /// <summary>
  /// Initializes a new instance of the <see cref="AssemblyInfo"/> class
  /// using the specified assembly to extract metadata attributes.
  /// </summary>
  /// <param name="assembly">The assembly from which to extract metadata attributes.</param>
  public AssemblyInfo(Assembly assembly)
  {
    Metadata = assembly
      .GetCustomAttributes<AssemblyMetadataAttribute>()
      .ToDictionary(attr => attr.Key, attr => attr.Value);
  }

  /// <summary>
  /// Gets the value of a specific metadata attribute by key.
  /// </summary>
  /// <param name="key">The key of the metadata attribute to retrieve.</param>
  /// <returns>The value of the metadata attribute if found; otherwise, null.</returns>
  private string? GetMetadata(string key) => Metadata.GetValueOrDefault(key);

  /// <summary>
  /// Gets the commit hash stored in the assembly's metadata, if available.
  /// </summary>
  /// <value>The commit hash, or null if not available.</value>
  public string? CommitHash => GetMetadata("CommitHash");

  /// <summary>
  /// Gets the commit date stored in the assembly's metadata, if available.
  /// </summary>
  /// <value>The commit date in string format, or null if not available.</value>
  public string? CommitDate => GetMetadata("CommitDate");
}

