import { BlazorState } from './BlazorState';
import { BlazorStateName, ReduxExtensionName, DevToolsName, ReduxDevToolsName } from './Constants';

export class ReduxDevTools {
  IsEnabled: boolean;
  DevTools: any;
  Extension: any;
  Config: { name: string; features: { pause: boolean; lock: boolean; persist: boolean; export: boolean; import: boolean; jump: boolean; skip: boolean; reorder: boolean; dispatch: boolean; test: boolean; }; };
  BlazorState: BlazorState;

  constructor() {
    this.BlazorState = window[BlazorStateName]; // Depends on this functionality
    this.Config = {
      name: 'Blazor State',
      features: {
        pause: false, // start/pause recording of dispatched actions
        lock: false, // lock/unlock dispatching actions and side effects
        persist: false, // persist states on page reloading
        export: false, // export history of actions in a file
        import: false, // import history of actions from a file
        jump: false, // jump back and forth (time traveling)
        skip: false, // skip (cancel) actions
        reorder: false, // drag and drop actions in the history list
        dispatch: false, // dispatch custom actions or action creators
        test: false // generate tests for the selected actions
      }
    };
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
      console.log('Redux DevTools are not installed.');
    }
    return extension;
  }

  GetDevTools() {
    const devTools = this.Extension && this.Extension.connect(this.Config);
    if (!devTools) {
      console.log('Unable to connect to Redux DevTools.');
    }
    return devTools;
  }

  MapRequestType(message) {
    var dispatchRequests = {
      'COMMIT': undefined,
      'IMPORT_STATE': undefined,
      'JUMP_TO_ACTION': 'BlazorState.Pipeline.ReduxDevTools.JumpToStateRequest',
      'JUMP_TO_STATE': 'BlazorState.Pipeline.ReduxDevTools.JumpToStateRequest',
      'RESET': undefined,
      'ROLLBACK': undefined,
      'TOGGLE_ACTION': undefined
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
      console.log(`Redux Dev tools type: ${message.type} maps to ${blazorRequestType}`);

    return blazorRequestType;
  }

  MessageHandler = (message) => {
    console.log('ReduxDevTools.MessageHandler');
    console.log(message);
    var jsonRequest;
    const requestType = this.MapRequestType(message);
    if (requestType) { // If we don't map this type then there is nothing to dispatch just ignore.
      jsonRequest = {
        // TODO: make sure non Requests from assemblies other than BlazorState also work.
        RequestType: requestType,
        Payload: message
      };

      this.BlazorState.DispatchRequest(requestType, message);
    } else
      console.log(`messages of this type are currently not supported`);
  }

  ReduxDevToolsDispatch(action, state) {
    if (action === 'init') {
      return window[DevToolsName].init(state);
    }
    else {
      return window[DevToolsName].send(action, state);
    }
  }
}