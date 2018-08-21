import { BlazorState, BlazorStateName } from './BlazorState';
import { ReduxDevTools, ReduxDevToolsName } from './ReduxDevTools';

const ReduxDevToolsFactoryName: string = 'reduxDevToolsFactory';

function initialize() {
  console.log("Initialize BlazorState");
  if (typeof window !== 'undefined' && !window[BlazorStateName]) {
    window[BlazorStateName] = new BlazorState();
    window[ReduxDevToolsFactoryName] = reduxDevToolsFactory;
  }
}

function reduxDevToolsFactory(): boolean {
  console.log('js - ReduxDevTools.Create');
  const reduxDevTools = new ReduxDevTools();
  window[ReduxDevToolsName] = reduxDevTools;
  return reduxDevTools.IsEnabled;
}

initialize();