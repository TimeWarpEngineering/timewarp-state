using BlazorState;
using MediatR;

namespace Middleware.Client.Features.Application;


public partial class ApplicationState
{
    public class TwoSecondTaskHandler : ActionHandler<TwoSecondTaskAction>
    {
        public TwoSecondTaskHandler(IStore aStore) : base(aStore)
        {
        }

        public override async Task<Unit> Handle(TwoSecondTaskAction aAction, CancellationToken aCancellationToken)
        {
            Console.WriteLine("Start 2 Second Task");
            await Task.Delay(millisecondsDelay: 2000, cancellationToken: aCancellationToken);
            Console.WriteLine("Completed 2 Second Task");
            return Unit.Value;
        }
    }
}
