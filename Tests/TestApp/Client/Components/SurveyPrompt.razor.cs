namespace TestApp.Client.Components;

using Microsoft.AspNetCore.Components;
using TestApp.Client.Features.Base.Components;

public partial class SurveyPrompt : BaseComponent
{
  [Parameter]
  public string Title { get; set; } // Demonstrates how a parent component can supply parameters
}
