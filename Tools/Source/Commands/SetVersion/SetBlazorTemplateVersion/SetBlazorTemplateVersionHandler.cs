namespace Tools.Commands.SetVersion
{
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;
  using Tools.Services;

  internal class SetBlazorTemplateVersionHandler : IRequestHandler<SetBlazorTemplateVersionRequest>
  {
    private YmlTools YmlTools { get; }

    public SetBlazorTemplateVersionHandler(GitService aGitService, YmlTools aYmlTools)
    {
      YmlTools = aYmlTools;
    }

    public Task<Unit> Handle(SetBlazorTemplateVersionRequest aSetBlazorTemplateVersionRequest, CancellationToken aCancellationToken)
    {
      YmlTools.UpdateAzurePipeLinesYml(
        aSetBlazorTemplateVersionRequest.Major,
        aSetBlazorTemplateVersionRequest.Minor,
        aSetBlazorTemplateVersionRequest.Patch,
        @"\Build\BlazorTemplate.yml");

      return Unit.Task;
    }
  }
}