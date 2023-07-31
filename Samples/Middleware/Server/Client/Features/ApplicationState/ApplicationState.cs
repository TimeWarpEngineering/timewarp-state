using BlazorState;

namespace Middleware.Client.Features.Application;

public partial class ApplicationState : State<ApplicationState>
{
    private List<string> _ProcessingList;

    public bool IsProcessing => _ProcessingList.Count > 0;

    public bool IsProcessingAny(params string[] aActions) => aActions.Intersect(_ProcessingList).Any();

    public IReadOnlyList<string> ProcessingList => _ProcessingList.AsReadOnly();
    public override void Initialize()
    {
        _ProcessingList = new List<string>();
    }
}