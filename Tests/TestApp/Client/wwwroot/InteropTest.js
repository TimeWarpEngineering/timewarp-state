const dispatchIncrementCountAction = () => {
  console.log("%dispatchIncrementCountAction", "color: green");
  const IncrementCountActionName = "TestApp.Client.Features.Counter.CounterState+IncrementCounterAction, TestApp.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
  const blazorState = window["BlazorState"];
  blazorState.DispatchRequest(IncrementCountActionName, { amount: 7 });
};

function initialize() {
  console.log("Initialize InteropTest.js");
  window["InteropTest"] = dispatchIncrementCountAction;
}

initialize();