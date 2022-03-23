import { blazorState } from '/_content/Blazor-State/js/BlazorState.js'

const dispatchIncrementCountAction = () => {
  console.log("%cdispatchIncrementCountAction", "color: green");
  const IncrementCountActionName = "TestApp.Client.Features.Counter.CounterState+IncrementCounterAction, TestApp.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
  //const blazorState = window["BlazorState"];
  blazorState.DispatchRequest(IncrementCountActionName, { amount: 7 });
};

function initialize() {
  console.log("Initialize InteropTest.js");
  window["InteropTest"] = dispatchIncrementCountAction;
}

export function beforeStart(options, extensions) {
  initialize();
  console.log("****beforeStart TestApp.Client ****");
}

export function afterStarted(blazor) {
  console.log("****afterStarted  TestApp.Client ****");
}
