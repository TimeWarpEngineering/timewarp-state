namespace Tools.Commands.SetVersion
{
  using MediatR;

  /// <summary>
  /// Set the version in all required files.
  /// </summary>
  public class SetVersionRequest : IRequest
  {
    /// <summary>
    /// All Valid projects
    /// </summary>
    internal static string[] ProjectList { get; } = new string[] { "BlazorState"};

    /// <summary>
    /// The Project {BlazorState,BlazorTemplate,ConsoleTemplate}
    /// </summary>
    public string Project { get; set; }

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
