namespace Tools.Commands.SetVersion
{
  using MediatR;

  /// <summary>
  /// Set the version in all required files.
  /// </summary>
  internal class SetConsoleTemplateVersionRequest : IRequest
  {
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
