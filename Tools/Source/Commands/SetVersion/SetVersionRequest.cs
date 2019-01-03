namespace Tools.Commands.SetVersion
{
  using MediatR;

  /// <summary>
  /// Set the version in all required files.
  /// </summary>
  public class SetVersionRequest : IRequest
  {

    /// <summary>
    /// BasePath of source
    /// </summary>
    public string BasePath { get; set; }

    /// <summary>
    /// Major Version Number
    /// </summary>
    public int Major { get; set; }

    /// <summary>
    /// Minor Version Number
    /// </summary>
    public int Minor { get; set; }

    /// <summary>
    /// Patch Version Number
    /// </summary>
    public int Patch { get; set; }
  }
}
