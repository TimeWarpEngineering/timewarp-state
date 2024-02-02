import { blazorState } from '/_content/Blazor-State/js/BlazorState.js'
import { log } from '/_content/Blazor-State/js/Logger.js'

const dispatchIncrementCountAction = () => {
  log("dispatchIncrementCountAction", "Dispatching IncrementCountAction", "function");
  const IncrementCountActionName = "Test.App.Client.Features.Counter.CounterState+IncrementCounterAction, Test.App.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
  blazorState.DispatchRequest(IncrementCountActionName, { amount: 7 });
};

export function beforeStart(options, extensions) {
  log("Interop Lifecycle", "Test.App.Client beforeStart", "info");
  window["InteropTest"] = dispatchIncrementCountAction;
}

export function afterStarted(blazor) {
  log("Interop Lifecycle", "Test.App.Client afterStarted", "success");
}

export function afterWebStarted(blazor) {
  log("Interop Lifecycle", "Test.App.Client afterWebStarted", "success");
}

export function beforeWebAssemblyStart(options, extensions) {
  log("Interop Lifecycle", "Test.App.Client beforeWebAssemblyStart", "warning");
}

export function afterWebAssemblyStarted(blazor) {
  log("Interop Lifecycle", "Test.App.Client afterWebAssemblyStarted", "success");
}

export function beforeServerStart(options, extensions) {
  log("Interop Lifecycle", "Test.App.Client beforeServerStart", "warning");
}
