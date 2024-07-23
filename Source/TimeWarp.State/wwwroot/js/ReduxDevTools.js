import { timeWarpState } from './TimeWarpState.js';
import { ReduxExtensionName, DevToolsName, ReduxDevToolsName } from './Constants.js';
import { log } from './Logger.js';
export class ReduxDevTools {
    IsEnabled;
    DevTools;
    Extension;
    Config;
    TimeWarpState;
    StackTrace;
    IsInitialized = false;
    constructor(reduxDevToolsOptions) {
        log("ReduxDevTools", JSON.stringify(reduxDevToolsOptions, null, 2), "info");
        this.TimeWarpState = timeWarpState;
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
        const dispatchRequests = {
            'COMMIT': undefined,
            'IMPORT_STATE': undefined,
            'JUMP_TO_ACTION': undefined,
            'JUMP_TO_STATE': undefined,
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
        let blazorRequestType;
        switch (message.type) {
            case 'START':
                blazorRequestType = 'TimeWarp.Features.ReduxDevTools.StartRequest';
                break;
            case 'STOP':
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
        log("ReduxDevTools", message.type, "info");
        const requestType = this.MapRequestType(message);
        if (requestType) {
            this.TimeWarpState.DispatchRequest(requestType, message).then();
        }
        else
            log("ReduxDevTools", `messages of type ${requestType} are currently not supported`, "warning");
    };
    ReduxDevToolsDispatch(action, state, stackTrace) {
        if (action.type === 'init') {
            if (!this.IsInitialized) {
                this.IsInitialized = true;
                return window[DevToolsName].init(state);
            }
            return;
        }
        window[ReduxDevToolsName].StackTrace = stackTrace;
        return window[DevToolsName].send(action, state);
    }
    GetStackTraceForAction(_action) {
        return window[ReduxDevToolsName].StackTrace ?? "None\n  at no stack (noFile:0:0)";
    }
}
//# sourceMappingURL=ReduxDevTools.js.map