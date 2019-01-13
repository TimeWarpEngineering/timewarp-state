namespace Tools.Commands.SetVersion
{
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;

  internal class SetConsoleTemplateVersionHandler : IRequestHandler<SetConsoleTemplateVersionRequest>
  {
    private YmlTools YmlTools { get; }

    public SetConsoleTemplateVersionHandler(YmlTools aYmlTools)
    {
      YmlTools = aYmlTools;
    }

    public Task<Unit> Handle(SetConsoleTemplateVersionRequest aSetConsoleTemplateVersionRequest, CancellationToken aCancellationToken)
    {
      UpdateAzurePipeLinesYml(aSetConsoleTemplateVersionRequest, @"\Build\Development\ConsoleTemplate.yml");
      UpdateAzurePipeLinesYml(aSetConsoleTemplateVersionRequest, @"\Build\ConsoleTemplate.yml");
      return Unit.Task;
    }

    private void UpdateAzurePipeLinesYml(SetConsoleTemplateVersionRequest aSetConsoleTemplateVersionRequest, string aRelativePath)
    {
      YmlTools.UpdateAzurePipeLinesYml(
        aSetConsoleTemplateVersionRequest.Major,
        aSetConsoleTemplateVersionRequest.Minor,
        aSetConsoleTemplateVersionRequest.Patch,
        aRelativePath
        );
    }
  }
}