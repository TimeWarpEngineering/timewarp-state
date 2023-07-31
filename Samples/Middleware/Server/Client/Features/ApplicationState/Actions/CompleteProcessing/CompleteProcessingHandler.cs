
using BlazorState;
using MediatR;

namespace Middleware.Client.Features.Application;

public partial class  ApplicationState
{
    public class CompleteProcessingHandler : ActionHandler<CompleteProcessingAction>
    {
        public CompleteProcessingHandler(IStore aStore) : base(aStore)
        {
        }
        protected ApplicationState ApplicationState => Store.GetState<ApplicationState>();

        public override Task<Unit>  Handle(CompleteProcessingAction aAction, CancellationToken aCancellationToken)
        {
         ApplicationState._ProcessingList.Remove(aAction.ActionName);
          return Unit.Task;
        }
    }
}
