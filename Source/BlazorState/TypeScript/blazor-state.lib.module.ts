import { blazorState } from './BlazorState.js';
import { ReduxDevTools } from './ReduxDevTools.js';
import {
  BlazorStateName,
  InitializeJavaScriptInteropName,
  ReduxDevToolsFactoryName,
  ReduxDevToolsName,
} from './Constants.js';

function InitializeJavaScriptInterop(JsonRequestHandler) {
  console.log("timewarp blazor-state InitializeJavaScriptInterop");
  blazorState.jsonRequestHandler = JsonRequestHandler;
};

function ReduxDevToolsFactory(reduxDevToolsOptions): boolean {
  console.log("timewarp blazor-state ReduxDevToolsFactory");

  if (!window[ReduxDevToolsName]) {
    const reduxDevTools = new ReduxDevTools(reduxDevToolsOptions);
    blazorState.reduxDevTools = reduxDevTools;
    window[ReduxDevToolsName] = reduxDevTools;
  }

  return window[ReduxDevToolsName].IsEnabled;
}

// These will be used by dotnet 7
//export function beforeStart(options, extensions) {
// console.log("timewarp blazor-state beforeStart");
// window[InitializeJavaScriptInteropName] = InitializeJavaScriptInterop;
// window[ReduxDevToolsFactoryName] = ReduxDevToolsFactory;
// window[BlazorStateName] = blazorState;
//}

//export function afterStarted(blazor) {
// console.log("timewarp blazor-state afterStarted");
//}

// ====================

export function beforeWebStart(options, extensions) {
  console.log("timewarp blazor-state beforeWebStart 2");
  window[InitializeJavaScriptInteropName] = InitializeJavaScriptInterop;
  window[ReduxDevToolsFactoryName] = ReduxDevToolsFactory;
  window[BlazorStateName] = blazorState;
}

//export function afterWebStarted(blazor) {
//  console.log("timewarp blazor-state afterWebStarted");
//}

//export function beforeWebAssemblyStart(options, extensions) {
//  console.log("timewarp blazor-state beforeWebAssemblyStart");
//}

//export function afterWebAssemblyStarted(blazor) {
//  console.log("timewarp blazor-state afterWebAssemblyStarted");
//}

//export function beforeServerStart(options, extensions) {
//  console.log("timewarp blazor-state beforeServerStart");
//}

//export function afterServerStarted(blazor) {
//  console.log("timewarp blazor-state afterServerStarted");
//}
