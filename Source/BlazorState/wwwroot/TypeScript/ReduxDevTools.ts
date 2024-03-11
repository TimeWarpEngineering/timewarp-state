import { blazorState, BlazorState } from './BlazorState.js';
import { ReduxExtensionName, DevToolsName, ReduxDevToolsName } from './Constants.js';
import { log } from './Logger.js';
import type { Config } from '@redux-devtools/extension';
import type {ConnectResponse, ListenerMessage, ReduxDevtoolsExtension} from './ReduxDevToolsTypes';
import { Action } from 'redux';
export class ReduxDevTools {
  IsEnabled: boolean;
  DevTools: ConnectResponse;
  Extension: ReduxDevtoolsExtension;
  Config: Config;
  BlazorState: BlazorState;
  StackTrace: string | undefined;

  private IsInitialized: boolean = false;

  constructor(reduxDevToolsOptions: Config) {
    log("ReduxDevTools", "constructor", "info");
    log("ReduxDevTools", reduxDevToolsOptions.toString(), "info");

    this.BlazorState = blazorState;
    this.Config = reduxDevToolsOptions;
    if (this.Config.trace) {
      this.Config.trace = this.GetStackTraceForAction;
    }
    this.Extension = this.GetExtension();
    this.DevTools = this.GetDevTools();
    this.IsEnabled = !!this.DevTools;
    this.Init();
  }

  Init() {
    if (this.IsEnabled) {
      this.DevTools.subscribe(this.MessageHandler);
      window[DevToolsName] = this.DevTools;
    }
  }

  GetExtension():ReduxDevtoolsExtension | undefined{
    const extension = window[ReduxExtensionName];

    if (!extension) {
      log("ReduxDevTools", "Redux DevTools are not installed.", "warning");
    }
    return extension;
  }

  GetDevTools():ConnectResponse | undefined{
    const devTools :ConnectResponse = this.Extension && this.Extension.connect(this.Config);
    if (!devTools) {
      log("ReduxDevTools", "Unable to connect to Redux DevTools.", "warning");
    }
    return devTools;
  }

  MapRequestType(message: ListenerMessage<unknown,Action>): string {
    const dispatchRequests = {
      'COMMIT': undefined,
      'IMPORT_STATE': undefined,
      'JUMP_TO_ACTION': 'TimeWarp.Features.ReduxDevTools.JumpToStateRequest',
      'JUMP_TO_STATE': 'TimeWarp.Features.ReduxDevTools.JumpToStateRequest',
      'LOCK_CHANGES': undefined,
      'PAUSE_RECORDING': undefined,
      'PERFORM_ACTION': undefined,
      'RESET': undefined,
      'REORDER_ACTION': undefined,
      'ROLLBACK': undefined,
      'SET_ACTIONS_ACTIVE': undefined,
      'SWEEP': undefined,
      'TOGGLE_ACTION': undefined,
    };
    let blazorRequestType: string;
    switch (message.type) {
      case 'START':
        blazorRequestType = 'TimeWarp.Features.ReduxDevTools.StartRequest';
        break;
      case 'STOP':
        //blazorRequestType = 'TimeWarp.Features.ReduxDevTools.StopRequest';
        break;
      case 'DISPATCH':
        blazorRequestType = dispatchRequests[message.payload.type];
        break;
    }
    blazorRequestType &&
      log("ReduxDevTools", `type: ${message.type} maps to ${blazorRequestType}`, "info");

    return blazorRequestType;
  }

  MessageHandler = (message: ListenerMessage<unknown,Action>) => {
    log("ReduxDevTools", "MessageHandler", "info");
    log("ReduxDevTools", message.type, "info")

    const requestType: string = this.MapRequestType(message);
    if (requestType) { // If we don't map this type then there is nothing to dispatch just ignore.
      this.BlazorState.DispatchRequest(requestType, message).then();
    } else
      log("ReduxDevTools", `messages of type ${requestType} are currently not supported`, "warning");
  }

  // noinspection JSUnusedGlobalSymbols // Called from Blazor
  ReduxDevToolsDispatch(action: Action<string>, state: unknown, stackTrace: any) {
    if (action.type === 'init') {
      if (!this.IsInitialized) {
        this.IsInitialized = true;
        return window[DevToolsName].init(state);
      }
      return;
    }

    // Handle other actions
    window[ReduxDevToolsName].StackTrace = stackTrace;
    return (window[DevToolsName] as ConnectResponse).send(action, state);
  }

  GetStackTraceForAction(_action: any): string {
    return window[ReduxDevToolsName].StackTrace ?? "None\n  at no stack (noFile:0:0)";
  }
}
