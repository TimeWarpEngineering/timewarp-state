﻿#pragma checksum "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor" "{8829d00f-11b8-4213-878b-770e8597ac16}" "f797c16261db2c5ddc8e4b327b7940238885c85603435ab2bfe57f7c77f137c0"
// <auto-generated/>
#pragma warning disable 1591
namespace Test.App.Client.Pages
{
    #line hidden
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using global::Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using System.Diagnostics.CodeAnalysis;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using System.Net.Http.Json;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using System.Text.RegularExpressions;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using static Microsoft.AspNetCore.Components.Web.RenderMode;

#line default
#line hidden
#nullable disable
#nullable restore
#line 9 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Microsoft.AspNetCore.Components.Web.Virtualization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 10 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 11 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using TimeWarp.Features.ActionTracking;

#line default
#line hidden
#nullable disable
#nullable restore
#line 12 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using TimeWarp.Features.Developer;

#line default
#line hidden
#nullable disable
#nullable restore
#line 14 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Test.App.Client;

#line default
#line hidden
#nullable disable
#nullable restore
#line 15 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Test.App.Client.Components;

#line default
#line hidden
#nullable disable
#nullable restore
#line 16 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Test.App.Client.Features.Base.Components;

#line default
#line hidden
#nullable disable
#nullable restore
#line 17 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Test.App.Client.Features.CloneTest.Pages;

#line default
#line hidden
#nullable disable
#nullable restore
#line 18 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Test.App.Client.Features.Color.Components;

#line default
#line hidden
#nullable disable
#nullable restore
#line 19 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Test.App.Client.Features.Counter.Components;

#line default
#line hidden
#nullable disable
#nullable restore
#line 20 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Test.App.Client.Features.EventStream.Components;

#line default
#line hidden
#nullable disable
#nullable restore
#line 21 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\_Imports.razor"
using Test.App.Client.Pages;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
           [Route(Route)]

#line default
#line hidden
#nullable disable
    [global::Test.App.Client.Pages.ActiveActionsPage.__PrivateComponentRenderModeAttribute]
    public partial class ActiveActionsPage : BaseComponent
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenComponent<global::Microsoft.AspNetCore.Components.Web.PageTitle>(0);
            __builder.AddAttribute(1, "ChildContent", (global::Microsoft.AspNetCore.Components.RenderFragment)((__builder2) => {
#nullable restore
#line (5,13)-(5,18) 25 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
__builder2.AddContent(2, Title);

#line default
#line hidden
#nullable disable
            }
            ));
            __builder.CloseComponent();
            __builder.AddMarkupContent(3, "\r\n");
            __builder.OpenElement(4, "h3");
#nullable restore
#line (6,6)-(6,11) 24 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
__builder.AddContent(5, Title);

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
            __builder.AddMarkupContent(6, "\r\n");
#nullable restore
#line (7,2)-(7,19) 24 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
__builder.AddContent(7, RenderModeDisplay);

#line default
#line hidden
#nullable disable
            __builder.AddMarkupContent(8, "\r\n<hr>\r\n\r\n\r\n");
            __builder.AddMarkupContent(9, "<p>Determine if Active Actions list working correctly</p>\r\n\r\n\r\n");
            __builder.OpenElement(10, "p");
            __builder.AddMarkupContent(11, "<strong>ActionTrackingState.IsActive:</strong> ");
#nullable restore
#line (14,52)-(14,80) 25 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
__builder.AddContent(12, ActionTrackingState.IsActive);

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
            __builder.AddMarkupContent(13, "\r\n");
            __builder.OpenElement(14, "p");
            __builder.AddMarkupContent(15, "<strong>ActionTrackingState.IsAnyActive (Two Second Tasks):</strong> ");
#nullable restore
#line (15,74)-(15,157) 25 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
__builder.AddContent(16, ActionTrackingState.IsAnyActive([typeof(ActionTrackingState.TwoSecondTask.Action)]));

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
            __builder.AddMarkupContent(17, "\r\n");
            __builder.OpenElement(18, "p");
            __builder.AddMarkupContent(19, "<strong>ActionTrackingState.IsAnyActive (Five Second Tasks):</strong> ");
#nullable restore
#line (16,75)-(16,159) 25 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
__builder.AddContent(20, ActionTrackingState.IsAnyActive([typeof(ActionTrackingState.FiveSecondTask.Action)]));

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
            __builder.AddMarkupContent(21, "\r\n");
            __builder.OpenElement(22, "p");
            __builder.AddMarkupContent(23, "<strong>ActionTrackingState.IsAnyActive (Two Second or Five Second Tasks):</strong> ");
#nullable restore
#line (17,89)-(17,222) 25 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
__builder.AddContent(24, ActionTrackingState.IsAnyActive([typeof(ActionTrackingState.FiveSecondTask.Action),typeof(ActionTrackingState.TwoSecondTask.Action)]));

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
            __builder.AddMarkupContent(25, "\r\n");
            __builder.AddMarkupContent(26, "<p><strong>Active Actions:</strong></p>");
#nullable restore
#line 19 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
 foreach(IAction action in ActionTrackingState.ActiveActions)
{

#line default
#line hidden
#nullable disable
            __builder.OpenElement(27, "p");
#nullable restore
#line (21,8)-(21,33) 25 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
__builder.AddContent(28, action.GetType().FullName);

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
#nullable restore
#line 22 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
}

#line default
#line hidden
#nullable disable
            __builder.OpenElement(29, "button");
            __builder.AddAttribute(30, "data-qa", "FiveSecondButton");
            __builder.AddAttribute(31, "onclick", global::Microsoft.AspNetCore.Components.EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 25 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
                                            FiveSecondTaskButtonClick

#line default
#line hidden
#nullable disable
            ));
            __builder.AddContent(32, "Launch Five Second Task");
            __builder.CloseElement();
            __builder.AddMarkupContent(33, "\r\n");
            __builder.OpenElement(34, "button");
            __builder.AddAttribute(35, "data-qa", "TwoSecondButton");
            __builder.AddAttribute(36, "onclick", global::Microsoft.AspNetCore.Components.EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 26 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
                                           TwoSecondTaskButtonClick

#line default
#line hidden
#nullable disable
            ));
            __builder.AddContent(37, "Launch Two Second Task");
            __builder.CloseElement();
            __builder.AddMarkupContent(38, "\r\n\r\n<hr>\r\n\r\n");
            __builder.AddMarkupContent(39, "<p><strong>Act:</strong><br>\r\n  Click the `Launch Five Second Task` Button.<br>\r\n  Click the `Launch Two Second Task` Button.<br></p>\r\n\r\n\r\n");
            __builder.AddMarkupContent(40, "<p><strong>Assert:</strong>\r\n  The states transition accordingly.\r\n</p>\r\n\r\n<hr>\r\n");
            __builder.AddMarkupContent(41, "<p><strong>Repeat</strong> the above steps where `Current Render Mode` is Server and Wasm.\r\n</p>");
        }
        #pragma warning restore 1998
#nullable restore
#line 47 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
 
  /// <summary>
  /// The title of the page
  /// </summary>
  public const string Title = "Active Actions Page";


  /// <summary>
  /// The route for the page
  /// </summary>
  public const string Route = "/ActiveActionsPage";
  
  private async Task FiveSecondTaskButtonClick() =>
    await Send(new ActionTrackingState.FiveSecondTask.Action());

  private async Task TwoSecondTaskButtonClick() =>
    await Send(new ActionTrackingState.TwoSecondTask.Action());

#line default
#line hidden
#nullable disable
        private sealed class __PrivateComponentRenderModeAttribute : global::Microsoft.AspNetCore.Components.RenderModeAttribute
        {
            private static global::Microsoft.AspNetCore.Components.IComponentRenderMode ModeImpl => 
#nullable restore
#line 1 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\ActiveActionsPage.razor"
            InteractiveAuto

#line default
#line hidden
#nullable disable
            ;
            public override global::Microsoft.AspNetCore.Components.IComponentRenderMode Mode => ModeImpl;
        }
    }
}
#pragma warning restore 1591
