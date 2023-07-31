using BlazorState;
using Middleware.Client.Pipeline;

namespace Middleware.Client.Features.Application;


public partial class ApplicationState
{
  [TrackProcessing] 
  public class FiveSecondTaskAction : IAction
  {
      public string ActionName {get; set;}
  }
}
    