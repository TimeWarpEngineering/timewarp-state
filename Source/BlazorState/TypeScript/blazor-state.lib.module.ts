import {blazorState} from './BlazorState.js';
import {ReduxDevTools} from './ReduxDevTools.js';
import {log, LogAction} from './Logger.js'
import {
  BlazorStateName,
  InitializeJavaScriptInteropName,
  ReduxDevToolsFactoryName,
  ReduxDevToolsName,
} from './Constants.js';

function InitializeJavaScriptInterop(JsonRequestHandler) {
  log("TimeWarp.State","InitializeJavaScriptInterop","info");
  blazorState.jsonRequestHandler = JsonRequestHandler;
};

function ReduxDevToolsFactory(reduxDevToolsOptions): boolean {
  log("TimeWarp.State","ReduxDevToolsFactory","info");

  if (!window[ReduxDevToolsName]) {
    const reduxDevTools = new ReduxDevTools(reduxDevToolsOptions);
    blazorState.reduxDevTools = reduxDevTools;
    window[ReduxDevToolsName] = reduxDevTools;
  }

  return window[ReduxDevToolsName].IsEnabled;
}

// These will be used by dotnet 7
// export function beforeStart(options, extensions) {
// log("TimeWarp.State","beforeStart","info", LogAction.Begin);
// }
//
// export function afterStarted(blazor) {
//   log("TimeWarp.State","afterStarted","info", LogAction.End);
// }

// ====================

export function beforeWebStart(options, extensions) {
  log("TimeWarp.State Web","beforeWebStart","info", LogAction.Begin);
  window[InitializeJavaScriptInteropName] = InitializeJavaScriptInterop;
  window[ReduxDevToolsFactoryName] = ReduxDevToolsFactory;
  window[BlazorStateName] = blazorState;
}

export function afterWebStarted(blazor) {
  log("TimeWarp.State Web","afterWebStarted","info", LogAction.End);
}

export function beforeWebAssemblyStart(options, extensions) {
  log("TimeWarp.State WebAssembly","beforeWebAssemblyStart","info", LogAction.Begin);
}

export function afterWebAssemblyStarted(blazor) {
  log("TimeWarp.State WebAssembly","afterWebAssemblyStarted","info", LogAction.End);
}

export function beforeServerStart(options, extensions) {
  log("TimeWarp.State Server","beforeServerStart","info", LogAction.Begin);
}

export function afterServerStarted(blazor) {
  log("TimeWarp.State Server","afterServerStarted","info", LogAction.End);
}
