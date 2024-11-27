namespace Sample01Wasm;

public partial class App : ComponentBase
{
    [Inject] private JsonRequestHandler JsonRequestHandler { get; set; } = null!;
    [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; } = null!;
    [Inject] private RouteManager RouteManager { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ReduxDevToolsInterop.InitAsync();
            await JsonRequestHandler.InitAsync();
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
