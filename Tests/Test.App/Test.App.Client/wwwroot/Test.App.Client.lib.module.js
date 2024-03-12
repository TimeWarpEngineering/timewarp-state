import { blazorState } from '/_content/Blazor-State/js/BlazorState.js'
import { log, LogAction } from '/_content/Blazor-State/js/Logger.js'

const dispatchIncrementCountAction = () => {
  log("dispatchIncrementCountAction", "Dispatching IncrementCountAction", "function");
  const IncrementCountActionName = "Test.App.Client.Features.Counter.CounterState+IncrementCount+Action, Test.App.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
  blazorState.DispatchRequest(IncrementCountActionName, { amount: 7 });
};

export function beforeWebStart(blazor) {
  log("Interop Lifecycle Web", "Test.App.Client beforeWebStart", "info", LogAction.Begin);
  window["InteropTest"] = dispatchIncrementCountAction;
}

export function afterWebStarted(blazor) {
  log("Interop Lifecycle Web", "Test.App.Client afterWebStarted", "success", LogAction.End);
}

export function beforeWebAssemblyStart(options, extensions) {
  log("Interop Lifecycle WebAssembly", "Test.App.Client beforeWebAssemblyStart", "info", LogAction.Begin);
}

export function afterWebAssemblyStarted(blazor) {
  log("Interop Lifecycle WebAssembly", "Test.App.Client afterWebAssemblyStarted", "success", LogAction.End);
}

export function beforeServerStart(options, extensions) {
  log("Interop Lifecycle Server", "Test.App.Client beforeServerStart", "info", LogAction.Begin);
}

export function afterServerStarted(blazor) {
  log("Interop Lifecycle Server", "Test.App.Client afterServerStarted", "success", LogAction.End);
}
