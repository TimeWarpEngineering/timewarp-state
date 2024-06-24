import { timeWarpState } from './TimeWarpState.js';
import { ReduxDevTools } from './ReduxDevTools.js';
import { log, LogAction } from './Logger.js';
import { TimeWarpStateName, InitializeJavaScriptInteropName, ReduxDevToolsFactoryName, ReduxDevToolsName, } from './Constants.js';
function InitializeJavaScriptInterop(JsonRequestHandler) {
    log("TimeWarp.State", "InitializeJavaScriptInterop", "info");
    timeWarpState.jsonRequestHandler = JsonRequestHandler;
}
function ReduxDevToolsFactory(reduxDevToolsOptions) {
    log("TimeWarp.State", "ReduxDevToolsFactory", "info");
    if (!window[ReduxDevToolsName]) {
        const reduxDevTools = new ReduxDevTools(reduxDevToolsOptions);
        timeWarpState.reduxDevTools = reduxDevTools;
        window[ReduxDevToolsName] = reduxDevTools;
    }
    return window[ReduxDevToolsName].IsEnabled;
}
function initializeEnvironment() {
    log("TimeWarp.State", "initializeEnvironment", "info", LogAction.Begin);
    window[InitializeJavaScriptInteropName] = InitializeJavaScriptInterop;
    window[ReduxDevToolsFactoryName] = ReduxDevToolsFactory;
    window[TimeWarpStateName] = timeWarpState;
    log("TimeWarp.State", "initializeEnvironment", "info", LogAction.End);
}
export function beforeWebStart(_options, _extensions) {
    log("TimeWarp.State Web", "beforeWebStart", "info", LogAction.Begin);
    initializeEnvironment();
}
export function afterWebStarted(_blazor) {
    log("TimeWarp.State Web", "afterWebStarted", "info", LogAction.End);
}
export function beforeWebAssemblyStart(_options, _extensions) {
    log("TimeWarp.State WebAssembly", "beforeWebAssemblyStart", "info", LogAction.Begin);
    initializeEnvironment();
}
export function afterWebAssemblyStarted(_blazor) {
    log("TimeWarp.State WebAssembly", "afterWebAssemblyStarted", "info", LogAction.End);
}
export function beforeServerStart(_options, _extensions) {
    log("TimeWarp.State Server", "beforeServerStart", "info", LogAction.Begin);
    initializeEnvironment();
}
export function afterServerStarted(_blazor) {
    log("TimeWarp.State Server", "afterServerStarted", "info", LogAction.End);
}
//# sourceMappingURL=TimeWarp.State.lib.module.js.map