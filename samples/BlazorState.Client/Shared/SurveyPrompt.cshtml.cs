namespace BlazorState.Client.Shared
{
  using Microsoft.AspNetCore.Blazor.Components;

  public class SurveyPromptModel : BlazorComponent
  {
    [Parameter]
    protected string Title { get; set; } // Demonstrates how a parent component can supply parameters
  }
}