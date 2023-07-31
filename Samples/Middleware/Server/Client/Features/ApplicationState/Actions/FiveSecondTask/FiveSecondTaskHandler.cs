using BlazorState;
using MediatR;

namespace Middleware.Client.Features.Application;


public partial class ApplicationState
{
    public class FiveSecondTaskHandler : ActionHandler<FiveSecondTaskAction>
    {
        public FiveSecondTaskHandler(IStore aStore) : base(aStore)
        {
        }

        public override async Task<Unit> Handle(FiveSecondTaskAction aAction, CancellationToken aCancellationToken)
        {
            Console.WriteLine("Start 5 Second Task");
            await Task.Delay(millisecondsDelay: 5000, cancellationToken: aCancellationToken);
            Console.WriteLine("Completed 5 Second Task");
            return Unit.Value;
        }
    }
}