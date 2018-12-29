namespace Tools.Commands.SetVersion
{
  using MediatR;

  internal class SetVersionRequest : IRequest
  {
    public string BasePath { get; set; }
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
  }
}
