namespace BlazorHosted_CSharp.Client.Components
{
  using Microsoft.AspNetCore.Components;

  public class SurveyPromptModel : ComponentBase
  {
    [Parameter]
    protected string Title { get; set; } // Demonstrates how a parent component can supply parameters
  }
}