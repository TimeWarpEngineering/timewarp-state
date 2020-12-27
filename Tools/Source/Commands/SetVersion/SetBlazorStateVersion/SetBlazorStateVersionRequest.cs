namespace Tools.Commands.SetVersion
{
  using MediatR;

  /// <summary>
  /// Set the version in all required files for BlazorState
  /// This will also set the reference in the TimeWarp Blazor Template
  /// so you may want to update its version also.
  /// </summary>
  internal class SetBlazorStateVersionRequest : IRequest
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
