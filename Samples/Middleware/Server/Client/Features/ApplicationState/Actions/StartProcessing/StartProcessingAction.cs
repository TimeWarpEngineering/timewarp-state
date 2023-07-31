using BlazorState;
using Middleware.Client.Pipeline;

namespace Middleware.Client.Features.Application;

public partial class ApplicationState
{
  public class StartProcessingAction : IAction
  {
      public string ActionName {get; set;}
  }
}
    