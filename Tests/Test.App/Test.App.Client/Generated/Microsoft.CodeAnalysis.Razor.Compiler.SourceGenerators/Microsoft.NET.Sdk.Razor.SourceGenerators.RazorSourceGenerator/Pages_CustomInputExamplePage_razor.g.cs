﻿#pragma checksum "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor" "{8829d00f-11b8-4213-878b-770e8597ac16}" "530994ce55abe4dc57d7ac7350602f13b47e1b94d8cd610f94cac0d9fbec7ec9"
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
    [global::Microsoft.AspNetCore.Components.RouteAttribute("/customInput")]
    [global::Test.App.Client.Pages.CustomInputExamplePage.__PrivateComponentRenderModeAttribute]
    public partial class CustomInputExamplePage : BaseComponent
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenComponent<global::Microsoft.AspNetCore.Components.Web.PageTitle>(0);
            __builder.AddAttribute(1, "ChildContent", (global::Microsoft.AspNetCore.Components.RenderFragment)((__builder2) => {
#nullable restore
#line (7,13)-(7,18) 25 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
__builder2.AddContent(2, Title);

#line default
#line hidden
#nullable disable
            }
            ));
            __builder.CloseComponent();
            __builder.AddMarkupContent(3, "\r\n");
            __builder.OpenElement(4, "h1");
#nullable restore
#line (8,6)-(8,11) 24 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
__builder.AddContent(5, Title);

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
            __builder.AddMarkupContent(6, "\r\n");
#nullable restore
#line (9,2)-(9,19) 24 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
__builder.AddContent(7, RenderModeDisplay);

#line default
#line hidden
#nullable disable
            __builder.AddMarkupContent(8, "\r\n<hr>\r\n\r\n\r\n");
            __builder.AddMarkupContent(9, "<p>\r\n  This test page is designed to validate the dynamic style adjustment of a CustomInput component based on the application\'s current theme (Light or Dark).\r\n</p>\r\n\r\n\r\n");
            __builder.OpenElement(10, "ul");
            __builder.OpenElement(11, "li");
            __builder.AddMarkupContent(12, "<strong>Current Theme:</strong> ");
#nullable restore
#line (20,38)-(20,61) 25 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
__builder.AddContent(13, ThemeState.CurrentTheme);

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
            __builder.AddMarkupContent(14, "\r\n  ");
            __builder.OpenElement(15, "li");
            __builder.AddMarkupContent(16, "<strong>Amount:</strong> ");
#nullable restore
#line (23,31)-(23,45) 25 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
__builder.AddContent(17, MyModel.Amount);

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
            __builder.CloseElement();
            __builder.AddMarkupContent(18, "\r\n\r\n\r\n");
            __builder.OpenElement(19, "button");
            __builder.AddAttribute(20, "onclick", global::Microsoft.AspNetCore.Components.EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 28 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
                 ToggleTheme

#line default
#line hidden
#nullable disable
            ));
            __builder.AddContent(21, "Toggle Theme");
            __builder.CloseElement();
            __builder.AddMarkupContent(22, "\r\n<hr>\r\n\r\n\r\n");
            __builder.OpenElement(23, "div");
            __builder.AddAttribute(24, "style", "border: 2px solid blue; padding: 10px; margin-bottom: 20px;");
            __builder.AddMarkupContent(25, "<h2>System Under Test: CustomInput Component</h2>\r\n  ");
            __builder.OpenComponent<global::Microsoft.AspNetCore.Components.Forms.EditForm>(26);
            __builder.AddComponentParameter(27, "FormName", "TheForm");
            __builder.AddComponentParameter(28, "Model", global::Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<global::System.Object>(
#nullable restore
#line 34 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
                                      MyModel

#line default
#line hidden
#nullable disable
            ));
            __builder.AddAttribute(29, "ChildContent", (global::Microsoft.AspNetCore.Components.RenderFragment<Microsoft.AspNetCore.Components.Forms.EditContext>)((context) => (__builder2) => {
                global::__Blazor.Test.App.Client.Pages.CustomInputExamplePage.TypeInference.CreateCustomInput_0(__builder2, 30, 31, 
#nullable restore
#line 35 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
                                                            () => MyModel.Amount

#line default
#line hidden
#nullable disable
                , 32, "Amount", 33, 
#nullable restore
#line 35 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
                             MyModel.Amount

#line default
#line hidden
#nullable disable
                , 34, global::Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, global::Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, __value => MyModel.Amount = __value, MyModel.Amount)), 35, () => MyModel.Amount);
            }
            ));
            __builder.CloseComponent();
            __builder.CloseElement();
            __builder.AddMarkupContent(36, "\r\n\r\n<hr>\r\n\r\n");
            __builder.AddMarkupContent(37, "<p><strong>Act:</strong> Toggle the theme by clicking the \'Toggle Theme\' button.</p>\r\n\r\n\r\n");
            __builder.AddMarkupContent(38, "<p><strong>Assert:</strong>\r\n  Verify that the theme changes from Light to Dark or vice versa upon clicking the \'Toggle Theme\' button by\r\n  inspecting the class on the input element.\r\n</p>\r\n\r\n\r\n");
            __builder.AddMarkupContent(39, "<p><strong>Assert:</strong> Verify that the CustomInput component properly reflects the current theme.</p>\r\n\r\n\r\n\r\n");
            __builder.AddMarkupContent(40, "<p><strong>Act:</strong> Enter a number into the input box.</p>\r\n");
            __builder.AddMarkupContent(41, "<p><strong>Assert:</strong> The Amount value updates on exiting the input.</p>\r\n\r\n<hr>\r\n");
            __builder.AddMarkupContent(42, "<p><strong>Repeat</strong> the above steps where `Current Render Mode` is Server and Wasm.\r\n</p>");
        }
        #pragma warning restore 1998
