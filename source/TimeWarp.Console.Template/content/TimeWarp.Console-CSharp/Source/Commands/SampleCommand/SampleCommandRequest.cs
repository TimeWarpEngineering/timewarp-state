namespace Console_CSharp.Commands.SampleCommand
{
  using MediatR;

  /// <summary>
  /// Sample Command.
  /// </summary>
  public class SampleCommandRequest : IRequest
  {

    /// <summary>
    /// Some string
    /// </summary>
    public string Parameter1 { get; set; }

    /// <summary>
    /// Some integer
    /// </summary>
    public int Parameter2 { get; set; }

    /// <summary>
    /// Another Integer
    /// </summary>
    public int Parameter3 { get; set; }

  }
}
