const logStyle = {
  info: "color: blue; font-weight: bold;",
  success: "color: green; font-weight: bold;",
  warning: "color: orange; font-weight: bold;",
  error: "color: red; font-weight: bold;",
  function: "color: darkorchid; font-weight: bold;",
};

const logTag = (tag) => `%c${tag}:`;

const log = (tag, message, level = "info") => {
  console.log(logTag(tag) + `%c ${message}`, logStyle[level], "color: black;");
};

import { blazorState } from '/_content/Blazor-State/js/BlazorState.js'

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