#nullable restore
#line 63 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
       
  readonly TheModel MyModel = new()
  {
    Amount = 10
  };

  class TheModel
  {
    public int Amount { get; set; }
  }

  /// <summary>
  /// The title for the page.
  /// </summary>
  public const string Title = "Custom Input Test";

  /// <summary>
  /// The route for the page.
  /// </summary>
  public const string Route = "/custominput";
  private void ToggleTheme()
  {
    Send
    (
    new ThemeState.Update.Action
    {
      NewTheme = ThemeState.CurrentTheme == ThemeState.Theme.Light ? ThemeState.Theme.Dark : ThemeState.Theme.Light
    }
    );
  }

#line default
#line hidden
#nullable disable
        private sealed class __PrivateComponentRenderModeAttribute : global::Microsoft.AspNetCore.Components.RenderModeAttribute
        {
            private static global::Microsoft.AspNetCore.Components.IComponentRenderMode ModeImpl => 
#nullable restore
#line 1 "C:\Users\Admin\source\repos\blazor-state\Tests\Test.App\Test.App.Client\Pages\CustomInputExamplePage.razor"
            InteractiveAuto

#line default
#line hidden
#nullable disable
            ;
            public override global::Microsoft.AspNetCore.Components.IComponentRenderMode Mode => ModeImpl;
        }
    }
}
namespace __Blazor.Test.App.Client.Pages.CustomInputExamplePage
{
    #line hidden
    internal static class TypeInference
    {
        public static void CreateCustomInput_0<T>(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder, int seq, int __seq0, global::System.Linq.Expressions.Expression<global::System.Func<T>> __arg0, int __seq1, global::System.String __arg1, int __seq2, T __arg2, int __seq3, global::Microsoft.AspNetCore.Components.EventCallback<T> __arg3, int __seq4, global::System.Linq.Expressions.Expression<global::System.Func<T>> __arg4)
        {
        __builder.OpenComponent<global::Test.App.Client.Components.CustomInput<T>>(seq);
        __builder.AddComponentParameter(__seq0, "ValidationFor", __arg0);
        __builder.AddComponentParameter(__seq1, "Label", __arg1);
        __builder.AddComponentParameter(__seq2, "Value", __arg2);
        __builder.AddComponentParameter(__seq3, "ValueChanged", __arg3);
        __builder.AddComponentParameter(__seq4, "ValueExpression", __arg4);
        __builder.CloseComponent();
        }
    }
}
#pragma warning restore 1591
