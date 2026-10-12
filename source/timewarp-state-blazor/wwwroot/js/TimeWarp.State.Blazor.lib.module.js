import { timeWarpState } from './timewarp-state.js';
import { log, LogAction } from './logger.js';
import { TimeWarpStateName, InitializeJavaScriptInteropName, } from './constants.js';
function InitializeJavaScriptInterop(JsonRequestHandler) {
    log("TimeWarp.State", "InitializeJavaScriptInterop", "info");
    timeWarpState.jsonRequestHandler = JsonRequestHandler;
}
function initializeEnvironment() {
    log("TimeWarp.State", "initializeEnvironment", "info", LogAction.Begin);
    window[InitializeJavaScriptInteropName] = InitializeJavaScriptInterop;
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
//# sourceMappingURL=TimeWarp.State.Blazor.lib.module.js.map