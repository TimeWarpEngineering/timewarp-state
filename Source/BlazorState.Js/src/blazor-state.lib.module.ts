import { BlazorState } from './BlazorState.js';
import { ReduxDevTools } from './ReduxDevTools.js';
import {
  BlazorStateName,
  InitializeJavaScriptInteropName,
  JsonRequestHandlerName,
  ReduxDevToolsFactoryName,
  ReduxDevToolsName,
} from './Constants.js';

function InitializeJavaScriptInterop(JsonRequestHandler) {
  console.log("InitializeJavaScriptInterop");
  window[JsonRequestHandlerName] = JsonRequestHandler;
};

function Initialize() {
  console.log("Initialize BlazorState");
  if (typeof window !== 'undefined' && !window[BlazorStateName]) {
    window[BlazorStateName] = new BlazorState();
    window[InitializeJavaScriptInteropName] = InitializeJavaScriptInterop;
    window[ReduxDevToolsFactoryName] = ReduxDevToolsFactory;
  }
}

function ReduxDevToolsFactory(): boolean {
  const reduxDevTools = new ReduxDevTools();
  window[ReduxDevToolsName] = reduxDevTools;
  return reduxDevTools.IsEnabled;
}

export function beforeStart(options, extensions) {
  Initialize();
  console.log("****beforeStart timewarp-state ****");
}

export function afterStarted(blazor) {
  console.log("****afterStarted  timewarp-state ****");
}
