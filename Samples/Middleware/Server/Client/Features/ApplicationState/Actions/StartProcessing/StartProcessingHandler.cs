using BlazorState;
using MediatR;

namespace Middleware.Client.Features.Application;
public partial class ApplicationState
{
    public class StartProcessingHandler : ActionHandler<StartProcessingAction>
    {
        public StartProcessingHandler(IStore aStore) : base(aStore)
        {
        }
        protected ApplicationState ApplicationState => Store.GetState<ApplicationState>();

        public override Task<Unit> Handle(StartProcessingAction aAction, CancellationToken aCancellationToken)
        {
          ApplicationState._ProcessingList.Add(aAction.ActionName);
          return Unit.Task;
        }
    }
}
    