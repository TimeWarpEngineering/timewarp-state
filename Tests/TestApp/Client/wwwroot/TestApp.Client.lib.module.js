import { blazorState } from '/_content/Blazor-State/js/BlazorState.js'

const dispatchIncrementCountAction = () => {
  console.log("%cdispatchIncrementCountAction", "color: green");
  const IncrementCountActionName = "TestApp.Client.Features.Counter.CounterState+IncrementCounterAction, TestApp.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
  blazorState.DispatchRequest(IncrementCountActionName, { amount: 7 });
};

export function beforeStart(options, extensions) {
  console.log("****beforeStart TestApp.Client ****");
  window["InteropTest"] = dispatchIncrementCountAction;
}

export function afterStarted(blazor) {
  console.log("****afterStarted  TestApp.Client ****");
}
