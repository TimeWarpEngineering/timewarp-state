namespace TestApp.Client.Components
{
  using Microsoft.AspNetCore.Components;

  public class SurveyPromptBase : ComponentBase
  {
    [Parameter]
    public string Title { get; set; } // Demonstrates how a parent component can supply parameters
  }
}