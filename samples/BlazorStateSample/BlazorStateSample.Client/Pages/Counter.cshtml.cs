using BlazorState;
using BlazorStateSample.Client.Features.Counter;
using BlazorStateSample.Client.Features.Counter.IncrementCount;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace BlazorStateSample.Client.Pages
{
	# region CounterModel
	
    public class CounterModel : ComponentBase, IBlazorStateComponent
    {
        public CounterState CounterState => Store.GetState<CounterState>();
        [Inject] public IMediator Mediator { get; set; }
        [Inject] public IStore Store { get; set; }

        public void ReRender() => StateHasChanged();

		#region IncrementCount
        public void IncrementCount()
        {
            var incrementCountRequest = new IncrementCountAction { Amount = 3 };
            Mediator.Send(incrementCountRequest);
        }
		#endregion IncrementCount
    }
	#endregion CounterModel
}
