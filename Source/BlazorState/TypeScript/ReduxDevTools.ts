import { blazorState, BlazorState } from './BlazorState.js';
import { ReduxExtensionName, DevToolsName, ReduxDevToolsName } from './Constants.js';
import { log } from './Logger.js';

type traceType = (action) => string;
//see reduxjs/redux-devtools/packages/redux-devtools-extension/src/index.ts for types
export class ReduxDevTools {
  IsEnabled: boolean;
  DevTools: any;
  Extension: any;
  Config: {
    name: string;
    trace: boolean | traceType;
    features: {
      pause: boolean;
      lock: boolean;
      persist: boolean;
      export: boolean;
      import: boolean;
      jump: boolean;
      skip: boolean;
      reorder: boolean;
      dispatch: boolean;
      test: boolean;
    };
  };
  BlazorState: BlazorState;
  StackTrace: string | undefined;

  private IsInitialized: boolean = false;

  constructor(reduxDevToolsOptions) {
    log("ReduxDevTools", "constructor", "info");
    log("ReduxDevTools", reduxDevToolsOptions.toString(), "info");

    this.BlazorState = blazorState;
    this.Config = reduxDevToolsOptions;
    if (this.Config.trace) {
      this.Config.trace = this.GetStackTraceForAction;
    }
    this.Extension = this.GetExtension();
    this.DevTools = this.GetDevTools();
    this.IsEnabled = this.DevTools ? true : false;
    this.Init();
  }

  Init() {
    if (this.IsEnabled) {
      this.DevTools.subscribe(this.MessageHandler);
      window[DevToolsName] = this.DevTools;
    }
  }

  GetExtension() {
    const extension = window[ReduxExtensionName];

    if (!extension) {
      log("ReduxDevTools", "Redux DevTools are not installed.", "warning");
    }
    return extension;
  }

  GetDevTools() {
    const devTools = this.Extension && this.Extension.connect(this.Config);
    if (!devTools) {
      log("ReduxDevTools", "Unable to connect to Redux DevTools.", "warning");
    }
    return devTools;
  }

  MapRequestType(message) {
    var dispatchRequests = {
      'COMMIT': undefined,
      'IMPORT_STATE': undefined,
      'JUMP_TO_ACTION': 'BlazorState.Pipeline.ReduxDevTools.JumpToStateRequest',
      'JUMP_TO_STATE': 'BlazorState.Pipeline.ReduxDevTools.JumpToStateRequest',
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
    var blazorRequestType;
    switch (message.type) {
      case 'START':
        blazorRequestType = 'BlazorState.Pipeline.ReduxDevTools.StartRequest';
        break;
      case 'STOP':
        //blazorRequestType = 'BlazorState.Pipeline.ReduxDevTools.StopRequest';
        break;
      case 'DISPATCH':
        blazorRequestType = dispatchRequests[message.payload.type];
        break;
    }
    blazorRequestType &&
      log("ReduxDevTools", `type: ${message.type} maps to ${blazorRequestType}`, "info");

    return blazorRequestType;
  }

  MessageHandler = (message) => {
    log("ReduxDevTools", "MessageHandler", "info");
    log("ReduxDevTools", message, "info")

    let jsonRequest;
    const requestType = this.MapRequestType(message);
    if (requestType) { // If we don't map this type then there is nothing to dispatch just ignore.
      jsonRequest = {
        // TODO: make sure non Requests from assemblies other than BlazorState also work.
        RequestType: requestType,
        Payload: message
      };
      
      this.BlazorState.DispatchRequest(requestType, message);
    } else
      log("ReduxDevTools", `messages of this type are currently not supported`, "warning");
  }

  ReduxDevToolsDispatch(action, state, stackTrace) {
    if (action === 'init') {
      if (!this.IsInitialized) {
        this.IsInitialized = true;
        return window[DevToolsName].init(state);
      }
      return;
    }

    // Handle other actions
    window[ReduxDevToolsName].StackTrace = stackTrace;
    return window[DevToolsName].send(action, state);
  }


  GetStackTraceForAction(action): string {
    return window[ReduxDevToolsName].StackTrace ?? "None\n  at no stack (nofile:0:0)";
  }
}
