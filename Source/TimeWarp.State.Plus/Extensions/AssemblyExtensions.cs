namespace TimeWarp.State.Plus.Extensions;

using System.Globalization;
using System.Runtime.CompilerServices;

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
  private readonly Assembly Assembly;
  private AssemblyCompanyAttribute AssemblyCompany => Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()!;
  private readonly Dictionary<string, string?> Metadata;

  /// <summary>
  /// Initializes a new instance of the <see cref="AssemblyInfo"/> class
  /// using the specified assembly to extract metadata attributes.
  /// </summary>
  /// <param name="assembly">The assembly from which to extract metadata attributes.</param>
  public AssemblyInfo(Assembly assembly)
  {
    Assembly = assembly;
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
  public string? CommitHash => InformationalVersion?.Split('+').Skip(1).FirstOrDefault();
  
  // Last 6 characters of the commit hash
  public string? ShortHash => CommitHash?[^6..];
  
  public string? CommitUrl => CommitHash is not null && RepositoryUrl is not null ? $"{RepositoryUrl.TrimEnd('/')}/commit/{CommitHash}" : null;
  
  /// <summary>
  /// Gets the commit date stored in the assembly's metadata, if available.
  /// </summary>
  /// <value>The commit date in string format, or null if not available.</value>
  public DateTime? CommitDate
  {
    get
    {
      string? commitDateString = GetMetadata(nameof(CommitDate));
      if (DateTime.TryParseExact(commitDateString, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime commitDate))
      {
        return commitDate;
      }

      return null;
    }
  }


  public string? RepositoryUrl => GetMetadata(nameof(RepositoryUrl));
  public string? Company => Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
  public string? Title => Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
  public string? Description => Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
  public string? Product => Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
  public string? InformationalVersion => Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
  public string? Version => Assembly.GetName().Version?.ToString();
  public string? FileVersion => Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
  public string? VersionString => Assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version;
  public string? Configuration => Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;
  public string? Trademark => Assembly.GetCustomAttribute<AssemblyTrademarkAttribute>()?.Trademark;
  public string? Culture => Assembly.GetCustomAttribute<AssemblyCultureAttribute>()?.Culture;
  public string? InternalsVisibleTo => string.Join(", ", Assembly.GetCustomAttributes<InternalsVisibleToAttribute>().Select(a => a.AssemblyName));
}
