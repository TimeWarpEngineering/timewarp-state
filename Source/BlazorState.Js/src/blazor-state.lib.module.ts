import { blazorState } from './BlazorState.js';
import { ReduxDevTools } from './ReduxDevTools.js';
import {
  InitializeJavaScriptInteropName,
  ReduxDevToolsFactoryName,
  ReduxDevToolsName,
} from './Constants.js';

function InitializeJavaScriptInterop(JsonRequestHandler) {
  console.log("InitializeJavaScriptInterop");
  blazorState.jsonRequestHandler = JsonRequestHandler;
};

function ReduxDevToolsFactory(): boolean {
  const reduxDevTools = new ReduxDevTools();
  window[ReduxDevToolsName] = reduxDevTools;
  return reduxDevTools.IsEnabled;
}

export function beforeStart(options, extensions) {
  console.log("****beforeStart timewarp-state ****");
  window[InitializeJavaScriptInteropName] = InitializeJavaScriptInterop;
  window[ReduxDevToolsFactoryName] = ReduxDevToolsFactory;
}

export function afterStarted(blazor) {
  console.log("****afterStarted  timewarp-state ****");
}
