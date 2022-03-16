namespace TestApp.Client.Pages;

using System;
using TestApp.Client.Features.Base.Components;

public partial class CustomInputExamplePage : BaseComponent
{
  protected void HandleValidSubmit() => Console.WriteLine("Valid Submit");
}
