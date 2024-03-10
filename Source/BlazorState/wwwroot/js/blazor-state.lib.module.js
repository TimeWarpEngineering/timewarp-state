import { blazorState } from './BlazorState.js';
import { ReduxDevTools } from './ReduxDevTools.js';
import { log, LogAction } from './Logger.js';
import { BlazorStateName, InitializeJavaScriptInteropName, ReduxDevToolsFactoryName, ReduxDevToolsName, } from './Constants.js';
function InitializeJavaScriptInterop(JsonRequestHandler) {
    log("TimeWarp.State", "InitializeJavaScriptInterop", "info");
    blazorState.jsonRequestHandler = JsonRequestHandler;
}
function ReduxDevToolsFactory(reduxDevToolsOptions) {
    log("TimeWarp.State", "ReduxDevToolsFactory", "info");
    if (!window[ReduxDevToolsName]) {
        const reduxDevTools = new ReduxDevTools(reduxDevToolsOptions);
        blazorState.reduxDevTools = reduxDevTools;
        window[ReduxDevToolsName] = reduxDevTools;
    }
    return window[ReduxDevToolsName].IsEnabled;
}
function initializeEnvironment() {
    log("TimeWarp.State", "initializeEnvironment", "info", LogAction.Begin);
    window[InitializeJavaScriptInteropName] = InitializeJavaScriptInterop;
    window[ReduxDevToolsFactoryName] = ReduxDevToolsFactory;
    window[BlazorStateName] = blazorState;
    log("TimeWarp.State", "initializeEnvironment", "info", LogAction.End);
}
export function beforeWebStart(options, extensions) {
    log("TimeWarp.State Web", "beforeWebStart", "info", LogAction.Begin);
    initializeEnvironment();
}
export function afterWebStarted(blazor) {
    log("TimeWarp.State Web", "afterWebStarted", "info", LogAction.End);
}
export function beforeWebAssemblyStart(options, extensions) {
    log("TimeWarp.State WebAssembly", "beforeWebAssemblyStart", "info", LogAction.Begin);
    initializeEnvironment();
}
export function afterWebAssemblyStarted(blazor) {
    log("TimeWarp.State WebAssembly", "afterWebAssemblyStarted", "info", LogAction.End);
}
export function beforeServerStart(options, extensions) {
    log("TimeWarp.State Server", "beforeServerStart", "info", LogAction.Begin);
    initializeEnvironment();
}
export function afterServerStarted(blazor) {
    log("TimeWarp.State Server", "afterServerStarted", "info", LogAction.End);
}
//# sourceMappingURL=blazor-state.lib.module.js.map