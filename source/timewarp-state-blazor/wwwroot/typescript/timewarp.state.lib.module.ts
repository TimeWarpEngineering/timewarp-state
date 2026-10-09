// noinspection JSUnusedLocalSymbols,JSUnusedGlobalSymbols

import {timeWarpState} from './timewarp-state.js';
import {ReduxDevTools} from './redux-dev-tools.js';
import {log, LogAction} from './logger.js'
import {
  TimeWarpStateName,
  InitializeJavaScriptInteropName,
  ReduxDevToolsFactoryName,
  ReduxDevToolsName,
} from './constants.js';
import { DotNetReference } from './dot-net-reference.js';

function InitializeJavaScriptInterop(JsonRequestHandler: DotNetReference) {
  log("TimeWarp.State","InitializeJavaScriptInterop","info");
  timeWarpState.jsonRequestHandler = JsonRequestHandler;
}

function ReduxDevToolsFactory(reduxDevToolsOptions: any): boolean {
  log("TimeWarp.State","ReduxDevToolsFactory","info");

  if (!window[ReduxDevToolsName]) {
    const reduxDevTools = new ReduxDevTools(reduxDevToolsOptions);
    timeWarpState.reduxDevTools = reduxDevTools;
    window[ReduxDevToolsName] = reduxDevTools;
  }

  return window[ReduxDevToolsName].IsEnabled;
}

function initializeEnvironment() {
  log("TimeWarp.State","initializeEnvironment","info", LogAction.Begin);
  window[InitializeJavaScriptInteropName] = InitializeJavaScriptInterop;
  window[ReduxDevToolsFactoryName] = ReduxDevToolsFactory;
  window[TimeWarpStateName] = timeWarpState;
  log("TimeWarp.State","initializeEnvironment","info", LogAction.End);
}

export function beforeWebStart(_options: any, _extensions: any) {
  log("TimeWarp.State Web","beforeWebStart","info", LogAction.Begin);
  initializeEnvironment();
}

export function afterWebStarted(_blazor: any) {
  log("TimeWarp.State Web","afterWebStarted","info", LogAction.End);
}

export function beforeWebAssemblyStart(_options: any, _extensions: any) {
  log("TimeWarp.State WebAssembly","beforeWebAssemblyStart","info", LogAction.Begin);
  initializeEnvironment();
}

export function afterWebAssemblyStarted(_blazor: any) {
  log("TimeWarp.State WebAssembly","afterWebAssemblyStarted","info", LogAction.End);
}

export function beforeServerStart(_options: any, _extensions: any) {
  log("TimeWarp.State Server","beforeServerStart","info", LogAction.Begin);
  initializeEnvironment();
}

export function afterServerStarted(_blazor: any) {
  log("TimeWarp.State Server","afterServerStarted","info", LogAction.End);
}
