
using BlazorState;

namespace Middleware.Client.Features.Application;

public partial class ApplicationState
{
    public class CompleteProcessingAction : IAction
    {
        public string ActionName {get; set;}
    }
}