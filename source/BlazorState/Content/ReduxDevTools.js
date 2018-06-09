class ReduxDevTools {
  constructor() {
    this.BlazorState = new BlazorState(); // Depends on this functionality
    this.Config = {
      name: 'Blazor State',
      features: {
        pause: false, // start/pause recording of dispatched actions
        lock: false, // lock/unlock dispatching actions and side effects
        persist: false, // persist states on page reloading
        export: false, // export history of actions in a file
        import: false, // import history of actions from a file
        jump: true, // jump back and forth (time traveling)
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
      this.DevTools.subscribe(ReduxDevTools.MessageHandler);

      Blazor.registerFunction(ReduxDevTools.ReduxDevToolsDispatch.name, ReduxDevTools.ReduxDevToolsDispatch);
      console.log(`${ReduxDevTools.ReduxDevToolsDispatch.name} function registered with Blazor`);

      window.devTools = this.DevTools;
    }
  }

  GetExtension() {
    const extension = window.__REDUX_DEVTOOLS_EXTENSION__;

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

  static MapRequestType(message) {
    var dispatchRequests = {
      'COMMIT': undefined,
      'IMPORT_STATE': undefined,
      'JUMP_TO_ACTION': 'BlazorState.Behaviors.ReduxDevTools.Features.JumpToState.JumpToStateRequest',
      'JUMP_TO_STATE': 'BlazorState.Behaviors.ReduxDevTools.Features.JumpToState.JumpToStateRequest',
      'RESET': undefined,
      'ROLLBACK': undefined,
      'TOGGLE_ACTION': undefined
    };
    var blazorRequestType;
    switch (message.type) {
      case 'START':
        blazorRequestType = 'BlazorState.Behaviors.ReduxDevTools.Features.Start.StartRequest';
        break;
      case 'STOP':
        //blazorRequestType = 'BlazorState.Behaviors.ReduxDevTools.Features.Stop.StopRequest';
        break;
      case 'DISPATCH':
        blazorRequestType = dispatchRequests[message.payload.type];
        break;
    }
    blazorRequestType &&
      console.log(`Redux Dev tools type: ${message.type} maps to ${blazorRequestType}`);

    return blazorRequestType;
  }

  static MessageHandler(message) {
    console.log('ReduxDevTools.MessageHandler');
    console.log(message);
    var jsonRequest;
    const requestType = ReduxDevTools.MapRequestType(message);
    if (requestType) { // If we don't map this type then there is nothing to dispatch just ignore.
      jsonRequest = {
        // TODO: make sure non Requests from assemblies other than BlazorState also work.
        //RequestType: 'BlazorState.Behaviors.DevTools.Features.Start.ReduxDevToolsStartRequest, BlazorState',
        //RequestType: 'BlazorState.Behaviors.DevTools.Features.Start.Request',
        RequestType: requestType,
        Payload: message
      };

      reduxDevTools.BlazorState.DispatchRequest(jsonRequest);
    } else
      console.log(`messages of this type are currently not supported`);
  }

  static ReduxDevToolsDispatch(action, state) {
    if (action === 'init') {
      console.log("ReduxDevTools.js: Dispatching redux action: init");
      return window.devTools.init(state);
    }
    else {
      console.log("ReduxDevTools.js: Dispatching redux action");
      console.log(action);
      return window.devTools.send(action, state);
    }
  }
}

var reduxDevTools;

Blazor.registerFunction('blazor-state.ReduxDevTools.create', function () {
  console.log('js - blazor-state.ReduxDevTools.create');
  reduxDevTools = new ReduxDevTools();
  return reduxDevTools.IsEnabled;
});