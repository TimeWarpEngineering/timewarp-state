import { timeWarpState } from '/_content/TimeWarp.State/js/timewarp-state.js'
import { log, LogAction } from '/_content/TimeWarp.State/js/logger.js'

const waitForJsonRequestHandler = async (timeoutMs = 10000) => {
  const deadline = Date.now() + timeoutMs;
  while (!timeWarpState.jsonRequestHandler) {
    if (Date.now() >= deadline) {
      throw new Error('TimeWarpState.jsonRequestHandler is not initialized after waiting');
    }
    await new Promise((resolve) => setTimeout(resolve, 50));
  }
};

const dispatchIncrementCountAction = async () => {
  log("dispatchIncrementCountAction", "Dispatching IncrementCountAction", "function");
  const IncrementCountActionName = "Test.App.Client.Features.Counter.CounterState+IncrementCountActionSet+Action, Test.App.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
  if (!timeWarpState.jsonRequestHandler) {
    await waitForJsonRequestHandler();
  }
  await timeWarpState.DispatchRequest(IncrementCountActionName, { amount: 7 });
};

const registerInteropTest = () => {
  window["InteropTest"] = dispatchIncrementCountAction;
};

export function beforeWebStart(blazor) {
  log("Interop Lifecycle Web", "Test.App.Client beforeWebStart", "info", LogAction.Begin);
  registerInteropTest();
}

export function afterWebStarted(blazor) {
  log("Interop Lifecycle Web", "Test.App.Client afterWebStarted", "success", LogAction.End);
}

export function beforeWebAssemblyStart(options, extensions) {
  log("Interop Lifecycle WebAssembly", "Test.App.Client beforeWebAssemblyStart", "info", LogAction.Begin);
  registerInteropTest();
}

export function afterWebAssemblyStarted(blazor) {
  log("Interop Lifecycle WebAssembly", "Test.App.Client afterWebAssemblyStarted", "success", LogAction.End);
}

export function beforeServerStart(options, extensions) {
  log("Interop Lifecycle Server", "Test.App.Client beforeServerStart", "info", LogAction.Begin);
  registerInteropTest();
}

export function afterServerStarted(blazor) {
  log("Interop Lifecycle Server", "Test.App.Client afterServerStarted", "success", LogAction.End);
}
